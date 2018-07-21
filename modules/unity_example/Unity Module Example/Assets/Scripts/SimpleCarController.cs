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
     
//[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour
{
	private float unity_delay = .35f;
	private bool unity_toMove = false;
	public bool force_movement = true;
	
	public bool isTimer;
	private float timeForTable;

	[DistanceVariable]
	public float distance;
	
	//public List<AxleInfo> axleInfos;

	public float motor1, motor2, motor3, motor4;
	public float maxMotorTorque;

	private float controllerValue1, controllerValue2, controllerValue3, controllerValue4;
	float maxSpeed=255f;

	private float unityMaxSpeed = 14.3f;
	private float acceleration = 10f;

	private Rigidbody _rigidbody;

	Vector3 accData;
	bool dirty=false;

	//NEW MOVEMENT STUFF
	public float moveSpeed=7f;
	public float rotateSpeed=35f;

	public float endTime;

	public static SimpleCarController instance;

	void Awake(){
		instance = this;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag("Destination"))
		{
			//Debug.Log(col.gameObject.name);
			endTime = Time.time;
			Debug.Log ("Time of Completion: " + (endTime - Playground.instance.startTime));
			//Playground.instance.isDest = false;
			//Destroy (col.gameObject);

			Playground.instance.distanceToDestination = -2;

		}else if (col.CompareTag("FLoor"))
		{
			Playground.instance.distanceToDestination = -1;
		}
	}
	
	void Start()
	{
		ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
	}

	public static double Yaw(Vector4 quaternion){
		double value = 2.0 * (quaternion.w * quaternion.y - quaternion.z * quaternion.x);
		value = value > 1.0 ? 1.0 : value;
		value = value < -1.0 ? -1.0 : value;

		double pitch = Math.Asin (value);
		return pitch * (180.0 / Math.PI);
	}

	void Update()
	{
		
//		if(accData!=null)
//			Debug.Log ("Got accData Acceleration: x = " + accData.accX + " y = " + accData.accY + " z = " + accData.accZ + " .");

		if (isTimer)
		{
			timeForTable += Time.deltaTime;
			//Debug.Log(timeForTable);
		}
		else
		{
//			if(timeForTable>0)
//				Debug.Log(timeForTable);
		}

		if (Input.GetKey(KeyCode.W))
		{

			//isTimer = true;
			controllerValue1=maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;
			
		}else if (Input.GetKey(KeyCode.S))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.localPosition -= transform.forward * moveSpeed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 =-maxSpeed;
			controllerValue4 =maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.D))
		{
			controllerValue1=maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 =maxSpeed;
			controllerValue4 =-maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
		}
		else
		{
			controllerValue1 = 0f;
			controllerValue2 = 0f;
			controllerValue3 = 0f;
			controllerValue4 = 0f;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
		}
		
		if (NetworkInput.GetKey(KeyCode.W))
		{

			//isTimer = true;
			controllerValue1=maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
			this.transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;
			
		}else if (NetworkInput.GetKey(KeyCode.S))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
			
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.localPosition -= transform.forward * moveSpeed * Time.deltaTime;
		}
		else if (NetworkInput.GetKey(KeyCode.A))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 =-maxSpeed;
			controllerValue4 =maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
			
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);
		}
		else if (NetworkInput.GetKey(KeyCode.D))
		{
			controllerValue1=maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 =maxSpeed;
			controllerValue4 =-maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
			
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
		}
		else
		{
			controllerValue1 = 0f;
			controllerValue2 = 0f;
			controllerValue3 = 0f;
			controllerValue4 = 0f;
			
		}
		
		SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);

	}

	private void SendRobotCommand(int val1, int val2, int val3, int val4)
	{
		if (ModuleClient.Instance != null)
		{
			Debug.Log("Do " + val1 + " " + val2 + " " + val3  + " " + val4);
			ModuleClient.Instance.SendRobotCommand(val1, val2, val3, val4);
		}
	}

	private void OnInfoUpdate(SensorInformation arg0)
	{
		motor1 = arg0.motor1;
		motor2= arg0.motor2;
		motor3 = arg0.motor3;
		motor4 = arg0.motor4;

		//reference to x accel

		//reference to z accel

//		if (Input.GetKey(KeyCode.W))
//		{
//			//Debug.Log ("did eddie lie to me");
//			float forwardMult=1.11f;
//			this.transform.localPosition += transform.forward * moveSpeed * forwardMult * Time.deltaTime;
//		}else if (Input.GetKey(KeyCode.S))
//		{
//			this.transform.localPosition -= transform.forward * moveSpeed * Time.deltaTime;
//		}
//		else if (Input.GetKey(KeyCode.A))
//		{
//			this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);
//		}
//		else if (Input.GetKey(KeyCode.D))
//		{
//			//Debug.Log ("this rotating right");
//			this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
//		}
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
	}

	IEnumerator DelayUnityMovement(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		//if (!TestStop.instance.isMoving) return;
		//CheckPhysicalMovement();
		
		unity_toMove = PhysicalIsMoving() || force_movement;
	}

	public bool PhysicalIsMoving()
	{
		return TestStop.instance.isMoving;
	}

}
