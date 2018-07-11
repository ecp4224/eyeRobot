package io.edkek.eyerobot.network.impl;

import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.packet.OkPacket;

import java.io.IOException;

public abstract class EyeClient extends Client<EyeServer> {
    public EyeClient(EyeServer server) throws IOException {
        super(server);
    }

    public EyeClient sendOk() throws IOException {
        return sendOk(true);
    }

    public EyeClient sendOk(boolean value) throws IOException {
        OkPacket packet = new OkPacket(this);
        packet.writePacket(value);
        return this;
    }

    public abstract void handlePacket(byte[] data) throws IOException;
}
