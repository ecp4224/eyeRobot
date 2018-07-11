using UnityEngine;

public class NewScore : Packet
{
    protected override void onWritePacket(IClient client, params object[] args)
    {
        float score = (float) args[0];

        write((byte) 0x06);
        write(score);
        endTCP();
    }
}