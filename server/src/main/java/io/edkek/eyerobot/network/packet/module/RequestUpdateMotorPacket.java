package io.edkek.eyerobot.network.packet.module;

import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.packet.Packet;
import io.edkek.eyerobot.network.packet.robot.UpdateMotorPacket;
import io.edkek.eyerobot.world.Robot;

import java.io.IOException;

public class RequestUpdateMotorPacket extends Packet<EyeServer, ModuleClient> {

    @Override
    public void onHandlePacket(ModuleClient client) throws IOException {
        int packetLength = consume(4).asInt(); //Ignore

        int motor1 = consume(4).asInt();
        int motor2 = consume(4).asInt();
        int motor3 = consume(4).asInt();
        int motor4 = consume(4).asInt();

        //get name of robot
        int nameLength = consume(4).asInt();

        String name = consume(nameLength).asString();

        if (client.getServer().getWorld().hasRobot(name)) {
            Robot robot = client.getServer().getWorld().getRobot(name);

            UpdateMotorPacket packet = new UpdateMotorPacket(robot.getClient());

            packet.writePacket(motor1, motor2, motor3, motor4);
        } else {
            client.getServer().getLogger().warn(client.getModule().getName() + " tried sending the robot \"" + name +
                    "\" a command but it's not connected!");
        }
    }
}
