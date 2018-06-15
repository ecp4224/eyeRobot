import socket
import sys
import atexit
from command import Command
from threading import Thread
from util.bytebuffer import ByteBuffer
from config import IP, PORT, BUFFER, NAME, PEER_IP


class RobotClient:
    socket = None
    connected = False
    server_address = (IP, PORT)
    packet_number = 0
    read_thread = None
    on_command = None

    def __init__(self, on_command):
        """

        :type on_command: function
        """
        if not callable(on_command):
            print 'Invalid function passed, please pass a function to on_command'
            sys.exit()

        self.on_command = on_command

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

        atexit.register(self.disconnect)

    def disconnect(self):
        self.connected = False

        self.socket.shutdown(socket.SHUT_WR)

    def send_session_packet(self):
        arr = bytearray([0x00, 0, len(NAME), len(PEER_IP)])
        arr.extend(NAME)
        arr.extend(PEER_IP)

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

                # Extract the rest of the packet
                buf = ByteBuffer(barr, 1, 24)

                pnum = buf.get_SLInt64()

                if pnum < self.packet_number:
                    continue # Ignore this packet, we got a packet with a higher number
                else:
                    self.packet_number = pnum # Update the latest packet number with the one in this packet

                # Get the value of each motor
                motor1 = buf.get_SLInt32()
                motor2 = buf.get_SLInt32()
                motor3 = buf.get_SLInt32()
                motor4 = buf.get_SLInt32()

                # Convert to motor command
                command = Command(motor1, motor2, motor3, motor4)

                # Invoke on_command callback
                self.on_command(command)
            else:
                print("Unknown packet:\nOpCode: " + barr[0] + "\nData:" + data)
