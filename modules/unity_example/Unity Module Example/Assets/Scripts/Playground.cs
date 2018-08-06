using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Playground : MonoBehaviour
{
	//Distance between the robot and the destination
	[DistanceVariable]
	public float distanceToDestination;

	//Setup determines the size of the playground
	[Header("Setup")]
	public float X=10f;
	public float Y=10f;
	public GameObject DestinationPrefab;

	//Debugging variables to configure the scene
	[Header("Debugging")]
	public GameObject currentDestination;
	public Vector3 destinationPos;
	public Vector3 offset;
	public float startTime;
	public bool isDest=false;
	private Vector3 initPos;
	private Quaternion initRot;

	//UI for episodes
	public Text episodeText;
	
	//Singleton reference
	public static Playground instance;

	//General UI
	[Header("UI")] 
	public Text winCountText;
	public Text lossCountText;
	public Text epocheCountText;
	public Text AIScore;
	public Text AIHighestScore;

	//AI successes and failures
	public int winCount = 0;
	public int lossCount = 0;

	//Keep track of the current highscore of the training AI
	private int highScore=-100000;
	
	//Singleton GET
	void Awake(){
	
		instance = this;
	
	}

	
	//On start we configure the GameServer as well as the inclient settings to reset the robot when an episode is complete
	void Start(){
		GameServer.Instance.RegisterGameManager(this);
		GameServer.Instance.OnReset(ResetGame);
	
		UpdatePlayground();
		initPos = SimpleCarController.instance.transform.position;
		initRot = SimpleCarController.instance.transform.rotation;

		//On start, spawn a destination for the AI to reach
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
		
		//Press Space to reset the game state, used for manual training
		if (Input.GetKeyDown(KeyCode.Space))
		{
			ResetGame(0, 0);
		}
		
		//To reset the destination of the game, left click anywhere on the playground
		if (Input.GetMouseButtonDown (0)) {
		
			SetDestination ();
			startTime = Time.time;

		}

		//Checks if a destination exists
		if (currentDestination != null)
		{
			//If the current distance between the destination and client is NOT negative
			if (distanceToDestination >= 0)
			{
				//Set the distance of the destination and client.
				distanceToDestination = Vector3.Distance(SimpleCarController.instance.transform.position,
					currentDestination.transform.position);
			}
		}
	}

	//If there is currently a destination, destroy the current one before spawning a new destination.
	public void RespawnDestination()
	{
		if (currentDestination != null)
			Destroy(currentDestination.gameObject);
		
		currentDestination = null;

		distanceToDestination = 0f;
		
		SpawnDestination();
	}
	
	//Spawn destination at a random point in the playground within its size constraints.
	public void SpawnDestination()
	{
		//+-1 for leeway from the cliff
		float x = Random.Range((-X / 2)+1, (X / 2)-1);
		float y = Random.Range((-Y / 2)+1, (Y / 2)-1);

		isDest = true;
		
		GameObject destination = Instantiate(DestinationPrefab, initPos + new Vector3(x, .77f, y), Quaternion.identity);

		currentDestination = destination;
	}
	
	//Resets the game state
	public void ResetGame(int episode, int steps)
	{
		distanceToDestination = 0;
		
		Debug.Log("Episode " + episode);
		epocheCountText.text = "Current Epoche: " + episode;
		
		var car = SimpleCarController.instance;
		
		//Restore initial position of robot
		if (car.transform.position != initPos)
		{
			car.transform.position = initPos;
		}
		
		//Restore initial rotation of robot
		if (car.transform.rotation != initRot)
		{
			car.transform.rotation = initRot;
		}

		//Calculate the final score of the AI
		SimpleCarController.instance.score /= (double) steps;

		if (SimpleCarController.instance.score > this.highScore)
		{
			this.highScore = (int)SimpleCarController.instance.score;
			AIHighestScore.text = "Highest: " + this.highScore;
		}

		SimpleCarController.instance.score = 0;
		SimpleCarController.instance.toCount = true;

	}

	//If any of the settings change for the size of the Playground, the playground will update.
	public void UpdatePlayground(){
		
		var dest = new Vector3 (X, 1f, Y);
		if(this.transform.localScale!=dest) this.transform.localScale = dest;

	}

	//Raycasts a mouseclick onto the world scene in order to spawn a destination cube.
	public void SetDestination(){

		if (currentDestination != null)
			Destroy(currentDestination.gameObject);
		
		currentDestination = null;
		
		isDest = true;
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast (ray, out hit)) {

			Vector2 mousePos = new Vector2 ();

			GameObject destination = Instantiate (DestinationPrefab, hit.point+offset, Quaternion.identity) as GameObject;

			currentDestination = destination;
		}
	
	}

}
