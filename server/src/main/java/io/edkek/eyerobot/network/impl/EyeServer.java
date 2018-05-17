package io.edkek.eyerobot.network.impl;

import io.edkek.eyerobot.config.ServerConfig;
import io.edkek.eyerobot.network.Client;
import io.edkek.eyerobot.network.Server;
import io.edkek.eyerobot.network.netty.TcpHandler;
import io.edkek.eyerobot.network.netty.TcpServerInitializer;
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
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class EyeServer extends Server {

    protected List<EyeClient> connectedClients = new ArrayList<>();
    private ServerConfig config;
    private TcpHandler handler;
    private World world;

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

        if (config.getServerSecret().length() != 32) {
            System.err.println("The server secret is not 32 characters!");
            System.err.println("Aborting..");
            System.exit(1);
            return;
        }

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
}
