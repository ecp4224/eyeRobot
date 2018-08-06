from threespace import threespace_api as ts_api
import sys
import glob
import serial
import time
from robot_client import RobotClient


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


print(serial_ports())


def callback(motor1, motor2, motor3, motor4):
    print(motor1)
    print(motor2)
    print(motor3)
    print(motor4)


robot = RobotClient(on_command=callback)
robot.connect()

## If the COM port is already known and the device type is known for the 3-Space
## Sensor device, we can just create the appropriate instance without doing a
## search.
com_port = "/dev/tty.usbmodem1421"
try:
    device = ts_api.TSUSBSensor(com_port=com_port)
except:
    print("No device on {0}".format(com_port))
## If a connection to the COM port fails, None is returned.
else:
    print(device)

    ## Now close the port.
    if device is not None:

        ## Now we can start getting information from the device.
        ## The class instances have all of the functionality that corresponds to the
        ## 3-Space Sensor device type it is representing.
        while True:
            quat = device.getUntaredOrientationAsQuaternion()
            acc = device.getCorrectedLinearAccelerationInGlobalSpace()
            compass = device.getNormalizedCompassVector()

            robot.send_info_packet(0, 0, 0, 0, acc, quat, compass)

        device.close()
