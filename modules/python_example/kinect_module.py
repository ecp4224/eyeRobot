import freenect
import socket
from utils.bytebuffer import ByteBuffer

keep_running = True

alpha = 0.3
previousAcc = [0, 0, 0]
velocity = [0, 0, 0]
was_moving = False
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(("127.0.0.1", 1337))


def send_depth(dev, data, timestamp):
    timestamp += 1


def send_rgb(dev, data, timestamp):
    timestamp += 1


def body(dev, *args):
    global alpha
    global previousAcc
    global velocity
    global was_moving

    acc = freenect.get_accel(dev)

    accX = int(acc[0])
    accY = int(acc[1])
    accZ = int(acc[2])

    accX = accX - previousAcc[0]
    accY = accY - previousAcc[1]
    accZ = accZ - previousAcc[2]

    velocity[0] += accX
    velocity[1] += accY
    velocity[2] += accZ

    previousAcc[0] = int(acc[0])
    previousAcc[1] = int(acc[1])
    previousAcc[2] = int(acc[2])

    if abs(velocity[0]) > 0 or abs(velocity[2]) > 0:
        print("Moving!!\tX: " + str(velocity[0]) + "m/s\tZ: " + str(velocity[2]) + "m/s")
        was_moving = True
    elif was_moving:
        print("Stopped moving")
        was_moving = False

    buf = ByteBuffer(bytearray([0] * 24), 0, 24)
    buf.put_SLInt32(accX)
    buf.put_SLInt32(accY)
    buf.put_SLInt32(accZ)
    buf.put_SLInt32(velocity[0])
    buf.put_SLInt32(velocity[1])
    buf.put_SLInt32(velocity[2])

    arr = bytearray([0] * 25)
    arr[0] = 0x03
    buf.set_position(0)
    buf.get(arr, 1, 24)

    sock.send(arr)

    if not keep_running:
        raise freenect.Kill


freenect.runloop(depth=send_depth,
                 video=send_rgb,
                 body=body)
