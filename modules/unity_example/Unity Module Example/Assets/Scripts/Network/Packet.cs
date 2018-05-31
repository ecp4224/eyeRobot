using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEngine;

/**
 * This class builds a Packet for a specified {@link Server} and a specified {@link Client}
 * @param <T> The type of {@link Server} this packet is meant for
 * @param <C> The type of {@link Client} this packet is meant for
 */
public class Packet : MonoBehaviour
{

    public byte opcode;
    
    [NonSerialized]
    private byte[] udpData;
    [NonSerialized]
    private MemoryStream tempWriter;
    [NonSerialized]
    private ModuleClient client;
    [NonSerialized]
    private bool ended;
    [NonSerialized]
    private int pos;

    [NonSerialized]
    private bool preserve;
    [NonSerialized]
    private byte[] preservedData;
    
    public void reuseFor(ModuleClient client) {
        this.client = client;
        ended = false;
        pos = 0;
    }

    protected int getPosition() {
        return pos;
    }

    protected void setPosition(int pos) {
        this.pos = pos;
    }

    protected bool isEnded() {
        return ended;
    }

    /**
     * Complete this packet and send it over TCP. This will execute {@link Client#write(byte[])} with
     * the resulting byte array
      */
    public void endTCP() {
        if (client == null)
            return;

        if (tempWriter != null) {
            byte[] data = endBytes();
            client.Send(data);
        } else {
            end();
        }
    }

    public byte[] endBytes() {
        if (preserve && preservedData != null)
            return preservedData;

        byte[] toReturn = new byte[0];
        if (tempWriter != null)
        {
            toReturn = tempWriter.ToArray();
            tempWriter.Close();
            tempWriter.Dispose();
        }

        if (!preserve) {
            end();
        } else {
            preservedData = toReturn;
        }

        return toReturn;
    }

    private void end() {
        tempWriter = null;
        ended = true;
        Destroy(this);
    }

    /**
     * Read a certain amount of data as a {@link ConsumedData}. This can be used to
     * transform the read data into a Java primitive
     * @param length How much data to read
     * @return A {@link ConsumedData} object to allow easy transformation of the data
     * @throws IOException If there was a problem reading the data
     * @see ConsumedData
     */
    protected ConsumedData consume(int length) {
        if (ended)
            throw new IOException("This packet has already ended!");

        if (udpData == null) {
            byte[] data = new byte[length];
            int endPos = pos + length;
            int i = 0;
            while (pos < endPos)
            {
                int r = client.serverSocket.Receive(data, i, length - i, 0);
                if (r == -1)
                    throw new IndexOutOfRangeException();
                pos += r;
                i += r;
            }

            return new ConsumedData(data);
        } else {
            byte[] data = new byte[length];

            if (pos + length > this.udpData.Length)
                throw new IndexOutOfRangeException();

            Array.Copy(this.udpData, pos, data, 0, length);
            //System.arraycopy(this.udpData, pos, data, 0, length);
            pos += length;

            return new ConsumedData(data);
        }
    }

    /**
     * Read a single byte or the entire packet as a {@link ConsumedData}. This can be used to
     * transform the read data into a Java primitive. Whether this method reads a single byte or the entire packet depends on
     * whether this packet is reading from a {@link java.io.InputStream} or a byte array. If from a {@link java.io.InputStream}, then
     * it will return a single byte, otherwise the entire packet
     * @return A {@link ConsumedData} object to allow easy transformation of the data
     * @throws IOException If there was a problem reading the data
     * @see ConsumedData
     */
    protected ConsumedData consume() {
        if (ended)
            throw new IOException("This packet has already ended!");

        if (udpData == null) {
            byte[] data = new byte[1];
            int r = client.serverSocket.Receive(data, 0, 1, 0);
            pos += data.Length;
            return new ConsumedData(data);
        } else {
            int toRead = udpData.Length - pos;
            return consume(toRead);
        }
    }

    public Packet write<T>(T obj) {
        if (preserve && preservedData != null)
            return this;

        String json = JsonUtility.ToJson(obj);
        byte[] toWrite = Encoding.ASCII.GetBytes(json);

        if (toWrite.Length > 600) { //Only ever gzip the json if it's bigger than 0.6kb
            byte[] data;
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    gzip.Write(toWrite, 0, toWrite.Length);
                }

                data = stream.ToArray();
            }
            
