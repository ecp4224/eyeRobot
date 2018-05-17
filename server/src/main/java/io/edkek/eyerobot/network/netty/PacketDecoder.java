package io.edkek.eyerobot.network.netty;

import io.edkek.eyerobot.network.PacketFactory;
import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.handler.codec.ByteToMessageDecoder;

import java.nio.ByteOrder;
import java.util.List;

public class PacketDecoder extends ByteToMessageDecoder {
    /**
     * This is where the raw bytes netty receives are chunked into packets.
     *
     * The packet size is determined by an opcode. Every packet should start at an opcode and end on the byte
     * before the next opcode
     *
     * The chunked packet stored should include the opcode, so it can be identified later on by the PacketFactory
     * @param ctx The channel this came from
     * @param byteBuf The current byte buffer in the pipeline
     * @param results Where the final packet is stored
     * @throws Exception
     */
    @Override
    protected void decode(ChannelHandlerContext ctx, ByteBuf byteBuf, List<Object> results) throws Exception {
        if (byteBuf.readableBytes() == 0)
            return;

        byte opCode = byteBuf.getByte(0);
        int packetSize = -1;
        if (opCode == 0) {
            //This block is the Session packet

            byteBuf = byteBuf.order(ByteOrder.LITTLE_ENDIAN); //Set this here for the handler
            short sessionLength = byteBuf.getShort(2);
            //packet size is the 2 bytes for Type and Length
            //and the value of the Length or the byte at [2] ([opcode, Type, Length, ..]
            //and the single byte for the opcode
            packetSize = 2 + sessionLength + 1;
        } else {
            //The PacketFactory can provide us the size of the packet represented by the opcode
            //plus one to include the opcode itself
            packetSize = PacketFactory.packetSize(opCode) + 1;
        }

        //We got an unknown opcode
        if (packetSize == -1) {
            System.err.println("Unknown op code: " + opCode);
        } else if (packetSize == -2) { //Size is first 4 bytes of packet
            packetSize = byteBuf.order(ByteOrder.LITTLE_ENDIAN).getInt(1); //Size should be after the opCode
        }

        //We don't have the full packet yet, return until we get more
        if (byteBuf.readableBytes() < packetSize)
            return;

        //Read the chunked packets
        byte[] packet = new byte[packetSize];
        byteBuf.readBytes(packetSize).getBytes(0, packet);

        //Store the result
        results.add(packet);
    }
}
