package io.edkek.eyerobot.network;

import io.netty.channel.ChannelHandlerContext;

import java.io.IOException;
import java.net.InetAddress;

/**
 * Represents a connected Client for a certain type of {@link Server}
 * @param <T> The type of {@link Server} this client is for
 */
public abstract class Client<T extends Server> {
    protected InetAddress IpAddress;
    protected int port = -1;
    protected T socketServer;
    protected boolean connected = true;
    protected ChannelHandlerContext channel;

    public Client(T server) throws IOException {
        this.socketServer = server;
    }

    public abstract void listen();

    /**
     * Get the IP Address of this Client. Most implementations should return a valid {@link InetAddress}. If this Client
     * does not support IP Addresses, then null will be returned
     * @return The {@link InetAddress} of this Client, otherwise null if not supported
     */
    public InetAddress getIpAddress() {
        return IpAddress;
    }

    public void attachChannel(ChannelHandlerContext channel) {
        if (this.channel != null)
            throw new IllegalStateException("Channel already set for client!");

        this.channel = channel;
    }

    /**
     * Get the port this Client is connected to. Most implementations should return a valid port. If this Client does not
     * ports, then -1 will be returned.
     * @return The port this Client is connected to, otherwise -1 if not supported
     */
    public int getPort() {
        return port;
    }

    /**
     * Set the port this Client is connected to. This value may only be set once!
     * @param port The port this Client is connected to
     */
    public void setPort(int port) {
        if (this.port != -1)
            throw new IllegalStateException("This client already has a UDP Port!");

        this.port = port;
    }

    /**
     * Set the {@link InetAddress} of this Client
     * @param ipAddress The {@link InetAddress} of this Client
     */
    public void setIpAddress(InetAddress ipAddress) {
        this.IpAddress = ipAddress;
    }

    @Deprecated
    public boolean isLoggedIn() {
        return port != -1 && IpAddress != null;
    }

    /**
     * Disconnect this Client
     * @throws IOException If there was a problem disconnecting this Client
     */
    public void disconnect() throws IOException {
        connected = false;

        onDisconnect();
    }

    protected abstract void onDisconnect() throws IOException;

    /**
     * Write data to this Client
     * @param data The data to write
     * @throws IOException If there was a problem writing the data
     */
    public abstract void write(byte[] data) throws IOException;

    /**
     * Read data into a byte array from this Client
     * @param into The array to read into
     * @param offset Where to start writing data into the array
     * @param length How much data to read
     * @return How much data was actually read from the Client, or -1 if there is no more data
     * @throws IOException If there was a problem reading data
     */
    public abstract int read(byte[] into, int offset, int length) throws IOException;

    public abstract void flush() throws IOException;

    /**
     * Get the {@link Server} this Client came from
     * @return The {@link Server} instance this Client came from
     */
    public T getServer() {
        return socketServer;
    }

    public boolean isConnected() {
        return connected;
    }
}
