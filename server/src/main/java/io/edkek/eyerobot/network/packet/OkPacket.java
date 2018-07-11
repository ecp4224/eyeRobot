package io.edkek.eyerobot.network.packet;

import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class OkPacket extends Packet<EyeServer, Client<EyeServer>> {
    public OkPacket(Client<EyeServer> client) {
        super(client);
    }

    @Override
    protected void onWritePacket(Client<EyeServer> client, Object... args) throws IOException {
        if (args.length == 0)
            return;

        boolean isOk = (boolean)args[0];

        write((byte)0x01)
                .write(isOk)
                .endTCP();
    }
}