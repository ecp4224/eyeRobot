public class ReleaseKey : Packet
{
    protected override void onHandlePacket(IClient client)
    {
        int key = consume(4).asInt();
        
        NetworkInput.Instance.ReleaseKey(key);
    }
}