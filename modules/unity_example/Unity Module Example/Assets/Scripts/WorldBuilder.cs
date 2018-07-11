using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour {

	public GameObject cubePrefab;
	public Transform world;
	public int threshold=1000;
	bool dirty=false;
<<<<<<< HEAD
	DepthEvent data;

	void Start(){
		
		ModuleClient.Instance.ListenFor<DepthEvent>(OnDepthData);
	
	}

	void OnDepthData (DepthEvent args, string module)
	{
		//Debug.Log ("Got Depth (" + args.data.Length + "x" + args.data [0].Length + ")");
		data=args;
		dirty = true;
	}
=======

	void Start(){
		
		//ModuleClient.Instance.ListenFor<DepthEvent>(OnDepthData);
	
	}

	//void OnDepthData (DepthEvent args, string module)
	//{
		//Debug.Log ("Got Depth (" + args.data.Length + "x" + args.data [0].Length + ")");
		//data=args;
	//	dirty = true;
	//}
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12

	void Update(){

		Draw ();

	}

	void Draw(){
	

<<<<<<< HEAD
		if (data == null || !dirty)
			return;
		//Debug.Log (array);

		for (int i = 0; i < 480; i++) {
=======
		//if (data == null || !dirty)
		//	return;
		//Debug.Log (array);

		/*for (int i = 0; i < 480; i++) {
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12
			for (int j = 0; j < 640; j++) {

				//Debug.Log (data.data [i] [j]);
				if (data.data [i] [j] == 0)
					continue;

				if (data.data [i] [j] < threshold) {
					
					Vector3 spawnPos = Camera.main.ScreenToWorldPoint (new Vector2 (-320, 240) + new Vector2 (i, j));
					Debug.Log (spawnPos);
					var pixel=Instantiate (cubePrefab, spawnPos + transform.forward * ((float)data.data [i] [j]/1000f), Quaternion.identity);
					pixel.transform.parent = world;
				}
			}
		}

		Debug.Log ("Drawn");
<<<<<<< HEAD
		dirty = false;
=======
		dirty = false;*/
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12

	}



}
