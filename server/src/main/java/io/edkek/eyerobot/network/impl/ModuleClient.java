package io.edkek.eyerobot.network.impl;

import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.PacketFactory;
import io.edkek.eyerobot.network.packet.Packet;
import io.edkek.eyerobot.world.WorldModule;
import io.netty.buffer.Unpooled;

import java.io.IOException;

public class ModuleClient extends EyeClient {
    private WorldModule module;

    public ModuleClient(EyeServer server) throws IOException {
        super(server);
    }

    @Override
    public void handlePacket(byte[] rawData) throws IOException {
        byte opCode = rawData[0];
        byte[] data = new byte[rawData.length - 1];

        System.arraycopy(rawData, 1, data, 0, data.length);

        Packet<EyeServer, ModuleClient> packet = PacketFactory.getModulePacket(opCode);
        if (packet == null) {
            System.err.println("Invalid opcode sent!\nIgnoring..");
            return;
        }

        packet.handlePacket(this, data);
        packet.endTCP();
    }

    public void attachModule(WorldModule module) {
        if (this.module != null)
            throw new IllegalStateException("This client already has a module state attached!");

        this.module = module;
    }

    public void listen() { }

    protected void onDisconnect() throws IOException {
        if (module != null && module.getWorld() != null) {
            module.getWorld().removeModule(module);
        }

        module = null;
    }

    @Override
    public void write(byte[] data) throws IOException {
        this.channel.write(Unpooled.copiedBuffer(data));
        this.channel.flush();
    }

    @Override
    public int read(byte[] into, int offset, int length) throws IOException {
        return 0;
    }

    @Override
    public void flush() throws IOException {
        this.channel.flush();
    }

    public WorldModule getModule() {
        return module;
    }
}
