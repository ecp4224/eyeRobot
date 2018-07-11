package io.edkek.eyerobot.network.packet.robot;

import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.RobotClient;
import io.edkek.eyerobot.network.packet.Packet;

import java.io.IOException;

public class UpdateMotorPacket extends Packet<EyeServer, RobotClient> {

    public UpdateMotorPacket(RobotClient client) {
        super(client);
    }

    @Override
    public void onWritePacket(RobotClient client, Object... args) throws IOException {
        int motor1 = (int) args[0];
        int motor2 = (int) args[1];
        int motor3 = (int) args[2];
        int motor4 = (int) args[3];

        long packetNumber = client.getLastPacketNumber() + 1;

        client.getServer().sendUdpPacket(
                write((byte) 0x03)
                        .write(packetNumber)
                        .write(motor1)
                        .write(motor2)
                        .write(motor3)
                        .write(motor4)
                        .endUDP()
        );

        client.setLastPacketNumber(packetNumber);
    }
}
