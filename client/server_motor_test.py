from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor
from robot_client import RobotClient

import atexit

print "Attach to motors"

mh = Adafruit_MotorHAT(addr=0x60)


# recommended for auto-disabling motors on shutdown!
def turnOffMotors():
    mh.getMotor(1).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(2).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(3).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(4).run(Adafruit_MotorHAT.RELEASE)


atexit.register(turnOffMotors)

print "Get motors"

frontl = mh.getMotor(3)
frontr = mh.getMotor(4)
rearl = mh.getMotor(1)
rearr = mh.getMotor(2)


def server_command(data):
    print "Got motor command from server"

    frontl.run(data.getMotor1Direction())
    frontr.run(data.getMotor2Direction())
    rearl.run(data.getMotor3Direction())
    rearr.run(data.getMotor4Direction())

    frontl.setSpeed(data.getMotor1())
    frontr.setSpeed(data.getMotor2())
    rearl.setSpeed(data.getMotor3())
    rearr.setSpeed(data.getMotor4())

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
    client.send_info_packet(motor1, motor2, motor3, motor4, 0.4, 0.7, bytearray([0] * 10), bytearray([0] * 20))



print "Prepare client"

client = RobotClient(on_command=server_command)

print "Connecting to server"

client.connect()
