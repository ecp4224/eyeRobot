using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestScore : Packet {
    protected override void onHandlePacket(IClient client)
    {
        //Consume the remaining of the packet
        consume(3);

        GameServer.Instance.RequestScoring(client);
    }
}
