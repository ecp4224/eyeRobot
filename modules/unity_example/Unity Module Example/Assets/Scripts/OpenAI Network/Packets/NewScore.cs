using UnityEngine;

public class NewScore : Packet
{
    protected override void onWritePacket(IClient client, params object[] args)
    {
        float score = (float) args[0];
        Quaternion rotation = (Quaternion) args[1];
        Vector3 position = (Vector3) args[2];

        write((byte) 0x06);
        
        write(score);
        
        write(rotation.x);
        write(rotation.y);
        write(rotation.z);
        
        write(rotation.w);
        write(position.x);
        write(position.y);
        write(position.z);
        
        endTCP();
    }
}