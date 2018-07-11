using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectMovement : Packet {

	protected override void onHandlePacket(IClient client)
	{
		int accelerationX = consume(4).asInt();
		int accelerationY = consume(4).asInt();
		int accelerationZ = consume(4).asInt();
		int velocityX = consume(4).asInt();
		int velocityY = consume(4).asInt();
		int velocityZ = consume(4).asInt();
		
		Vector3 acceleration = new Vector3(accelerationX, accelerationY, accelerationZ);
		Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);

		GameServer.Instance.DoKinectMovement(acceleration, velocity);
	}
}
