import atexit
import freenect
import socket
import select
import sys
import time
from threading import Thread

import gym
import numpy as np
from gym import spaces
from gym.utils import seeding

from eyerobot_gym.config import *
from eyerobot_gym.utils.bytebuffer import ByteBuffer

from eyerobot_gym.Policy import *


class EyeRobotEnv(gym.Env):
    metadata = {'render.modes': ['human']}

    episode = 0
    step_count = 0

    kill_kinect = False

    current_depth_frame = None
    current_rgb_frame = None
    depth_frame_timestamp = 0
    rgb_frame_timestamp = 0

    last_action = 0
    rotation = (0, 0, 0, 0)
    position = (0, 0, 0)

    policy = EagerScorePolicy()

    acceleration = [0, 0, 0]
    velocity = [0, 0, 0]
    new_distance = 0
    first_distance = 0
    distance_count = 0
    socket = None
    connected = False
    server_address = (IP, PORT)
    read_thread = None
    kinect_thread = None
    current_key = 0
    packet_map = [None, None, None, None, None, None, None, None, None, None, None, None, None, None, None, None]

    def on_score(self, packet):
        tmp = packet.read(4 * 8)
        tmp_buf = ByteBuffer(tmp, 0, 4 * 8)

        distance = tmp_buf.get_LFloat32()
        rx = tmp_buf.get_LFloat32()
        ry = tmp_buf.get_LFloat32()
        rz = tmp_buf.get_LFloat32()
        rw = tmp_buf.get_LFloat32()

        px = tmp_buf.get_LFloat32()
        py = tmp_buf.get_LFloat32()
        pz = tmp_buf.get_LFloat32()

        self.rotation = (rx, ry, rz, rw)
        self.position = (px, py, pz)

        print("Got rotation " + str(self.rotation))

        self.new_distance = distance
        self.distance_count += 1

    def send_depth(self, dev, data, timestamp):
        self.current_depth_frame = data
        self.depth_frame_timestamp = timestamp

    def send_rgb(self, dev, data, timestamp):
        self.current_rgb_frame = data
        self.current_rgb_frame = timestamp

    def body(self, dev, *args):
        acc = freenect.get_accel(dev)

        accX = int(acc[0])
        accY = int(acc[1])
        accZ = int(acc[2])

        accX = accX - self.acceleration[0]
        accY = accY - self.acceleration[1]
        accZ = accZ - self.acceleration[2]

        self.velocity[0] += accX
        self.velocity[1] += accY
        self.velocity[2] += accZ

        self.acceleration[0] = int(acc[0])
        self.acceleration[1] = int(acc[1])
        self.acceleration[2] = int(acc[2])

        if self.connected:
            self.send_kinect_info(self.velocity, [accX, accY, accZ])

        if self.kill_kinect:
            freenect.shutdown(dev)

    """EyeRobot OpenAI Gym
        
        The object of the game for the robot is to get as close to the goal as possible
        within 200 steps
        
        Each step sends a command to the robot W/A/S/D that is executed for 1 second before the next step
        
        After each step the agent is provided with one of four possible observations
        which indicate where the robot is in relation to the randomly chosen goal
        0 - No movement yet submitted (only after reset)
        1 - Robot is further away from target than last step
        2 - Robot has not made any progress towards the goal
        3 - Robot is closer to target than last step
        
        The rewards are:
        0   - if the robot hasn't gotten close to the goal or hasn't moved
        0.5 - if the robot has gotten closer to the goal
        1   - if the robot got to the goal
        
        The episode terminates after the robot reaches the goal or 200 steps have been taken
        
        The robot will need to use a memory of previously submitted actions and observations
        in order to efficiently explore the available actions
        
        The purpose is to have agents optimise their exploration parameters (e.g. how far to
        explore from previous actions) based on previous experience.
        """
    def __init__(self):
        self.action_space = spaces.Discrete(5)
        self.observation_space = spaces.Discrete(4)

        self.observation = [0, 0, 0, 0]

        self.packet_map[0x06] = self.on_score

        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        except socket.error:
            print('Failed to create socket')
            sys.exit()

        print("init")

        self.connect()
        self.seed()
        self.reset()

    def seed(self, seed=None):
        self.np_random, seed = seeding.np_random(seed)
        return [seed]

    def send_kinect_info(self, velocity, acceleration):
        buf = ByteBuffer(bytearray([0] * 24), 0, 24)
        buf.put_SLInt32(acceleration[0])
        buf.put_SLInt32(acceleration[1])
        buf.put_SLInt32(acceleration[2])
        buf.put_SLInt32(velocity[0])
        buf.put_SLInt32(velocity[1])
        buf.put_SLInt32(velocity[2])

        arr = bytearray([0] * 25)
        arr[0] = 0x04
        buf.set_position(0)
        buf.get(arr, 1, 24)

        self.socket.send(arr)

    def send_game_reset(self):
        buf = ByteBuffer(bytearray([0] * 8), 0, 8)
        buf.put_SLInt32(self.episode)
        buf.put_SLInt32(self.step_count)

        arr = bytearray([0] * 9)
        arr[0] = 0x01
        buf.set_position(0)
        buf.get(arr, 1, 8)

        self.socket.send(arr)

    def send_key_event(self, key, action):
        buf = ByteBuffer(bytearray([0] * 4), 0, 4)
        buf.put_SLInt32(key)

        arr = bytearray([0] * 5)
        arr[0] = 0x02 if action == 1 else 0x03
        buf.set_position(0)
        buf.get(arr, 1, 4)

        self.socket.send(arr)

    def request_score(self):
        arr = [0x05, 1, 1, 1]
        to_send = bytearray(arr)

        self.socket.send(to_send)

    def start_kinect(self):
        if ENABLE_KINECT:
            freenect.runloop(depth=self.send_depth,
                             video=self.send_rgb,
                             body=self.body)
        else:
            self.current_depth_frame = np.empty((640, 480))
            self.current_rgb_frame = np.empty((640, 480))

    def connect(self):
        self.socket.connect(self.server_address)

        self.connected = True

        self.read_thread = Thread(target=self.start_reading)
        self.read_thread.start()

        self.kinect_thread = Thread(target=self.start_kinect)
        self.kinect_thread.start()

        atexit.register(self.disconnect)

    def disconnect(self):
        self.connected = False

        self.socket.close()

    def update_observation(self, state):
        rx, ry, rz, rw = self.rotation
        px, py, pz = self.position

        self.observation = [state, self.new_distance, self.last_action, self.step_count, rx, ry, rz, rw]

    def step(self, action):
        assert self.action_space.contains(action)

        # Get Unity Key from action
        key = 0
        if action == 0:
            key = W_KEY
        elif action == 1:
            key = A_KEY
        elif action == 2:
            key = S_KEY
        else:
            key = D_KEY

        print("First step? " + str((self.new_distance == 0)) + "\tDistance: " + str(self.new_distance))
        first_step = self.new_distance == 0

        cur_score = self.distance_count
        prev_distance = self.new_distance
        prev_timestamp = self.depth_frame_timestamp

        #if self.current_key != 0:
        #    print("Releasing previous key")
        #    self.send_key_event(self.current_key, 0)

        #print("Sending key press " + str(key) + " : " + str(action))

        #if key != 0:

        # Send Key Press to Unity
        self.send_key_event(key, 1)

        time.sleep(KEY_DELAY)

        self.send_key_event(key, 0)

        # Remember previous key
        self.current_key = key

        print("Requesting score")
        # Request the current score from Unity
        self.request_score()

        while cur_score == self.distance_count:
            print("Waiting for score..")
            time.sleep(SCORE_DELAY)

        if ENABLE_KINECT:
            while prev_timestamp == self.depth_frame_timestamp:
                print("Waiting for new frame..")
                time.sleep(SCORE_DELAY)

        print("Calculating observation and reward")
        difference = prev_distance - self.new_distance

        if first_step:
            self.first_distance = self.new_distance
            difference = 0

        self.step_count += 1

        reward, done = self.policy.calculate(self, difference)

        print "Got reward " + str(reward)

        to_return = [self.observation, self.current_depth_frame] if ENABLE_KINECT else [self.observation]

        return to_return, reward, done, {"distance": self.new_distance, "steps": self.step_count}

    def reset(self):
        self.send_game_reset()

        self.step_count = 0
        self.observation = [0, 0, 0, 0, 0, 0, 0, 0]
        self.distance_count = 0
        self.new_distance = 0
        self.first_distance = 0
        self.episode += 1
        self.policy.reset()

        if self.current_key != 0:
            self.send_key_event(self.current_key, 0)

        while self.current_depth_frame is None:
            print("Waiting for frame..")
            time.sleep(SCORE_DELAY)

        return [self.observation, self.current_depth_frame]

    def safe_read(self, count):
        arr = bytearray()

        # dataInSocket, _, _ = select.select([self.socket], [], [])
        # print(dataInSocket)

        try:
            r, _, _ = select.select([self.socket], [], [])

            while len(arr) < count and self.connected:
                tmp, _ = self.socket.recvfrom(count)
                tmp2 = bytearray(tmp)
                arr.extend(tmp2)
        except:
            if self.connected:
                print("Failed to read from socket!")
            else:
                print("Socket shutdown")

        return arr

    def close(self):
        self.disconnect()
        self.kill_kinect = True
        self.current_depth_frame = None
        self.current_rgb_frame = None
        self.kinect_thread.join(1000)
        self.read_thread.join(1000)

    def start_reading(self):
        while self.connected:
            print "Waiting for command.."

            header_arr = self.safe_read(1)

            if len(header_arr) == 0:
                continue

            op_code = header_arr[0]

            packet = type('', (object,), {
                "op_code": op_code,
                "read": self.safe_read
            })()

            if self.packet_map[op_code] is not None:
                packet_processor = self.packet_map[op_code]

                if callable(packet_processor):
                    packet_processor(packet)
                else:
                    print "No processor for " + str(op_code)
            else:
                print "Unknown op_code " + str(op_code)
