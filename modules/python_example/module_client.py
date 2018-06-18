import socket
import sys
import atexit
from threading import Thread
from util.bytebuffer import ByteBuffer
from config import IP, PORT, BUFFER, NAME
import json


class ModuleClient:
    socket = None
    connected = False
    server_address = (IP, PORT)
    packet_number = 0
    read_thread = None
    on_info_update = None
    on_event = None

    def __init__(self):
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

    def start_reading(self):
        while self.connected:
            print "Waiting for command.."

            # Wait until we get a UDP packet
            data, adr = self.socket.recvfrom(BUFFER)

            # Convert to byte array
            barr = bytearray(data)

            if barr[0] == 0x03:
                # If the opcode is 0x03, then we got a command
                print("Got Motor Command")
            else:
                print("Unknown packet:\nOpCode: " + barr[0] + "\nData:" + data)
