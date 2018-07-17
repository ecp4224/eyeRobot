using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Playground : MonoBehaviour
{
	[DistanceVariable]
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
	private Vector3 initPos;
	private Quaternion initRot;

	public Text episodeText;
	

	public static Playground instance;


	void Awake(){
	
		instance = this;
	
	}

	void Start(){
		GameServer.Instance.RegisterGameManager(this);
		GameServer.Instance.OnReset(ResetGame);
		//this.transform.localScale = new Vector3 (X, 1f, Y);
		UpdatePlayground();
		initPos = SimpleCarController.instance.transform.position;
		initRot = SimpleCarController.instance.transform.rotation;

		//ResetGame();
		
		SpawnDestination();
	}

	void Update(){

		if (Input.GetKeyDown(KeyCode.Space))
		{
			ResetGame(0, 0f);
		}
		if (Input.GetMouseButtonDown (0)) {
		
			SetDestination ();
			startTime = Time.time;

			//Debug.Log ("Destination: " + destination);
		}

		if (currentDestination != null)
		{
			if (distanceToDestination >= 0)
			{
				distanceToDestination = Vector3.Distance(SimpleCarController.instance.transform.position,
					currentDestination.transform.position);
			}
		}

		Debug.Log (distanceToDestination);
	
	
	
	}

	public void RespawnDestination()
	{
		if (currentDestination != null)
			Destroy(currentDestination.gameObject);
		
		currentDestination = null;

		distanceToDestination = 0f;
		
		SpawnDestination();
	}
	
	public void SpawnDestination()
	{
		//+-1 for leeway from the cliff
		float x = Random.Range((-X / 2)+1, (X / 2)-1);
		float y = Random.Range((-Y / 2)+1, (Y / 2)-1);

		isDest = true;
		
		GameObject destination = Instantiate(DestinationPrefab, initPos + new Vector3(x, .77f, y), Quaternion.identity);

		currentDestination = destination;
	}
	
	public void ResetGame(int episode, float timer)
	{
		distanceToDestination = 0;
		
		Debug.Log("Episode " + episode);
		episodeText.text = "Episode " + episode;
		
		var car = SimpleCarController.instance;
		
		if (car.transform.position != initPos)
		{
			car.transform.position = initPos;
		}

		if (car.transform.rotation != initRot)
		{
			car.transform.rotation = initRot;
		}
		
	}



	public void UpdatePlayground(){
		
		var dest = new Vector3 (X, 1f, Y);
		if(this.transform.localScale!=dest) this.transform.localScale = dest;

	}

	public void SetDestination(){

		if (currentDestination != null)
			Destroy(currentDestination.gameObject);
		
		currentDestination = null;
		
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

