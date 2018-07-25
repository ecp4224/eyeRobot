package io.edkek.eyerobot.network.packet.impl;

import io.edkek.eyerobot.network.impl.EyeClient;
import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.RobotClient;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class DepthPacket extends Packet<EyeServer, EyeClient> {

    @Override
    public void onHandlePacket(EyeClient client) throws IOException {
        if (client instanceof RobotClient) {
            RobotClient robotClient = (RobotClient) client;

            long packetNumber = consume(8).asLong();

            if (robotClient.getLastKinectPacketNumber() < packetNumber)
                return;
            else {
                robotClient.setLastKinectPacketNumber(packetNumber);
            }

            int depthLength = consume(4).asInt();
            double accx = consume(8).asDouble();
            double accy = consume(8).asDouble();
            double accz = consume(8).asDouble();
            byte[] depthData = consume(depthLength).raw();

            robotClient.getRobot().update(accx, accy, accz, new byte[0], depthData);
        }
    }
}
