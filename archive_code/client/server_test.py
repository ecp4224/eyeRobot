from robot_client import RobotClient
from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor
import time


def onCommand(data):
    motor1 = data.getMotor1()
    motor2 = data.getMotor2()
    motor3 = data.getMotor3()
    motor4 = data.getMotor4()

    if data.getMotor1Direction() == Adafruit_MotorHAT.BACKWARD:
        motor1 = -motor1

    if data.getMotor2Direction() == Adafruit_MotorHAT.BACKWARD:
        motor2 = -motor2

    if data.getMotor3Direction() == Adafruit_MotorHAT.BACKWARD:
        motor3 = -motor3

    if data.getMotor4Direction() == Adafruit_MotorHAT.BACKWARD:
        motor4 = -motor4

    # Echo back the command, along with some test variables
    client.send_info_packet(motor1, motor2, motor3, motor4)


client = RobotClient(on_command=onCommand)

client.connect()

print "Connected!"
