using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AsyncMonoBehavior : MonoBehaviour {

	protected Thread PostBackgroundJob(Action action)
	{
		var thread = new Thread(new ThreadStart(action));
		
		thread.Start();

		return thread;
	}
}
