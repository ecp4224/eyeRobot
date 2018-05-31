package io.edkek.eyerobot.network.packet.module;

import io.edkek.eyerobot.network.impl.EyeClient;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.Packet;
import io.edkek.eyerobot.world.Robot;

import java.io.IOException;

public class RequestSensorInformationPacket extends Packet<EyeServer, ModuleClient> {
    @Override
    public void onHandlePacket(ModuleClient client) throws IOException {
        int packetLength = consume(4).asInt(); //Ignore

        int robotLength = consume(4).asInt();
        boolean hasFilter = consume(1).asBoolean();
        String name = consume(robotLength).asString();

        String filter = "";
        if (hasFilter) {
            filter = consume(consume(4).asInt()).asString();
        }

        if (client.getServer().getWorld().hasRobot(name)) {
            Robot robot = client.getServer().getWorld().getRobot(name);

            client.getModule().requestSensorInformation(robot, filter);
        } else {
            client.getServer().getLogger().warn(client.getModule().getName() + " tried requesting information from " +
                    "the robot \"" + name + "\" but it's not connected!");
        }
    }
}
