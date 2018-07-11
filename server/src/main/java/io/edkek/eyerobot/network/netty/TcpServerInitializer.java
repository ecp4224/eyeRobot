package io.edkek.eyerobot.network.netty;

import io.edkek.eyerobot.network.impl.EyeServer;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.socket.nio.NioSocketChannel;

public class TcpServerInitializer extends ChannelInitializer<NioSocketChannel> {
    private final EyeServer server;
    private final TcpHandler handler;

    public TcpServerInitializer(EyeServer server, TcpHandler handler) {
        this.server = server;
        this.handler = handler;
    }


    protected void initChannel(NioSocketChannel nioSocketChannel) throws Exception {
        ChannelPipeline pipeline = nioSocketChannel.pipeline();

        pipeline.addLast(new PacketDecoder());
        pipeline.addLast(handler);
    }
}
