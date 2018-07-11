
public class AccelEvent : IEvent
{
    public byte EventId
    {
        //Pick a random number for this SimpleEvent
        get { return 5; }
    }

	public float tilt;
	public float accX;
	public float accY;
	public float accZ;
}