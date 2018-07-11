public class PressKey : Packet
{
    protected override void onHandlePacket(IClient client)
    {
        int key = consume(4).asInt();
        
        NetworkInput.Instance.PressKey(key);
    }
}