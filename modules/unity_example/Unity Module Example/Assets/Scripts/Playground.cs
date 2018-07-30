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
	public Vector3 destinationPos;
	public Vector3 offset;
	public float startTime;
	//public float elapsedTime;
	public bool isDest=false;
	private Vector3 initPos;
	private Quaternion initRot;

	public Text episodeText;
	

	public static Playground instance;

	[Header("UI")] 
	public Text winCountText;
	public Text lossCountText;
	public Text epocheCountText;
	public Text AIScore;
	public Text AIHighestScore;

	public int winCount = 0;
	public int lossCount = 0;

	private int highScore=-100000;
	
	


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

	void Update()
	{

		//UPDATE Destination Pos
		if (currentDestination != null)
		{
			if(destinationPos!=currentDestination.transform.position)
				destinationPos = currentDestination.transform.position;
		}


		//UPDATE UI
		winCountText.text = "Win: " + winCount.ToString();
		lossCountText.text = "Loss: " + lossCount.ToString();
		AIScore.text = "Current: " + System.Math.Round(SimpleCarController.instance.score, 2).ToString();
		//epocheCountText.text = 
		
		
		
		
		
	if (Input.GetKeyDown(KeyCode.Space))
		{
			ResetGame(0, 0);
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

		Debug.Log ("Go: " + distanceToDestination);
	
	
	
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
	
	public void ResetGame(int episode, int steps)
	{
		distanceToDestination = 0;
		
		Debug.Log("Episode " + episode);
		epocheCountText.text = "Current Epoche: " + episode;
		
		var car = SimpleCarController.instance;
		
		if (car.transform.position != initPos)
		{
			car.transform.position = initPos;
		}

		if (car.transform.rotation != initRot)
		{
			car.transform.rotation = initRot;
		}

		SimpleCarController.instance.score /= (double) steps;

		if (SimpleCarController.instance.score > this.highScore)
		{
			this.highScore = (int)SimpleCarController.instance.score;
			AIHighestScore.text = "Highest: " + this.highScore;
		}

		SimpleCarController.instance.score = 0;
		SimpleCarController.instance.toCount = true;

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

