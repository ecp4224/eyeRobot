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

	public double score;
	private bool toScore = true;
	[HideInInspector]public bool toCount = true;
	
	private float unity_delay = .35f;
	private bool unity_toMove = false;
	public bool force_movement = true;
	
	public bool isTimer;
	private bool simulate_rotate = false;
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
			
			//this.score += 100;
			
			if (toCount)
			{
				this.score += 100;
				Playground.instance.winCount++;
				toCount = false;
			}
			
			Playground.instance.distanceToDestination = -2;


		}
		else if (col.CompareTag("FLoor"))
		{
			
			
			if (toCount){
				this.score -= 100;
				Playground.instance.lossCount++;
				toCount = false;
			}
			
			Playground.instance.distanceToDestination = -1;
	}
	}
	
	void Start()
	{
		simulate_rotate = ModuleClient.Instance == null;
		
		if (!simulate_rotate)
			ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
	}

	void Update()
	{

		if (Input.GetKeyDown(KeyCode.Tab))
		{
			got_first = false;
		}
		
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
				this.transform.position += transform.forward * moveSpeed * Time.deltaTime;
			
		}else if (Input.GetKey(KeyCode.S))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
				this.transform.position -= transform.forward * moveSpeed * Time.deltaTime;
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
			if (simulate_rotate) this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);
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
			if (simulate_rotate) this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
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
			controllerValue1 = maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;

			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);

			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
			var distance = Vector3.Distance(this.transform.position, Playground.instance.destinationPos);

			this.transform.position += transform.forward * moveSpeed * Time.deltaTime;

			if (toScore)
			{
				if (Vector3.Distance(this.transform.position, Playground.instance.destinationPos) < distance)
				{
					score++;
				}
				else
				{
					score -= 2;
				}

				toScore = false;
			}
	}else if (NetworkInput.GetKey(KeyCode.S))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
			
			//SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
			
			//StartCoroutine(DelayUnityMovement(unity_delay));
			//if(unity_toMove)
			var distance = Vector3.Distance(this.transform.position, Playground.instance.destinationPos);

			this.transform.position -= transform.forward * moveSpeed * Time.deltaTime;

			if (toScore)
			{
				if (Vector3.Distance(this.transform.position, Playground.instance.destinationPos) < distance)
				{
					score++;
				}
				else
				{
					score -= 2;
				}

				toScore = false;
			}
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
			if (simulate_rotate) this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);

			if (toScore)
			{
				score--;

				toScore = false;
			}
				
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
			if (simulate_rotate) this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
			
			if (toScore)
			{
				score--;

				toScore = false;
			}
		}
		else
		{
			controllerValue1 = 0f;
			controllerValue2 = 0f;
			controllerValue3 = 0f;
			controllerValue4 = 0f;

			toScore = true;

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

	private bool got_first = false;
	private Quaternion inital;
	private void OnInfoUpdate(SensorInformation info)
	{
		motor1 = info.motor1;
		motor2= info.motor2;
		motor3 = info.motor3;
		motor4 = info.motor4;
		
		info.orientation.x = 0;
		info.orientation.z = 0;
		if (!got_first)
		{
			inital = info.orientation;
			got_first = true;
		}
		
		var newRotation = new Quaternion(transform.rotation.x, info.orientation.y - inital.y, transform.rotation.z, info.orientation.w);
		
		gameObject.transform.rotation = newRotation;

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
