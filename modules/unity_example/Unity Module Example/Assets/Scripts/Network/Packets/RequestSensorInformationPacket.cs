using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestSensorInformationPacket : Packet {

	protected override void onWritePacket(IClient client, params object[] args)
	{
		string robot = (string)args[0];
		string filter = (string) args[1];

		bool hasFilter = !string.IsNullOrEmpty(filter);

		write((byte) 0x07);
		write(robot.Length);
		write(hasFilter);
		write(robot);

		if (hasFilter)
		{
			write(filter.Length);
			write(filter);
		}

		appendSizeToFront();
		endTCP();
	}
}
