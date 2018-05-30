from robot_client import RobotClient


def test(data):
    print("Got callback with " + str(data.getMotor1()))


client = RobotClient(on_command=test)

client.connect()

print "Connected!"
