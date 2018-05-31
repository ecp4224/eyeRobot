using UnityEngine;

public class EventPacket : Packet
{
    protected override void onHandlePacket(ModuleClient client)
    {
        int packetSize = consume(4).asInt(); //Ignore

        byte eventId = consume(1).asByte();
        int ownerLength = consume(4).asInt();
        int eventLength = consume(4).asInt();

        string moduleOwner = consume(ownerLength).asString();
        string json = consume(eventLength).asString();
        
        ModuleClient.Instance.RaiseEvent(eventId, json, moduleOwner);
    }
}