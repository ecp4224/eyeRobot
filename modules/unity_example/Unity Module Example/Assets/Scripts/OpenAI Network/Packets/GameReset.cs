using UnityEngine;

public class GameReset : Packet {
	protected override void onHandlePacket(IClient client)
	{
		int episode = consume(4).asInt();
		int steps = consume(4).asInt();
		
		GameServer.Instance.DoReset(episode, steps);
	}
}
