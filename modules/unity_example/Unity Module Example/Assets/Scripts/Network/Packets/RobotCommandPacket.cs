using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotCommandPacket : Packet {

	protected override void onWritePacket(IClient client, params object[] args)
	{
		int motor1 = (int) args[0];
		int motor2 = (int) args[1];
		int motor3 = (int) args[2];
		int motor4 = (int) args[3];
		string name = (string) args[4];
		
		write((byte)0x08);
		write(motor1);
		write(motor2);
		write(motor3);
		write(motor4);
		write(name.Length);
		write(name);
		appendSizeToFront();
		endTCP();
	}
}
