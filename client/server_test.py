from robot_client import RobotClient
import time


def test(data):
    motor1 = data.getMotor1()
    motor2 = data.getMotor2()
    motor3 = data.getMotor3()
    motor4 = data.getMotor4()

    # Echo back the command, along with some test variables
    client.send_info_packet(motor1, motor2, motor3, motor4, 0.4, 0.7, bytearray([0] * 10), bytearray([0] * 20))


client = RobotClient(on_command=test)

client.connect()

print "Connected!"
