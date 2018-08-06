from module_client import ModuleClient


def on_data(data, module):
    print module


RGB = 4

client = ModuleClient()
client.connect()

client.listen_for(RGB, on_data)
