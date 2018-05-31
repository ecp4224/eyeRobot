using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventButton : MonoBehaviour
{
	public Text text;
	
	// Use this for initialization
	void Start ()
	{
		ModuleClient.Instance.ListenFor<SimpleEvent>(OnSimpleEvent);
	}

	public void TriggerEvent()
	{
		SimpleEvent e = new SimpleEvent();
		e.Hello = "hi";
		e.World = 42;

		ModuleClient.Instance.TriggerEvent(e);
	}

	void OnSimpleEvent(SimpleEvent @event, string moduleOwner)
	{
		Debug.Log("Got event! " + @event.World);

		text.text = "Last Event:\n" +
		            "From: " + moduleOwner + "\n" +
		            "Data: \n" +
		            "\tHello: " + @event.Hello + "\n" +
		            "\tWorld: " + @event.World;
	}
}
