using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
// public class AxleInfo {
// 	public WheelCollider leftWheel;
// 	public WheelCollider rightWheel;
// 	public bool motor;
// 	public bool steering;
// }
     
[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
	public bool isTimer;
	private float timeForTable;
	
	//public List<AxleInfo> axleInfos;

	public float motor1, motor2, motor3, motor4;
	public float maxMotorTorque;

	private float controllerValue1, controllerValue2, controllerValue3, controllerValue4;
	float maxSpeed=255f;

	private float unityMaxSpeed = 14.3f;
	private float acceleration = 10f;

	private Rigidbody _rigidbody;


	//NEW MOVEMENT STUFF
	public float moveSpeed=7f;
	public float rotateSpeed=35f;


	void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag("Finish"))
		{
			Debug.Log(col.gameObject.name);
			isTimer = false;
		}
	}
	
	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
		ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
		ModuleClient.Instance.ListenFor<DepthEvent>(OnDepthData);
	}

	void OnDepthData (DepthEvent args, string module)
	{
		Debug.Log ("Got Depth (" + args.data.Length + "x" + args.data [0].Length + ")");
	}

	void Update()
	{


		if (isTimer)
		{
			timeForTable += Time.deltaTime;
			//Debug.Log(timeForTable);
		}
		else
		{
			if(timeForTable>0)
				Debug.Log(timeForTable);
		}


		
		if (Input.GetKey(KeyCode.W))
		{
			isTimer = true;

			controllerValue1=maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;




		}else if (Input.GetKey(KeyCode.S))
		{
			

			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;



		}
		else if (Input.GetKey(KeyCode.A))
		{

	

			controllerValue1=-maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 =-maxSpeed;
			controllerValue4 =maxSpeed;



		}
		else if (Input.GetKey(KeyCode.D))
		{

			controllerValue1=maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 =maxSpeed;
			controllerValue4 =-maxSpeed;



		}
		else
		{
			controllerValue1 = 0f;
			controllerValue2 = 0f;
			controllerValue3 = 0f;
			controllerValue4 = 0f;
		}

		ModuleClient.Instance.SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	}



	private void OnInfoUpdate(SensorInformation arg0)
	{
		motor1 = arg0.motor1;
		motor2= arg0.motor2;
		motor3 = arg0.motor3;
		motor4 = arg0.motor4;

		if (Input.GetKey(KeyCode.W))
		{
			//Debug.Log ("did eddie lie to me");
			float forwardMult=1.11f;
			this.transform.localPosition += transform.forward * moveSpeed * forwardMult * Time.deltaTime;



		}else if (Input.GetKey(KeyCode.S))
		{

			this.transform.localPosition -= transform.forward * moveSpeed * Time.deltaTime;




		}
		else if (Input.GetKey(KeyCode.A))
		{

			this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);




		}
		else if (Input.GetKey(KeyCode.D))
		{

			//Debug.Log ("this rotating right");
			this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);




		}

		
	
	}

	// finds the corresponding visual wheel
	// correctly applies the transform
	public void ApplyLocalPositionToVisuals(WheelCollider collider)
	{
		if (collider.transform.childCount == 0) {
			return;
		}
     
		Transform visualWheel = collider.transform.GetChild(0);
     
		Vector3 position;
		Quaternion rotation;
		collider.GetWorldPose(out position, out rotation);
     
		//visualWheel.transform.position = position;
		//visualWheel.transform.rotation = rotation;
	}

	public void FixedUpdate()
	{
//		var frontleft = axleInfos[0].leftWheel;
//		var frontright = axleInfos[0].rightWheel;
//		var backleft = axleInfos[1].leftWheel;
//		var backright = axleInfos[1].rightWheel;

		//Find the speed by the square root of the velocity of the rigidbody of your car
		var speed = _rigidbody.velocity.sqrMagnitude;

//		Debug.Log(speed + " : " + _rigidbody.velocity);

		//Only add motorTorque if your speed is less then max speed
		//if (Input.GetKey(KeyCode.W))
		//{
		if (speed < unityMaxSpeed)
		{
			
//			frontleft.motorTorque = (motor1 / 255f) * maxMotorTorque;
//			frontright.motorTorque = (motor2 / 255f) * maxMotorTorque;
//			backleft.motorTorque = (motor3 / 255f) * maxMotorTorque;
//			backright.motorTorque = (motor4 / 255f) * maxMotorTorque;
		}
	//}
	else
	{
//			frontleft.motorTorque = 0f;
//			frontright.motorTorque = 0f;
//			backleft.motorTorque = 0f;
//			backright.motorTorque = 0f;
	}
		
//		ApplyLocalPositionToVisuals(frontleft);
//		ApplyLocalPositionToVisuals(frontright);
//		ApplyLocalPositionToVisuals(backleft);
//		ApplyLocalPositionToVisuals(backright);


//		frontleft.brakeTorque = 0f;
//		frontright.brakeTorque = 0f;
//		backleft.brakeTorque = 0f;
//		backright.brakeTorque = 0f;

		/*float motor = maxMotorTorque * Input.GetAxis("Vertical");
		
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
     
		foreach (AxleInfo axleInfo in axleInfos) {
			if (axleInfo.steering) {
				//axleInfo.leftWheel.steerAngle = steering;
				//axleInfo.rightWheel.steerAngle = steering;
				axleInfo.rightWheel.motorTorque = steering;
			}
			if (axleInfo.motor) {
				axleInfo.leftWheel.motorTorque = motor;

			}
			ApplyLocalPositionToVisuals(axleInfo.leftWheel);
			ApplyLocalPositionToVisuals(axleInfo.rightWheel);
		}*/
	}
}
