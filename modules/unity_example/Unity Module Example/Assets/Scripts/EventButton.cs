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
		//ModuleClient.Instance.ListenFor<SimpleEvent>(OnSimpleEvent);
	}

	public void TriggerEvent()
	{
		DepthEvent e = new DepthEvent();
		e.data = new[]
		{
			new int[10],
			new int[10],
			new int[10]
		};

		ModuleClient.Instance.TriggerEvent(e);
	}
}
