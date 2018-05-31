using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotorButton : MonoBehaviour
{

	public InputField[] fields;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SendMotorCommand()
	{
		int motor1 = int.Parse(fields[0].text);
		int motor2 = int.Parse(fields[1].text);
		int motor3 = int.Parse(fields[2].text);
		int motor4 = int.Parse(fields[3].text);
		
		ModuleClient.Instance.SendRobotCommand(motor1, motor2, motor3, motor4);
	}
}
