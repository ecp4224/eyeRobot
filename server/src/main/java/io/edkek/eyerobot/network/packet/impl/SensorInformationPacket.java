package io.edkek.eyerobot.network.packet.impl;

import io.edkek.eyerobot.network.impl.EyeClient;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.network.impl.RobotClient;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class SensorInformationPacket extends Packet<EyeServer, EyeClient> {

    @Override
    public void onHandlePacket(EyeClient client) throws IOException {
        if (client instanceof RobotClient) {
            RobotClient robotClient = (RobotClient)client;

            long packetNumber = consume(8).asLong();

            if (robotClient.getLastPacketNumber() < packetNumber)
                return;
            else {
                robotClient.setLastPacketNumber(packetNumber);
            }

            int motor1 = consume(4).asInt();
            int motor2 = consume(4).asInt();
            int motor3 = consume(4).asInt();
            int motor4 = consume(4).asInt();

            float accX = consume(4).asFloat();
            float accY = consume(4).asFloat();
            float accZ = consume(4).asFloat();

            float compassX = consume(4).asFloat();
            float compassY = consume(4).asFloat();
            float compassZ = consume(4).asFloat();

            float orientationX = consume(4).asFloat();
            float orientationY = consume(4).asFloat();
            float orientationZ = consume(4).asFloat();
            float orientationW = consume(4).asFloat();


            robotClient.getRobot().update(motor1, motor2, motor3, motor4,
                    accX, accY, accZ,
                    compassX, compassY, compassZ,
                    orientationX, orientationY, orientationZ, orientationW);
        }
    }
}
