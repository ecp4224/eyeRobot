using UnityEngine;

public class GameReset : Packet {
	protected override void onHandlePacket(IClient client)
	{
		int episode = consume(4).asInt();
		int timer = consume(4).asInt();

		float f_timer = timer / 60f;
		
		GameServer.Instance.DoReset(episode, f_timer);
	}
}
