using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class GameServer : AsyncMonoBehavior
{
	
	public static GameServer Instance
	{
		get { return _instance; }
	}
	
	private static GameServer _instance;

	private PacketFactory packets;

	[HideInInspector]
	[BindResource("Prefabs/NetworkClient")]
	public GameObject ClientPrefab;
	
	public short port = 1337;

	[HideInInspector] public TcpListener listener;

	[HideInInspector] 
	public bool Connected = false;
	
	private Queue<Action> _actions = new Queue<Action>();
	private LinkedList<UnityAction<int, int>> _resets = new LinkedList<UnityAction<int, int>>();
	private LinkedList<UnityAction<Vector3, Vector3>> _movements = new LinkedList<UnityAction<Vector3, Vector3>>();

	private Func<float> scorer;
	private GameObject scorer_owner;
	void Awake()
	{
		if (_instance == null)
		{
			DontDestroyOnLoad(gameObject);
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		
		UnityBinder.Inject(this);
		
		listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
		listener.Start();

		listener.BeginAcceptTcpClient(OnClientAccepted, listener);

		packets = GetComponent<PacketFactory>();
	}

	public void RegisterGameManager(MonoBehaviour script)
	{
		var bindingFlags = BindingFlags.Instance |
		                   BindingFlags.NonPublic |
		                   BindingFlags.Public;
		
		var fields = script.GetType().GetFields(bindingFlags);

		foreach (var field in fields)
		{
			var distance = (DistanceVariable[])field.GetCustomAttributes(typeof(DistanceVariable), true);

			if (distance.Length > 0)
			{
				if (field.FieldType == typeof(float))
				{
					var field1 = field;
					scorer = () => (float) field1.GetValue(script);
					scorer_owner = script.gameObject;
					break;
				}
			}
		}

		if (scorer == null)
		{
			Debug.LogError("Found no DistanceVariable attribute in provided GameManager " +
			               "(" + script.GetType().FullName + ")! Make a float variable" +
			               "representing the robot's distance to the current goal and put [DistanceVariable] above" +
			               "the new variable. This variable will be monitored and sent to OpenAI to calculate score");
		}
	}

	public void OnReset(UnityAction<int, int> GameReset)
	{
		_resets.AddLast(GameReset);
	}

	public void DoReset(int episode, int steps)
	{
		_actions.Enqueue(delegate
		{
			foreach (var resetCallback in _resets)
			{
				resetCallback(episode, steps);
			}
		});
	}

	private void OnClientAccepted(IAsyncResult ar)
	{
		TcpListener listener = ar.AsyncState as TcpListener;
		if (listener == null)
			return;

		try
		{
			var client = listener.EndAcceptTcpClient(ar);
			
			_actions.Enqueue(delegate
			{
				Debug.Log("Got client!");
				
				var newClient = Instantiate(ClientPrefab);
				var networkClient = newClient.GetComponent<NetworkClient>();

				networkClient.client = client;
				networkClient.packets = packets;
			});
		}
		finally
		{
			listener.BeginAcceptTcpClient(OnClientAccepted, listener);
		}
	}

	void FixedUpdate()
	{
		while (_actions.Count > 0)
		{
			var temp = _actions.Dequeue();
			if (temp == null)
				continue;
			temp();
		}
	}

	public void OnMovement(UnityAction<Vector3, Vector3> action)
	{
		_movements.AddLast(action);
	}

	public void DoKinectMovement(Vector3 acceleration, Vector3 velocity)
	{
		_actions.Enqueue(delegate
		{
			foreach (var movementCallback in _movements)
			{
				movementCallback(acceleration, velocity);
			}
		});
	}

	public void RequestScoring(IClient client)
	{
		if (scorer == null)
		{
			Debug.LogError("OpenAI requested current distance but no monitor is active! Make a float variable" +
			               "representing the robot's distance to the current goal and put [DistanceVariable] above" +
			               "the new variable. This variable will be monitored and sent to OpenAI to calculate score." +
			               "Once the variable is made, invoke GameServer.Instance.RegisterGameManager() to register" +
			               "the script holding the variable.");
		}
		
		_actions.Enqueue(delegate
		{
			float distance = scorer();

			var packetWriter = gameObject.GetComponent<NewScore>();
			if (packetWriter == null)
			{
				packetWriter = gameObject.AddComponent<NewScore>();
			}

			packetWriter.writePacket(client, distance, scorer_owner.transform.rotation, scorer_owner.transform.position);
		});
	}
}

[AttributeUsage(AttributeTargets.Field)]
public class DistanceVariable : Attribute { }
