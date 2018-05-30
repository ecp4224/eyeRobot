package io.edkek.eyerobot.network.impl;

import io.edkek.eyerobot.config.ServerConfig;
import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.Server;
import io.edkek.eyerobot.network.netty.TcpHandler;
import io.edkek.eyerobot.network.netty.TcpServerInitializer;
import io.edkek.eyerobot.world.Robot;
import io.edkek.eyerobot.world.World;
import io.netty.bootstrap.ServerBootstrap;
import io.netty.channel.EventLoopGroup;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.handler.logging.LogLevel;
import io.netty.handler.logging.LoggingHandler;
import io.netty.util.concurrent.GenericProgressiveFutureListener;
import io.netty.util.concurrent.ProgressiveFuture;
import me.eddiep.jconfig.JConfig;

import java.io.File;
import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;

public class EyeServer extends Server {

    protected List<EyeClient> connectedClients = new ArrayList<>();
    private ServerConfig config;
    private TcpHandler handler;
    private World world;

    protected DatagramSocket udpServerSocket;
    protected Thread udpThread;
    protected HashMap<UdpClientInfo, RobotClient> connectedUdpClients = new HashMap<>();

    @Override
    protected void onStart() {
        super.onStart();

        world = new World();

        config = JConfig.newConfigObject(ServerConfig.class);
        File file = new File("server.json");
        if (!file.exists())
            config.save(file);
        else
            config.load(file);

        //Setup UDP
        try {
            if (!config.getServerIP().equals(""))
                udpServerSocket = new DatagramSocket(config.getServerPort(), InetAddress.getByName(config.getServerIP()));
            else
                udpServerSocket = new DatagramSocket(config.getServerPort());
        } catch (IOException e) {
            e.printStackTrace();
        }

        log.info("Listening on port " + udpServerSocket.getLocalPort());

        udpThread = new Thread(UDP_SERVER_RUNNABLE);
        udpThread.start();

        //Setup TCP
        handler = new TcpHandler(this);
        final EventLoopGroup bossGroup = new NioEventLoopGroup(1);
        final EventLoopGroup workerGroup = new NioEventLoopGroup();
        try {
            ServerBootstrap b = new ServerBootstrap();
            b.group(bossGroup, workerGroup)
                    .channel(NioServerSocketChannel.class)
                    .handler(new LoggingHandler(LogLevel.DEBUG))
                    .childHandler(new TcpServerInitializer(this, handler));

            //TODO Handle future when channel is closed
            b.bind(config.getServerPort()).sync().channel().closeFuture().addListener(new GenericProgressiveFutureListener<ProgressiveFuture<Void>>() {
                @Override
                public void operationProgressed(ProgressiveFuture progressiveFuture, long l, long l1) throws Exception {
                }

                @Override
                public void operationComplete(ProgressiveFuture progressiveFuture) throws Exception {
                    bossGroup.shutdownGracefully();
                    workerGroup.shutdownGracefully();
                }
            });
            getLogger().info("Listening on port " + config.getServerPort());
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public ServerConfig getConfig() {
        return config;
    }

    public World getWorld() {
        return world;
    }

    public void onDisconnect(EyeClient client) throws IOException {
        log.info("[SERVER] " + client.getIpAddress() + " disconnected..");
        connectedClients.remove(client);
    }

    public List<EyeClient> getConnectedClients() {
        return Collections.unmodifiableList(connectedClients);
    }

    public void addClient(EyeClient mClient) {
        connectedClients.add(mClient);
    }

    private void validateUdpSession(DatagramPacket packet) throws IOException {
        byte[] data = packet.getData();
        ByteBuffer buffer = ByteBuffer.allocate(data.length).order(ByteOrder.LITTLE_ENDIAN).put(data);
        buffer.position(0);
        if (buffer.get() != 0x00)
            return;

        byte type = buffer.get();

        if (type != 0) {
            //They are not who we think they are
            //robots are required to set this field to 0
            //otherwise kill them
            return;
        }

        byte length = buffer.get();

        //Lastly extract the name using the length field
        String name = new String(data, 3, length, Charset.forName("ASCII"));

        //Check to see if module already in the world
        if (world.hasRobot(name)) {
            return;
        }

        UdpClientInfo info = new UdpClientInfo(packet.getAddress(), packet.getPort());
        RobotClient client = new RobotClient(this, info);
        Robot robot = new Robot(name, client);

        client.attachRobot(robot);

        connectedUdpClients.put(info, client);

        client.setIpAddress(packet.getAddress());
        client.setPort(client.getPort());

        log.info("UDP connection made with robot " + info + " using name " + name);
    }

    public void sendUdpPacket(DatagramPacket packet) throws IOException {
        udpServerSocket.send(packet);
    }

    private final Runnable UDP_SERVER_RUNNABLE = new Runnable() {
        @Override
        public void run() {
            Thread.currentThread().setName("UDP Server Listener");
            DatagramPacket receivePacket;
            byte[] receiveData;
            while (isRunning()) {
                try {
                    receiveData = new byte[1024];

                    receivePacket = new DatagramPacket(receiveData, 0, receiveData.length);
                    udpServerSocket.receive(receivePacket);

                    if (!isRunning())
                        break;

                    UdpClientInfo info = new UdpClientInfo(receivePacket.getAddress(), receivePacket.getPort());
                    RobotClient client;
                    if ((client = connectedUdpClients.get(info)) != null) {
                        client.processUdpPacket(receivePacket);
                    } else {
                        new UdpAcceptThread(receivePacket).run();
                    }

                } catch (Throwable t) {
                    t.printStackTrace();
                }
            }
        }
    };

    private class UdpAcceptThread extends Thread {
        private DatagramPacket packet;
        public UdpAcceptThread(DatagramPacket packet) { this.packet = packet; }

        @Override
        public void run() {
            try {
                validateUdpSession(packet);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    class UdpClientInfo {
        private InetAddress address;
        private int port;

        public UdpClientInfo(InetAddress address, int port) {
            this.address = address;
            this.port = port;
        }

        @Override
        public boolean equals(Object o) {
            if (this == o) return true;
            if (o == null || getClass() != o.getClass()) return false;

            UdpClientInfo that = (UdpClientInfo) o;

            if (port != that.port) return false;
            if (!address.equals(that.address)) return false;

            return true;
        }

        @Override
        public int hashCode() {
            int result = address.hashCode();
            result = 31 * result + port;
            return result;
        }

        @Override
        public String toString() {
            return "UdpClientInfo{" +
                    "address=" + address +
                    ", port=" + port +
                    '}';
        }
    }
}
