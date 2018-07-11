package io.edkek.eyerobot.network.packet.impl;

import io.edkek.eyerobot.network.impl.EyeClient;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class OkPacket extends Packet<EyeServer, EyeClient> {

    @Override
    protected void onWritePacket(EyeClient client, Object... args) throws IOException {
        if (args.length == 0)
            return;

        boolean isOk = (boolean)args[0];

        write((byte)0x01)
                .write(isOk)
                .endTCP();
    }
}
