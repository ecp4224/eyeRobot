using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

/**
 * Represented {@link ConsumedData} that can be transformed into a Java primitive
 */
public class ConsumedData {
    private byte[] data;

    public ConsumedData(byte[] data, bool flip) {
        this.data = data;
    }

    public ConsumedData(byte[] data) : this(data, false) { }

    /**
     * Transform this data to an int
     * @return The int value
     */
    public int asInt()
    {
        return BitConverter.ToInt32(data, 0);
    }

    /**
     * Transform this data into a long
     * @return The long value
     */
    public long asLong()
    {
        return BitConverter.ToInt64(data, 0);
    }

    /**
     * Transform this data into a float
     * @return The float value
     */
    public float asFloat()
    {
        return BitConverter.ToSingle(data, 0);
    }

    /**
     * Transform this data into a double
     * @return The double value
     */
    public double asDouble()
    {
        return BitConverter.ToDouble(data, 0);
    }

    /**
     * Transform this data into a short
     * @return The short value
     */
    public short asShort()
    {
        return BitConverter.ToInt16(data, 0);
    }

    /**
     * Transform this data into a boolean. This read a single byte and returns true if the value is 1, or false if the value is 0
     * @return The boolean value
     */
    public bool asBoolean() {
        return data[0] == 1;
    }

    /**
     * Transform this data into a String, decoded using ASCII
     * @return The String value
     */
    public String asString()
    {
        return Encoding.ASCII.GetString(data);
    }

    /**
     * Transform this data into a String, decoded using the provided {@link Charset}
     * @param charset The charset to use for decoding
     * @return The String value
     */
    public String asString(Encoding charset)
    {
        return charset.GetString(data);
    }

    /**
     * Transform this data into a single byte
     * @return The byte value
     */
    public byte asByte() {
        return data[0];
    }

    public T asObject<T>()
    {
        int uncompressedLength = BitConverter.ToInt32(data, 0);
        int remain = data.Length - 4;
        byte[] d = new byte[remain];
        
        Array.Copy(data, 4, d, 0, d.Length);

        string json;
        if (uncompressedLength > 600)
        {
            byte[] uncompressedData = new byte[uncompressedLength];
            using (MemoryStream stream = new MemoryStream(d))
            {
                using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    int i = 0;
                    while (i < uncompressedData.Length)
                    {
                        int read = gzip.Read(uncompressedData, i, uncompressedLength - i);
                        i += read;
                    }
                }
            }

            json = Encoding.ASCII.GetString(uncompressedData);
        }
        else
        {
            json = Encoding.ASCII.GetString(d);
        }

        return JsonUtility.FromJson<T>(json);
    }

    public byte[] raw() {
        return data;
    }
}
