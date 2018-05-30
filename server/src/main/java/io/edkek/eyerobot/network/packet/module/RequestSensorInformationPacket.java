package io.edkek.eyerobot.network.packet.module;

import io.edkek.eyerobot.network.impl.EyeClient;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class RequestSensorInformationPacket extends Packet<EyeServer, ModuleClient> {
    @Override
    public void onHandlePacket(ModuleClient client) throws IOException {

    }
}
