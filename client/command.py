from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor


class Command:
    motor1 = 0
    motor2 = 0
    motor3 = 0
    motor4 = 0

    def __init__(self, motor1, motor2, motor3, motor4):
        self.motor1 = motor1
        self.motor2 = motor2
        self.motor3 = motor3
        self.motor4 = motor4

    def getMotor1Direction(self):
        if self.motor1 > 0:
            return Adafruit_MotorHAT.FORWARD
        elif self.motor1 < 0:
            return Adafruit_MotorHAT.BACKWARD
        else:
            return Adafruit_MotorHAT.RELEASE

    def getMotor2Direction(self):
        if self.motor2 > 0:
            return Adafruit_MotorHAT.FORWARD
        elif self.motor2 < 0:
            return Adafruit_MotorHAT.BACKWARD
        else:
            return Adafruit_MotorHAT.RELEASE

    def getMotor3Direction(self):
        if self.motor3 > 0:
            return Adafruit_MotorHAT.FORWARD
        elif self.motor3 < 0:
            return Adafruit_MotorHAT.BACKWARD
        else:
            return Adafruit_MotorHAT.RELEASE

    def getMotor4Direction(self):
        if self.motor4 > 0:
            return Adafruit_MotorHAT.FORWARD
        elif self.motor4 < 0:
            return Adafruit_MotorHAT.BACKWARD
        else:
            return Adafruit_MotorHAT.RELEASE

    def getMotor1(self):
        return abs(self.motor1)

    def getMotor2(self):
        return abs(self.motor2)

    def getMotor3(self):
        return abs(self.motor3)

    def getMotor4(self):
        return abs(self.motor4)
