using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEvent : IEvent
{   
    public byte EventId
    {
        //Pick a random number for this SimpleEvent
        get { return 4; }
    }

    public string Hello;
    public int World;
}
