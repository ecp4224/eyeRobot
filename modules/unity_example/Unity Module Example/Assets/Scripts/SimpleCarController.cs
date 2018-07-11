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

	AccelEvent accData;
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
			Playground.instance.isDest = false;
			Destroy (col.gameObject);
		}
	}
	
	void Start()
	{
		ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
		ModuleClient.Instance.ListenFor<AccelEvent>(OnAccelData);
	}

	void OnAccelData (AccelEvent args, string module)
	{
		Debug.Log ("Got args Acceleration: x = " + args.accX + " y = " + args.accY + " z = " + args.accZ + " .");

		accData=args;
		dirty = true;

		Debug.Log ("Yaw: " + Yaw (new Vector4 (args.accX, args.accY, args.accZ, args.tilt)));
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
			this.transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;

			isTimer = true;
			controllerValue1=maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;
		}else if (Input.GetKey(KeyCode.S))
		{
			this.transform.localPosition -= transform.forward * moveSpeed * Time.deltaTime;

			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);

			controllerValue1=-maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 =-maxSpeed;
			controllerValue4 =maxSpeed;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);

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

		//
		//
		//

		if (NetworkInput.GetKey(KeyCode.W))
		{
			this.transform.localPosition += transform.forward * moveSpeed * Time.deltaTime;

			isTimer = true;
			controllerValue1=maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;
		}else if (NetworkInput.GetKey(KeyCode.S))
		{
			this.transform.localPosition -= transform.forward * moveSpeed * Time.deltaTime;

			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
		}
		else if (NetworkInput.GetKey(KeyCode.A))
		{
			this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);

			controllerValue1=-maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 =-maxSpeed;
			controllerValue4 =maxSpeed;
		}
		else if (NetworkInput.GetKey(KeyCode.D))
		{
			this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);

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

		//ModuleClient.Instance.SendRobotCommand((int)controllerValue1, (int)controllerValue2, (int)controllerValue3, (int)controllerValue4);
	}

	private void OnInfoUpdate(SensorInformation arg0)
	{
		motor1 = arg0.motor1;
		motor2= arg0.motor2;
		motor3 = arg0.motor3;
		motor4 = arg0.motor4;

		//reference to x accel

		//reference to z accel

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
	}

}
