using System.Collections;
using System.Collections.Generic;
using UnityEngine;

<<<<<<< HEAD
public class Playground : MonoBehaviour {

	//[DistanceVariable]
	public float distanceToDestination;

=======
public class Playground : MonoBehaviour
{
	[DistanceVariable]
	public float distanceToDestination;
	
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12
	[Header("Setup")]
	public float X=10f;
	public float Y=10f;
	public GameObject DestinationPrefab;

	[Header("Debugging")]
<<<<<<< HEAD
	public GameObject currentDestination;
=======
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12
	public Vector3 destination;
	public Vector3 offset;
	public float startTime;
	public bool isDest=false;

	public static Playground instance;
<<<<<<< HEAD
=======
	

>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12


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
<<<<<<< HEAD

		if (currentDestination != null)
			distanceToDestination = Vector3.Distance (SimpleCarController.instance.transform.position, currentDestination.transform.position);

		Debug.Log (distanceToDestination);
=======
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12
	
	
	
	}



	public void UpdatePlayground(){
		
		var dest = new Vector3 (X, 1f, Y);
		if(this.transform.localScale!=dest) this.transform.localScale = dest;

	}

	public void SetDestination(){
	
<<<<<<< HEAD
		if(currentDestination!=null) 
			return;
		
=======
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12
		isDest = true;
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit)) {

			Vector2 mousePos = new Vector2 ();
			//destination = hit.point;

			GameObject destination = Instantiate (DestinationPrefab, hit.point+offset, Quaternion.identity) as GameObject;
<<<<<<< HEAD
			currentDestination = destination;
=======
			
>>>>>>> 2db63d233229d1199c808e9e8598b8f2e94b7d12
		}
	
	}

}

