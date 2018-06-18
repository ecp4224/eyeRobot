import freenect
import zlib

from robot_client import RobotClient

keep_running = True
tilt = 0


def on_command(cmd):
    global tilt

    tilt = cmd.getMotor1()
    if tilt < 0:
        tilt = 0
    if tilt > 30:
        tilt = 30


client = RobotClient(on_command=on_command)
client.connect()


def send_depth(dev, data, timestamp):
    global keep_running
    global client

    b = data.tobytes()

    compressed_data = zlib.compress(b)
    pls_work = zlib.compress(compressed_data)

    client.send_depth_packet(pls_work)


def send_rgb(dev, data, timestamp):
    global keep_running

    b = data.tobytes()

    compressed_data = zlib.compress(b)
    pls_work = zlib.compress(compressed_data)

    client.send_rgb_packet(pls_work)


def body(dev, *args):
    global tilt

    freenect.set_tilt_degs(dev, tilt)

    acc = freenect.get_accel(dev)

    client.send_info_packet(tilt, acc[0], acc[1], acc[2])

    if not keep_running:
        client.disconnect()
        raise freenect.Kill


freenect.runloop(depth=send_depth,
                 video=send_rgb,
                 body=body)
