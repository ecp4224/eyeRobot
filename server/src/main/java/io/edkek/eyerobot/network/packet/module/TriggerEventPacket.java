package io.edkek.eyerobot.network.packet.module;

import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.Packet;
import io.edkek.eyerobot.world.World;
import io.edkek.eyerobot.world.WorldModule;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

public class TriggerEventPacket extends Packet<EyeServer, ModuleClient> {

    @Override
    public void onHandlePacket(ModuleClient client) throws IOException {
        World world = client.getModule().getWorld();
        String moduleOwner = client.getModule().getName();

        int packetLength = consume(4).asInt(); //Ignore

        byte eventId = consume(1).asByte();
        boolean shouldFilter = consume(1).asBoolean();

        int eventDataLength = consume(4).asInt();

        String eventData = consume(eventDataLength).asString();

        //client.getServer().getLogger().info("Forwarding event " + eventId + " from " + moduleOwner + " (event length: " + eventDataLength + ") ");

        List<WorldModule> modulesToSend;
        if (shouldFilter) {
            modulesToSend = new ArrayList<>(5);

            int filterLength = consume(4).asInt();
            String fileStr = consume(filterLength).asString();

            String[] modules = fileStr.split(",");

            for (String module : modules) {
                WorldModule m = world.getModule(module);

                if (m != null) {
                    modulesToSend.add(m);
                }
            }
        } else {
            modulesToSend = world.getAllModules();

            modulesToSend.remove(client.getModule());
        }

        EventPacket packet = new EventPacket();
        for (WorldModule module : modulesToSend) {
            packet.reuseFor(module.getClient());

            packet.writeAndPreservePacket(eventId, moduleOwner, eventData);
        }

        packet.dispose();
    }
}
