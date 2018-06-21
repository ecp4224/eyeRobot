import freenect

from module_client import ModuleClient

keep_running = True
tilt = 0
updateit = True

client = ModuleClient()
client.connect()


def send_depth(dev, data, timestamp):
    global keep_running
    global client

    # b = data.tobytes()

    # compressed_data = zlib.compress(b)
    # pls_work = zlib.compress(compressed_data)

    client.trigger_event(3, {"data": data.tolist()}, "")


def send_rgb(dev, data, timestamp):
    global keep_running

    # b = data.tobytes()

    # compressed_data = zlib.compress(b)
    # pls_work = zlib.compress(compressed_data)

    client.trigger_event(4, {"data": data.tolist()}, "")


def body(dev, *args):
    global tilt
    global updateit

    if updateit:
        freenect.set_depth_mode(dev, freenect.RESOLUTION_MEDIUM, freenect.DEPTH_11BIT)
        updateit = False

    freenect.set_tilt_degs(dev, tilt)

    acc = freenect.get_accel(dev)

    client.trigger_event(5, {"tilt": tilt, "accX": acc[0], "accY": acc[1], "accZ": acc[2]}, "")

    if not keep_running:
        client.disconnect()
        raise freenect.Kill


freenect.runloop(depth=send_depth,
                 video=send_rgb,
                 body=body)
