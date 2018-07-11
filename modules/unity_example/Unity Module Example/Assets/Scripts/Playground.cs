using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playground : MonoBehaviour {

	//[DistanceVariable]
	public float distanceToDestination;

	[Header("Setup")]
	public float X=10f;
	public float Y=10f;
	public GameObject DestinationPrefab;

	[Header("Debugging")]
	public GameObject currentDestination;
	public Vector3 destination;
	public Vector3 offset;
	public float startTime;
	public bool isDest=false;

	public static Playground instance;


	void Awake(){
	
		instance = this;
	
	}

	void Start(){

		//this.transform.localScale = new Vector3 (X, 1f, Y);
		UpdatePlayground();

	}

	void Update(){
	
		if (Input.GetMouseButtonDown (0)) {
		
			SetDestination ();
			startTime = Time.time;

			//Debug.Log ("Destination: " + destination);
		
		}

		if (currentDestination != null)
			distanceToDestination = Vector3.Distance (SimpleCarController.instance.transform.position, currentDestination.transform.position);

		Debug.Log (distanceToDestination);
	
	
	
	}



	public void UpdatePlayground(){
		
		var dest = new Vector3 (X, 1f, Y);
		if(this.transform.localScale!=dest) this.transform.localScale = dest;

	}

	public void SetDestination(){
	
		if(currentDestination!=null) 
			return;
		
		isDest = true;
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit)) {

			Vector2 mousePos = new Vector2 ();
			//destination = hit.point;

			GameObject destination = Instantiate (DestinationPrefab, hit.point+offset, Quaternion.identity) as GameObject;
			currentDestination = destination;
		}
	
	}

}

