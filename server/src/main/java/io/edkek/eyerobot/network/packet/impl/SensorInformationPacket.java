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

            int rgbLength = consume(4).asInt();
            int depthLength = consume(4).asInt();
            float acc = consume(4).asFloat();
            float gyro = consume(4).asFloat();
            int motor1 = consume(4).asInt();
            int motor2 = consume(4).asInt();
            int motor3 = consume(4).asInt();
            int motor4 = consume(4).asInt();
            byte[] rgbData = consume(rgbLength).raw();
            byte[] depthData = consume(depthLength).raw();

            robotClient.getRobot().update(motor1, motor2, motor3, motor4);
        }
    }
}
