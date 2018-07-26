using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoButton : MonoBehaviour
{

	public Text text;

	public void RequestInfo()
	{
		//ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
	}

	private void OnInfoUpdate(SensorInformation newInformation)
	{
		text.text = "Motor Information:\n" +
		            "\n" +
		            "Motor1: " + newInformation.motor1 + "\n" +
		            "Motor2: " + newInformation.motor2 + "\n" +
		            "Motor3: " + newInformation.motor3 + "\n" +
		            "Motor4: " + newInformation.motor4;
	}
}
