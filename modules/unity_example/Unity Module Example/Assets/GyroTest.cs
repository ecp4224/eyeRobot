using UnityEngine;

public class GyroTest : MonoBehaviour
{

	// Use this for initialization
	void Start () {
		ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
	}

	private void OnInfoUpdate(SensorInformation info)
	{	
		gameObject.transform.rotation = info.orientation;
	}
}
