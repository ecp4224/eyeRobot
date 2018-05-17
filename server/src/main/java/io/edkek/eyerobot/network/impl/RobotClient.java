package io.edkek.eyerobot.network.impl;

import io.edkek.eyerobot.network.Client;

import java.io.IOException;

public class RobotClient extends EyeClient {
    public RobotClient(EyeServer server) throws IOException {
        super(server);
    }

    @Override
    public void handlePacket(byte[] data) {

    }

    public void listen() {

    }

    protected void onDisconnect() throws IOException {

    }

    public void write(byte[] data) throws IOException {

    }

    public int read(byte[] into, int offset, int length) throws IOException {
        return 0;
    }

    public void flush() throws IOException {

    }
}
