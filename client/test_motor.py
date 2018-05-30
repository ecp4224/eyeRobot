# installation instructions can be found here https://learn.adafruit.com/adafruit-dc-and-stepper-motor-hat-for-raspberry-pi/installing-software

# !/usr/bin/python
from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor

import time
import atexit

# create a default object, no changes to I2C address or frequency
mh = Adafruit_MotorHAT(addr=0x60)


# recommended for auto-disabling motors on shutdown!
def turnOffMotors():
    mh.getMotor(1).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(2).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(3).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(4).run(Adafruit_MotorHAT.RELEASE)


atexit.register(turnOffMotors)

frontl = mh.getMotor(3)
frontr = mh.getMotor(4)
rearl = mh.getMotor(1)
rearr = mh.getMotor(2)

# use setSpeed(n) where n is from 0 to 255 to set the speed of the motor
# use Adafruit_MotorHAT.FORWARD or .BACKWARD or .RELEASE to set the direction forward, backward, or stop


# the following will run the robot forward for 1.5 seconds and then stop
frontl.run(Adafruit_MotorHAT.FORWARD)
frontr.run(Adafruit_MotorHAT.FORWARD)
rearl.run(Adafruit_MotorHAT.FORWARD)
rearr.run(Adafruit_MotorHAT.FORWARD)

frontl.setSpeed(255)
frontr.setSpeed(255)
rearl.setSpeed(255)
rearr.setSpeed(255)

time.sleep(1.5)  # seconds

frontl.run(Adafruit_MotorHAT.RELEASE)
frontr.run(Adafruit_MotorHAT.RELEASE)
rearl.run(Adafruit_MotorHAT.RELEASE)
rearr.run(Adafruit_MotorHAT.RELEASE)
