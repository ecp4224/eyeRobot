package io.edkek.eyerobot.network.packet.module;

import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class JsonSensorInformationPacket extends Packet<EyeServer, ModuleClient> {

    public JsonSensorInformationPacket(ModuleClient client) {
        super(client);
    }

    @Override
    protected void onWritePacket(ModuleClient client, Object... args) throws IOException {
        String robot = (String)args[0];
        String json = (String)args[1];

        write((byte)0x08)
                .write(robot.length())
                .write(robot)
                .write(json.length())
                .write(json)
                .endTCP();
    }
}
