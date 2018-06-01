using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
 public class AxleInfo {
 	public WheelCollider leftWheel;
 	public WheelCollider rightWheel;
 	public bool motor;
 	public bool steering;
 }
     
[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour {
	public List<AxleInfo> axleInfos;

	public float motor1, motor2, motor3, motor4;
	public float maxMotorTorque;
	public float maxSpeed;

	private Rigidbody _rigidbody;

	void Start()
	{
		_rigidbody = GetComponent<Rigidbody>();
		ModuleClient.Instance.RequestSensorInformation("eyeRobot", OnInfoUpdate);
	}

	private void OnInfoUpdate(SensorInformation arg0)
	{
		motor1 = arg0.motor1;
		motor2= arg0.motor2;
		motor3 = arg0.motor3;
		motor4 = arg0.motor4;
		
		var frontleft = axleInfos[0].leftWheel;
		var frontright = axleInfos[0].rightWheel;
		var backleft = axleInfos[1].leftWheel;
		var backright = axleInfos[1].rightWheel;

		frontleft.brakeTorque = Mathf.Infinity;
		frontright.brakeTorque = Mathf.Infinity;
		backleft.brakeTorque = Mathf.Infinity;
		backright.brakeTorque = Mathf.Infinity;
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
     
		visualWheel.transform.position = position;
		visualWheel.transform.rotation = rotation;
	}
     
	public void FixedUpdate()
	{
		var frontleft = axleInfos[0].leftWheel;
		var frontright = axleInfos[0].rightWheel;
		var backleft = axleInfos[1].leftWheel;
		var backright = axleInfos[1].rightWheel;
		
		//Find the speed by the square root of the velocity of the rigidbody of your car
		var speed = _rigidbody.velocity.sqrMagnitude;
		
		Debug.Log(speed + " : " + _rigidbody.velocity);
       
		//Only add motorTorque if your speed is less then max speed
		if(speed < maxSpeed) {
			frontleft.motorTorque = (motor1 / 255f) * maxMotorTorque;
			frontright.motorTorque = (motor2 / 255f) * maxMotorTorque;
			backleft.motorTorque = (motor3 / 255f) * maxMotorTorque;
			backright.motorTorque = (motor4 / 255f) * maxMotorTorque;
		}
		else
		{
			frontleft.motorTorque = 0f;
			frontright.motorTorque = 0f;
			backleft.motorTorque = 0f;
			backright.motorTorque = 0f;
		}
		
		ApplyLocalPositionToVisuals(frontleft);
		ApplyLocalPositionToVisuals(frontright);
		ApplyLocalPositionToVisuals(backleft);
		ApplyLocalPositionToVisuals(backright);


		frontleft.brakeTorque = 0f;
		frontright.brakeTorque = 0f;
		backleft.brakeTorque = 0f;
		backright.brakeTorque = 0f;

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
