
public class DepthEvent : IEvent
{
    public byte EventId
    {
        //Pick a random number for this SimpleEvent
        get { return 3; }
    }

    public int[][] data;
}