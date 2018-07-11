using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OkPacket : Packet {

	protected override void onHandlePacket(IClient client)
	{
		bool val = consume(1).asBoolean();
		
		Debug.Log("Got Ok: " + val);
	}
}
