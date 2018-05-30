import socket
import sys
import array
from threading import Thread
from util.bytebuffer import ByteBuffer
from config import IP, PORT, BUFFER, NAME


class RobotClient:
    socket = None
    connected = False
    server_address = (IP, PORT)
    packet_number = 0
    read_thread = None

    def __init__(self):
        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        except socket.error:
            print 'Failed to create socket'
            sys.exit()

    def connect(self):
        # self.socket.connect(self.server_address)

        self.connected = True

        self.read_thread = Thread(target=self.start_reading)
        self.read_thread.start()

        self.send_session_packet()

    def send_session_packet(self):
        arr = bytearray([0x00, 0, len(NAME)])
        arr.extend(NAME)

        self.socket.sendto(arr, self.server_address)

    def send_info_packet(self, motor1, motor2, motor3, motor4, acc, gyro, rgbArray, depthArray):
        total_size = 40 + len(rgbArray) + len(depthArray)

        array = bytearray([0] * total_size)
        buf = ByteBuffer(array, 0, total_size)

        buf.put_SLInt64(self.packet_number)
        buf.put_SLInt32(len(rgbArray))
        buf.put_SLInt32(len(depthArray))
        buf.put_LFloat32(acc)
        buf.put_LFloat32(gyro)
        buf.put_SLInt32(motor1)
        buf.put_SLInt32(motor2)
        buf.put_SLInt32(motor3)
        buf.put_SLInt32(motor4)
        buf.put_bytes(rgbArray, 0, len(rgbArray))
        buf.put_bytes(depthArray, 0, len(depthArray))

        to_send = bytearray([0] * (total_size + 1))
        to_send[0] = 0x02
        buf.get(to_send, 1, total_size)

        self.socket.sendto(to_send, self.server_address)

    def start_reading(self):
        while self.connected:
            print "Waiting for command.."
            data, adr = self.socket.recvfrom(BUFFER)

            if data[0] == 0x03:
                print("GOT COMMAND: " + data)
            else:
                print("Unknown packet: " + data)
