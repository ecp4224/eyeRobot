package io.edkek.eyerobot.network.packet.module;

import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class EventPacket extends Packet<EyeServer, ModuleClient> {

    @Override
    protected void onWritePacket(ModuleClient client, Object... args) throws IOException {
        if (args.length == 0)
            return;

        byte eventId = (byte)args[0];
        String moduleOwner = (String)args[1];
        String eventData = (String)args[2];

        int moduleOwnerLength = moduleOwner.length();
        int eventDataLength = eventData.length();

        write((byte)0x05)
                .write(eventId)
                .write(moduleOwnerLength)
                .write(eventDataLength)
                .write(moduleOwner)
                .write(eventData)
                .appendSizeToFront()
                .endTCP();
    }
}
