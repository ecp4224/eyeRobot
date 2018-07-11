using Newtonsoft.Json;

public class TriggerEventPacket : Packet
{
    protected override void onWritePacket(IClient client, params object[] args)
    {
        IEvent @event = args[0] as IEvent;
        string filter = args[1] as string;

        if (@event == null)
            return;

        string json = JsonConvert.SerializeObject(@event);
        bool shouldFilter = !string.IsNullOrEmpty(filter);

        write((byte) 0x06);
        write(@event.EventId);
        write(shouldFilter);
        write(json.Length);
        write(json);

        if (shouldFilter)
        {
            write(filter.Length);
            write(filter);
        }

        appendSizeToFront();
        endTCP();
    }
}