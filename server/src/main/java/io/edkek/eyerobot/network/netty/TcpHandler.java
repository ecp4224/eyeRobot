package io.edkek.eyerobot.network.netty;

import io.edkek.eyerobot.network.impl.EyeServer;
import io.edkek.eyerobot.network.impl.ModuleClient;
import io.edkek.eyerobot.world.Robot;
import io.edkek.eyerobot.world.WorldModule;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;

import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.Charset;
import java.util.HashMap;

public class TcpHandler extends SimpleChannelInboundHandler<byte[]> {
    private EyeServer server;
    private HashMap<ChannelHandlerContext, ModuleClient> clients = new HashMap<>();

    public TcpHandler(EyeServer server) {
        this.server = server;
    }

    public EyeServer getServer() {
        return server;
    }

    protected void messageReceived(ChannelHandlerContext channelHandlerContext, byte[] data) throws Exception {
        //Get the client for this channel
        ModuleClient client = clients.get(channelHandlerContext);

        if (client == null) {
            //If we don't have one, then it's a new channel trying to join

            //They must be a module client because Robots cannot connect
            //over TCP. They must connect over UDP

            ModuleClient mClient = new ModuleClient(server);
            InetSocketAddress socketAddress = (InetSocketAddress)channelHandlerContext.channel().remoteAddress();
            mClient.attachChannel(channelHandlerContext);
            clients.put(channelHandlerContext, mClient);

            ByteBuffer buf = ByteBuffer.allocate(data.length).order(ByteOrder.LITTLE_ENDIAN).put(data);
            buf.position(0);

            //Extract the opcode, type, and length field
            byte opcode = buf.get();
            byte type = buf.get();

            if (type != 1) {
                //They are not who we think they are
                //modules are required to set this field to 1
                //otherwise kill them

                channelHandlerContext.disconnect();
                return;
            }

            byte length = buf.get();

            //Lastly extract the name using the length field
            String name = new String(data, 3, length, Charset.forName("ASCII"));

            //Check to see if module already in the world
            if (server.getWorld().hasModule(name)) {
                channelHandlerContext.disconnect();
                return;
            }

            //Create the module state and give it the client
            final WorldModule module = new WorldModule(name, mClient);

            //Now save all the new stuff
            server.getWorld().addModule(module); //Add the state to the world
            mClient.attachModule(module); //Attach the state to the client
            mClient.sendOk(); //Notify the client that login was successful
            clients.put(channelHandlerContext, mClient); //Save the channel -> client
            server.addClient(mClient); //Add the client to a list of connected clients

            //WE DID IT :D
            server.getLogger().info("TCP connection made with client " + socketAddress.getAddress() + " using name " + name);
        } else {
            //We already have a client, have them handle the data
            client.handlePacket(data);
        }
    }

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        cause.printStackTrace();
        ctx.close();
    }

    @Override
    public void channelActive(ChannelHandlerContext ctx) throws Exception {
        server.getLogger().info("Client connected @ " + ctx.channel().remoteAddress());
    }

    @Override
    public void channelReadComplete(ChannelHandlerContext ctx) {
        ctx.flush();
    }
}
