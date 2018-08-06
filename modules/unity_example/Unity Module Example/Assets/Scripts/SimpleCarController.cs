using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
This script is used to control a physical robot and its virtual counterpart regarding its movements.
This also keeps track of the scoring system used to assess the AI.
*/
public class SimpleCarController : MonoBehaviour
{

	//score used to assess AI performance
	public double score;
	private bool toScore = true;
	[HideInInspector]public bool toCount = true;
	
	//we had offset delays in order to parallelize the virtual and physical world
	private float unity_delay = .35f;
	private bool unity_toMove = false;
	public bool force_movement = true;
	
	public bool isTimer;
	private bool simulate_rotate = false;
	private float timeForTable;

	//distance is used to assess where the destination is
	[DistanceVariable]
	public float distance;
	
	//Individual values for each motor on the car
	public float motor1, motor2, motor3, motor4;
	public float maxMotorTorque;

	//ControllerValues are used to set values for the motors
	private float controllerValue1, controllerValue2, controllerValue3, controllerValue4;
	float maxSpeed=255f;

	private float unityMaxSpeed = 14.3f;
	private float acceleration = 10f;

	private Rigidbody _rigidbody;

	Vector3 accData;
	bool dirty=false;
	
	//Movement Stats
	public float moveSpeed=7f;
	public float rotateSpeed=35f;

	public float endTime;
	
	//Singleton reference
	public static SimpleCarController instance;

	void Awake(){
		instance = this;
	}

	//This function gets called when the Robot collides with another object(Destination or Floor)
	void OnTriggerEnter(Collider col)
	{
		//Destination = good reward for the AI
		if (col.CompareTag("Destination"))
		{
		
			endTime = Time.time;
			Debug.Log ("Time of Completion: " + (endTime - Playground.instance.startTime));
			
			//ensure that the score is only added to the total once
			if (toCount)
			{
				this.score += 100;
				Playground.instance.winCount++;
				toCount = false;
			}
			
			//-2 Distance used to communicate with AI(tell it goodjob)
			Playground.instance.distanceToDestination = -2;


		}
		//Floor = BAD reward for the AI, it has fallen off of the course
		else if (col.CompareTag("FLoor"))
		{
			
			//ensure that the score is only deducted from the total once
			if (toCount){
				this.score -= 100;
				Playground.instance.lossCount++;
				toCount = false;
			}
			
			//-1 Distance used to communicate with AI(tell it BADjob)
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

		//setup code to get initial rotation
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			got_first = false;
		}
		
		//Used to time the robot, how long it took to reach the destination
		if (isTimer)
		{
			timeForTable += Time.deltaTime;
		}

		
		/*
		Following code takes in Inputs WASD from user and moves the robot depending on the input.
		W: Forward movement
		S: Backward movement
		A: Left rotation
		D: Right rotation
		*/
		if (Input.GetKey(KeyCode.W))
		{
			controllerValue1=maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;

			this.transform.position += transform.forward * moveSpeed * Time.deltaTime;
			
		}else if (Input.GetKey(KeyCode.S))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 = -maxSpeed;
			controllerValue4 = -maxSpeed;
			
			this.transform.position -= transform.forward * moveSpeed * Time.deltaTime;
		}
		else if (Input.GetKey(KeyCode.A))
		{
			controllerValue1=-maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 =-maxSpeed;
			controllerValue4 =maxSpeed;
			
			if (simulate_rotate) this.transform.Rotate (-Vector3.up * rotateSpeed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.D))
		{
			controllerValue1=maxSpeed;
			controllerValue2 = -maxSpeed;
			controllerValue3 =maxSpeed;
			controllerValue4 =-maxSpeed;
			
			if (simulate_rotate) this.transform.Rotate (Vector3.up * rotateSpeed * Time.deltaTime);
		}
		else
		{
			controllerValue1 = 0f;
			controllerValue2 = 0f;
			controllerValue3 = 0f;
			controllerValue4 = 0f;
		}

		/*
		Following code takes in Inputs WASD from AI &moves the robot depending on the input.
		W: Forward movement
		S: Backward movement
		A: Left rotation
		D: Right rotation
		It also takes into account the current distance it is away from the destination and determines if 
		the movement taken is positive or negative toward the goal.
		*/
		if (NetworkInput.GetKey(KeyCode.W))
		{
			controllerValue1 = maxSpeed;
			controllerValue2 = maxSpeed;
			controllerValue3 = maxSpeed;
			controllerValue4 = maxSpeed;

			//Distance taken from the moment before the movement is applied
			var distance = Vector3.Distance(this.transform.position, Playground.instance.destinationPos);

			this.transform.position += transform.forward * moveSpeed * Time.deltaTime;

			//toScore used to only allocate scores once at a time
			if (toScore)
			{
				//Check if the distance between the robot and Destination got larger or smaller
				if (Vector3.Distance(this.transform.position, Playground.instance.destinationPos) < distance)
				{	
					//allocate score if distance was reduced
					score++;
				}
				else
				{
					//otherwise penalize the AI
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

	//Send Robot Command takes 4 values and uses them to set the values of the Motors on the robot
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
		
		//we are only interested in the left and right rotations of the robot (Y axis)
		info.orientation.x = 0;
		info.orientation.z = 0;
		if (!got_first)
		{
			//obtain initial rotation of the robot using the gyroscope
			inital = info.orientation;
			got_first = true;
		}
		
		//from the initial rotation, any changes in rotation will kept in check
		var newRotation = new Quaternion(transform.rotation.x, info.orientation.y - inital.y, transform.rotation.z, info.orientation.w);
		
		//All changes in rotation is also applied to the virtual robot
		gameObject.transform.rotation = newRotation;

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

	//Offset to parallize virtual and physical world
	IEnumerator DelayUnityMovement(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		unity_toMove = PhysicalIsMoving() || force_movement;
	}

	//bool to check if the physical robot is moving
	public bool PhysicalIsMoving()
	{
		return TestStop.instance.isMoving;
	}

}
