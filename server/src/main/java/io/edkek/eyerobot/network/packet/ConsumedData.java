package io.edkek.eyerobot.network.packet;

import io.edkek.eyerobot.utils.Global;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.charset.Charset;
import java.util.zip.GZIPInputStream;

/**
 * Represented {@link ConsumedData} that can be transformed into a Java primitive
 */
public class ConsumedData {
    private byte[] data;
    private ByteBuffer buffer;

    ConsumedData(byte[] data, boolean flip) {
        this.data = data;
        buffer = ByteBuffer.allocate(data.length);
        buffer.order(ByteOrder.LITTLE_ENDIAN);
        buffer.put(data);
        buffer.position(0);
        if (flip)
            buffer.flip();
    }

    ConsumedData(byte[] data) {
        this(data, false);
    }

    /**
     * Transform this data to an int
     * @return The int value
     */
    public int asInt() {
        return buffer.getInt();
    }

    /**
     * Transform this data into a long
     * @return The long value
     */
    public long asLong() {
        return buffer.getLong();
    }

    /**
     * Transform this data into a float
     * @return The float value
     */
    public float asFloat() {
        return buffer.getFloat();
    }

    /**
     * Transform this data into a double
     * @return The double value
     */
    public double asDouble() {
        return buffer.getDouble();
    }

    /**
     * Transform this data into a short
     * @return The short value
     */
    public short asShort() {
        return buffer.getShort();
    }

    /**
     * Transform this data into a boolean. This read a single byte and returns true if the value is 1, or false if the value is 0
     * @return The boolean value
     */
    public boolean asBoolean() {
        return buffer.get() == 1;
    }

    /**
     * Transform this data into a String, decoded using ASCII
     * @return The String value
     */
    public String asString() {
        return new String(data);
    }

    /**
     * Transform this data into a String, decoded using the provided {@link Charset}
     * @param charset The charset to use for decoding
     * @return The String value
     */
    public String asString(Charset charset) {
        return new String(data, charset);
    }

    /**
     * Transform this data into a single byte
     * @return The byte value
     */
    public byte asByte() {
        return data[0];
    }

    public <T> T as(Class<T> class_) throws IOException {
        int uncompressedLength = buffer.getInt();
        int remain = buffer.remaining();
        byte[] data = new byte[remain];
        buffer.get(data, 0, data.length);

        String json;
        if (uncompressedLength > 600) {
            ByteArrayInputStream tempStream = new ByteArrayInputStream(data);
            GZIPInputStream inputStream = new GZIPInputStream(tempStream);
            byte[] uncompressedData = new byte[uncompressedLength];

            int i = 0;
            while (i < uncompressedData.length) {
                int read = inputStream.read(uncompressedData, i, uncompressedLength - i);
                i += read;
            }

            json = new String(uncompressedData, Charset.forName("ASCII"));

            data = null;
            uncompressedData = null;
            inputStream.close();
            tempStream.close();
        } else {
            json = new String(data, Charset.forName("ASCII"));
        }

        return Global.GSON.fromJson(json, class_);
    }

    public byte[] raw() {
        return data;
    }
}
