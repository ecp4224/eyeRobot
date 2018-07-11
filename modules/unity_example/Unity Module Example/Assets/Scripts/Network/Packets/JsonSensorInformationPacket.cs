using UnityEngine;

public class JsonSensorInformationPacket : Packet
{
    protected override void onHandlePacket(IClient client)
    {
        int robotLength = consume(4).asInt();

        string robot = consume(robotLength).asString();
        
        int jsonLength = consume(4).asInt();

        string json = consume(jsonLength).asString();

        SensorInformation info = JsonUtility.FromJson<SensorInformation>(json);

        ModuleClient.Instance.NewInfoFor(robot, info);
    }
}