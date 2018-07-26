using UnityEngine;

public class GyroTest : MonoBehaviour
{

	// Use this for initialization
	void Start () {
		ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
	}

	private void OnInfoUpdate(SensorInformation info)
	{
		info.orientation.x = 0;
		info.orientation.z = 0;
		gameObject.transform.rotation = info.orientation;
	}
}
