package io.edkek.eyerobot.network.impl;

import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.PacketFactory;
import io.edkek.eyerobot.network.packet.Packet;
import io.edkek.eyerobot.world.*;
import io.edkek.eyerobot.world.Robot;

import java.awt.*;
import java.io.IOException;
import java.net.DatagramPacket;

public class RobotClient extends EyeClient {
    private long lastPacketNumber;
    private Robot robot;
    private EyeServer.UdpClientInfo udpClientInfo;

    public RobotClient(EyeServer server, EyeServer.UdpClientInfo info) throws IOException {
        super(server);
        udpClientInfo = info;

        super.setIpAddress(info.getAddress());
        super.setPort(info.getPort());
    }

    @Override
    public void handlePacket(byte[] data) {

    }

    public void listen() {

    }

    protected void onDisconnect() throws IOException {
        if (robot != null && robot.getWorld() != null) {
            robot.getWorld().removeRobot(robot);
        }

        robot = null;
        socketServer.onDisconnect(this);
    }

    public void write(byte[] data) throws IOException {

    }

    public int read(byte[] into, int offset, int length) throws IOException {
        return 0;
    }

    public void flush() throws IOException {

    }

    public void setLastPacketNumber(long lastPacketNumber) {
        this.lastPacketNumber = lastPacketNumber;
    }

    public long getLastPacketNumber() {
        return lastPacketNumber;
    }

    public Robot getRobot() {
        return robot;
    }

    public void processUdpPacket(DatagramPacket receivePacket) throws IOException {
        byte[] rawData = receivePacket.getData();
        byte opCode = rawData[0];
        byte[] data = new byte[receivePacket.getLength() - 1];

        System.arraycopy(rawData, 1, data, 0, data.length);

        Packet<EyeServer, RobotClient> packet = PacketFactory.getRobotPacket(opCode);
        if (packet == null) {
            getServer().getLogger().error("Invalid opcode sent via UDP: " + opCode + ", data length=" + data.length);
            return;
        }
        packet.handlePacket(this, data);
        packet.endUDP();
    }

    public void attachRobot(Robot robot) {
        this.robot = robot;
    }

    public EyeServer.UdpClientInfo getUdpClientInfo() {
        return udpClientInfo;
    }
}
