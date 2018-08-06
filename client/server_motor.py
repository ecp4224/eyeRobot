from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor
from robot_client import RobotClient
from threespace import threespace_api as ts_api
import sys
import glob
import serial
import atexit


def serial_ports():
    """ Lists serial port names

        :raises EnvironmentError:
            On unsupported or unknown platforms
        :returns:
            A list of the serial ports available on the system
    """
    if sys.platform.startswith('win'):
        ports = ['COM%s' % (i + 1) for i in range(256)]
    elif sys.platform.startswith('linux') or sys.platform.startswith('cygwin'):
        # this excludes your current terminal "/dev/tty"
        ports = glob.glob('/dev/tty[A-Za-z]*')
    elif sys.platform.startswith('darwin'):
        ports = glob.glob('/dev/tty.*')
    else:
        raise EnvironmentError('Unsupported platform')

    result = []
    for port in ports:
        try:
            s = serial.Serial(port)
            s.close()
            result.append(port)
        except (OSError, serial.SerialException):
            pass
    return result


# Print serial ports (to find gyroscope)
print(serial_ports())

print "Attach to motors"

# Attach to motors
mh = Adafruit_MotorHAT(addr=0x60)


def turnOffMotors():
    """Disable all motors"""
    
    mh.getMotor(1).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(2).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(3).run(Adafruit_MotorHAT.RELEASE)
    mh.getMotor(4).run(Adafruit_MotorHAT.RELEASE)


# Run function when script exists
atexit.register(turnOffMotors)

print "Get motors"

# Save motor references
frontl = mh.getMotor(3)
frontr = mh.getMotor(4)
rearl = mh.getMotor(1)
rearr = mh.getMotor(2)

device = None

# Function to run when a new command is received from the server
def server_command(data):
    """Operate the motors as specified by the server"""
    
    print "Got motor command from server"
    global device

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

    quat = device.getUntaredOrientationAsQuaternion()
    acc = device.getCorrectedLinearAccelerationInGlobalSpace()
    compass = device.getNormalizedCompassVector()

    if data.getMotor1Direction() == Adafruit_MotorHAT.BACKWARD:
        motor1 = -motor1

    if data.getMotor2Direction() == Adafruit_MotorHAT.BACKWARD:
        motor2 = -motor2

    if data.getMotor3Direction() == Adafruit_MotorHAT.BACKWARD:
        motor3 = -motor3

    if data.getMotor4Direction() == Adafruit_MotorHAT.BACKWARD:
        motor4 = -motor4

    # Echo back the command, along with some test variables
    client.send_info_packet(motor1, motor2, motor3, motor4, acc, quat, compass)

# Port that gyroscope is on
com_port = "/dev/tty.usbmodem1411"

print "Attaching to COM port " + com_port

# Setup gyroscope
try:
    device = ts_api.TSUSBSensor(com_port=com_port)
except:
    print("No device on {0}".format(com_port))
else:
    print(device)

print "Prepare client"

# Create a new RobotClient and set the on_command callback to be the server_command function
client = RobotClient(on_command=server_command)

print "Connecting to server"

# Connect to the server in config.py
client.connect()
