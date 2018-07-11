import socket
import sys
import atexit
from threading import Thread
from utils.bytebuffer import ByteBuffer
from config import IP, PORT, BUFFER, NAME
import json
from collections import namedtuple


def _json_object_hook(d): return namedtuple('X', d.keys())(*d.values())


def json2obj(data): return json.loads(data, object_hook=_json_object_hook)


class ModuleClient:
    socket = None
    connected = False
    server_address = (IP, PORT)
    packet_number = 0
    read_thread = None
    on_info_update = None
    on_event = None
    last_ok = False
    event_callbacks = {}
    sensor_callbacks = {}
    packet_map = [None, None, None, None, None, None, None, None, None, None, None, None, None, None, None, None]

    def event_packet(self, packet):
        ignore = packet.read(4)

        event_id = packet.read(1)[0]

        tmp = packet.read(8)
        tmp_buf = ByteBuffer(tmp, 0, 8)

        owner_length = tmp_buf.get_SLInt32()
        event_length = tmp_buf.get_SLInt32()

        owner = str(packet.read(owner_length))

        event_json = str(packet.read(event_length))

        print "Got event #" + str(event_id) + " from " + owner + ". Data: " + event_json

        event_data = json2obj(event_json)

        if event_id in self.event_callbacks:
            for callback in self.event_callbacks[event_id]:
                callback(event_data, owner)

    def json_sensor_information_packet(self, packet):
        tmp = packet.read(4)

        tmp_buf = ByteBuffer(tmp, 0, 4)

        robot_length = tmp_buf.get_SLInt32()

        robot = str(packet.read(robot_length))

        tmp = packet.read(4)

        tmp_buf = ByteBuffer(tmp, 0, 4)

        json_length = tmp_buf.get_SLInt32()

        json = str(packet.read(json_length))

        info = json2obj(json)

        if robot in self.sensor_callbacks:
            for callback in self.sensor_callbacks[robot]:
                callback(robot, info)

        print packet

    def ok_packet(self, packet):
        val = packet.read(1)[0]

        self.last_ok = val == 1

        print "Got OK: " + str(self.last_ok)

    def __init__(self):
        self.packet_map[0x05] = self.event_packet
        self.packet_map[0x08] = self.json_sensor_information_packet
        self.packet_map[0x01] = self.ok_packet

        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        except socket.error:
            print 'Failed to create socket'
            sys.exit()

    def connect(self):
        self.socket.connect(self.server_address)

        self.connected = True

        self.read_thread = Thread(target=self.start_reading)
        self.read_thread.start()

        self.send_session_packet()

        atexit.register(self.disconnect)

    def disconnect(self):
        self.connected = False

        self.socket.shutdown(socket.SHUT_WR)

    def send_session_packet(self):
        arr = bytearray([0x00, 1, len(NAME)])
        arr.extend(NAME)

        self.socket.sendto(arr, self.server_address)

    def request_sensor_information(self, robot, filter, callback):
        if not callable(callback):
            print "Invalid callback!"
            return

        should_filter = filter != ""
        filter_size = 0
        if should_filter:
            filter_size = 4 + len(filter)

        total_size = 9 + len(robot) + filter_size

        array = bytearray([0] * total_size)
        buf = ByteBuffer(array, 0, total_size)

        # 4 bytes
        buf.put_SLInt32(total_size + 1)

        # 4 + 4 = 8
        buf.put_SLInt32(len(robot))

        # 4 + 4 + 1 = 9
        tmp = bytearray([0])
        if should_filter:
            tmp[0] = 1

        buf.put_bytes(tmp, 0, 1)

        temp2 = bytearray()
        temp2.extend(robot)
        buf.put_bytes(temp2, 0, len(temp2))

        if should_filter:
            buf.put_SLInt32(len(filter))

            temp3 = bytearray()
            temp3.extend(filter)

            buf.put_bytes(temp3, 0, len(temp3))

        to_send = bytearray([0] * (total_size + 1))
        to_send[0] = 0x07
        buf.set_position(0)
        buf.get(to_send, 1, total_size)

        self.socket.sendto(to_send, self.server_address)

        if robot not in self.sensor_callbacks:
            self.sensor_callbacks[robot] = list()

        self.sensor_callbacks[robot].append(callback)

    def trigger_event(self, event_id, event_data, filter):
        event = json.dumps(event_data)
        should_filter = filter != ""
        filter_size = 0
        if should_filter:
            filter_size = 4 + len(filter)

        total_size = 10 + len(event) + filter_size

        array = bytearray([0] * total_size)
        buf = ByteBuffer(array, 0, total_size)

        # 4 bytes
        buf.put_SLInt32(total_size + 1)

        # 4 + 2 = 6
        temp = bytearray([0] * 2)
        temp[0] = event_id
        if should_filter:
            temp[1] = 1
        else:
            temp[1] = 0

        buf.put_bytes(temp, 0, 2)

        # 4 + 2 + 4 = 10
        buf.put_SLInt32(len(event))

        temp2 = bytearray()
        temp2.extend(event)

        buf.put_bytes(temp2, 0, len(temp2))

        if should_filter:
            buf.put_SLInt32(len(filter))

            temp3 = bytearray()
            temp3.extend(filter)

            buf.put_bytes(temp3, 0, len(temp3))

        to_send = bytearray([0] * (total_size + 1))
        to_send[0] = 0x06
        buf.set_position(0)
        buf.get(to_send, 1, total_size)

        self.socket.sendto(to_send, self.server_address)

    def send_robot_command(self, motor1, motor2, motor3, motor4, robot_name):
        total_size = 24 + len(robot_name)
        array = bytearray([0] * total_size)
        buf = ByteBuffer(array, 0, total_size)
        buf.put_SLInt32(total_size + 1)
        buf.put_SLInt32(motor1)
        buf.put_SLInt32(motor2)
        buf.put_SLInt32(motor3)
        buf.put_SLInt32(motor4)
        buf.put_SLInt32(len(robot_name))

        temp = bytearray()
        temp.extend(robot_name)

        buf.put_bytes(temp, 0, len(temp))

        to_send = bytearray([0] * (total_size + 1))
        to_send[0] = 0x08
        buf.set_position(0)
        buf.get(to_send, 1, total_size)

        self.socket.sendto(to_send, self.server_address)

    def safe_read(self, count):
        arr = bytearray()

        while len(arr) < count and self.connected:
            tmp, _ = self.socket.recvfrom(count)
            tmp2 = bytearray(tmp)
            arr.extend(tmp2)

        return arr

    def start_reading(self):
        while self.connected:
            print "Waiting for command.."

            header_arr = self.safe_read(1)

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

    def listen_for(self, event_id, callback):
        if not callable(callback):
            print "Invalid callback!"
            return

        if event_id not in self.event_callbacks:
            self.event_callbacks[event_id] = list()

        self.event_callbacks[event_id].append(callback)
