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


def server_command(cmd):
    print "Got motor command from server"

    frontl.run(cmd.getMotor1Direction())
    frontr.run(cmd.getMotor2Direction())
    rearl.run(cmd.getMotor3Direction())
    rearr.run(cmd.getMotor4Direction())

    frontl.setSpeed(cmd.getMotor1())
    frontr.setSpeed(cmd.getMotor2())
    rearl.setSpeed(cmd.getMotor3())
    rearr.setSpeed(cmd.getMotor4())


print "Prepare client"

client = RobotClient(on_command=server_command)

print "Connecting to server"

client.connect()