            //4 + to include size of uncompressed
            write(4 + data.Length); //The size of this chunk
            write(toWrite.Length); //Size of uncompressed json
            write(data); //compressed json
        } else {
            write(4 + toWrite.Length); //The size of this chunk
            write(toWrite.Length); //Size of json string
            write(toWrite); //json
        }
        return this;
    }

    public Packet write(byte[] val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(val, 0, val.Length);
        return this;
    }

    public Packet write(byte[] val, int offset, int length) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(val, offset, length);
        return this;
    }

    /**
     * Write a byte into this packet
     * @param val The byte value
     * @return This packet
     */
    public Packet write(byte val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(new[] { val }, 0 , 1);
        return this;
    }

    /**
     * Write an int into this packet
     * @param val The int value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(int val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(BitConverter.GetBytes(val), 0, 4);
        return this;
    }

    /**
     * Write a float into this packet
     * @param val The float value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(float val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(BitConverter.GetBytes(val), 0, 4);
        return this;
    }

    /**
     * Write a double into this packet
     * @param val The double value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(double val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(BitConverter.GetBytes(val), 0, 8);
        return this;
    }

    /**
     * Write a long into this packet
     * @param val The long value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(long val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(BitConverter.GetBytes(val), 0, 8);
        return this;
    }

    /**
     * Write a short into this packet
     * @param val The short value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(short val) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(BitConverter.GetBytes(val), 0, 2);
        return this;
    }

    /**
     * Write a String into this packet, encoded as ASCII
     * @param string The String value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(string str) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        byte[] data = Encoding.ASCII.GetBytes(str);
        tempWriter.Write(data, 0, data.Length);
        return this;
    }

    /**
     * Write a String into this packet encoded as a given {@link Charset}
     * @param string The String value
     * @param charset The charset to encode the String as
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(String str, Encoding charset) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        byte[] data = charset.GetBytes(str);
        tempWriter.Write(data, 0, data.Length);
        return this;
    }

    /**
     * Write a boolean into this packet. This will write 1 byte, 1 being true and 0 being false
     * @param value The boolean value
     * @throws IOException if there was a problem writing the value
     * @return This packet
     */
    public Packet write(bool value) {
        if (preserve && preservedData != null)
            return this;

        validateTempStream();
        tempWriter.Write(new [] { value ? (byte)1 : (byte)0 }, 0, 1);
        return this;
    }

    /**
     * Append the current size of this packet to the front of the packet. This is useful for dynamic length packets
     * @return This packet
     * @throws IOException If there was an error creating the packet
     */
    public Packet appendSizeToFront() {
        if (preserve && preservedData != null)
            return this;

        if (tempWriter == null)
            throw new ArgumentException();

        byte[] currentData = tempWriter.ToArray();
        tempWriter.Close();
        tempWriter.Dispose();
        tempWriter = null; //Reset writer

        write(currentData[0]); //Write opCode first
        //We add 4 to include the space for this new info (the packet size)
        write(currentData.Length + 4); //Then append size of packet to front of packet
        write(currentData, 1, currentData.Length - 1); //Then write rest of packet
        return this;
    }

    private void validateTempStream() {
        if (tempWriter == null)
            tempWriter = new MemoryStream();
    }

    /**
     * Read the contents of this packet and perform logic
     * @return This packet
     * @throws IOException If there was a problem reading the packet
     */
    public Packet handlePacket(ModuleClient client, byte[] data) {
        this.client = client;
        this.udpData = data;
        tempWriter = null;
        ended = false;
        pos = 0;

        onHandlePacket(client);
        return this;
    }

    public Packet handlePacket(ModuleClient client) {
        this.client = client;
        this.udpData = null;
        tempWriter = null;
        ended = false;
        pos = 0;

        onHandlePacket(client);
        return this;
    }

    /**
     * Start writing this packet with the given data
     * @param args The data for this packet
     * @return This packet
     * @throws IOException If there was a problem reading the packet
     */
    public Packet writePacket(ModuleClient client, params object[] args)
    {
        this.client = client;
        preserve = false;
        preservedData = null;
        onWritePacket(client, args);
        return this;
    }

    public Packet writeAndPreservePacket(ModuleClient client, params object[] args)
    {
        this.client = client;
        preserve = true;
        onWritePacket(client, args);
        return this;
    }

    protected virtual void onHandlePacket(ModuleClient client) {
        throw new ArgumentException();
    }

    protected virtual void onWritePacket(ModuleClient client, params object[] args) {
        throw new ArgumentException();
    }

    public void dispose() {
        preservedData = null;
        udpData = null;
        client = null;

        if (tempWriter != null) {
            tempWriter.Close();
            tempWriter.Dispose();

            tempWriter = null;
        }
    }
}
