using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using Newtonsoft.Json;

public class ModuleClient : AsyncMonoBehavior, IClient
{
	// State object for receiving data from the server.  
	public class StateObject {  
		// Client socket.  
		public Socket workSocket = null;  
		// Size of receive buffer.  
		public const int BufferSize = 1024;  
		// Receive buffer.  
		public byte[] buffer = new byte[BufferSize];

		//Buffering packet
		public int currIndex;
		public byte[] packet = new byte[BufferSize];
	}

	public class EventCallback
	{
		public MethodInfo method;
		public object target;
	}
	
	public static ModuleClient Instance
	{
		get { return _instance; }
	}
	
	private static ModuleClient _instance;

	public string ip = "127.0.0.1";
	public short port = 1337;

	public string moduleName = "Example";
	
	public Socket socket
	{
		get { return serverSocket; }
	}

	[HideInInspector]
	public Socket serverSocket;

	[HideInInspector] 
	public bool Connected = false;

	private Dictionary<string, UnityAction<SensorInformation>> sensorCallbacks = new Dictionary<string, UnityAction<SensorInformation>>();
	private Dictionary<byte, EventCallback> eventCallbacks = new Dictionary<byte, EventCallback>();
	private Dictionary<byte, Type> eventCache = new Dictionary<byte, Type>();
	
	private Queue<Action> _actions = new Queue<Action>();

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
		
		serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		//Connect in a background thread, otherwise unity will lock up
		serverSocket.BeginConnect(ip, port, ServerConected, null);
	}

	void FixedUpdate()
	{
		while (_actions.Count > 0)
		{
			_actions.Dequeue()();
		}
	}

	void OnDestroy() {
		if (serverSocket.Connected)
		{
			serverSocket.Disconnect(false);
			serverSocket.Close();
		}
	}

	private void OnApplicationQuit()
	{
		if (serverSocket.Connected)
		{
			serverSocket.Disconnect(false);
			serverSocket.Close();
		}
	}

	private void ServerConected(IAsyncResult ar)
	{
		if (!serverSocket.Connected)
		{
			Debug.LogError("Failed to connect to server!");
			Debug.LogError("Is the server running on " + ip + ":" + port);
			return;
		}
	
		Debug.Log("Connected to server!");
		SendSession();
		Connected = true;

		PostBackgroundJob(delegate
		{
			while (Connected)
			{
				byte[] opcode = new byte[1];
				int r = serverSocket.Receive(opcode, 0, 1, 0);

				if (r == 0)
				{
					//TODO Disconnect
					break;
				}

				Packet p = PacketFactory.Instance[opcode[0]];
				if (p == null)
				{
					Debug.Log("Invalid packet: " + opcode[0]);
					//TODO Invalid
					//TODO Disconnect
					break;
				}

				p.handlePacket(this);
			}
		});
	}

	public void Send(byte[] data)
	{
		serverSocket.BeginSend(data, 0, data.Length, 0, SendCallback, serverSocket);
	}

	public void SendRobotCommand(int motor1, int motor2, int motor3, int motor4, string robot = "eyeRobot")
	{
		RobotCommandPacket packet = gameObject.AddComponent<RobotCommandPacket>();
		packet.writePacket(this, motor1, motor2, motor3, motor4, robot);
	}

	public void RequestSensorInformation(string robot, UnityAction<SensorInformation> onInfoUpdate, string filter = "")
	{
		sensorCallbacks.Add(robot, onInfoUpdate);

		RequestSensorInformationPacket packet = gameObject.AddComponent<RequestSensorInformationPacket>();
		packet.writePacket(this, robot, filter);
	}

	private void SendSession()
	{
		byte[] toSend = new byte[3 + moduleName.Length];

		toSend[0] = 0x00;
		toSend[1] = 1;
		toSend[2] = (byte) moduleName.Length;

		byte[] namebytes = Encoding.ASCII.GetBytes(moduleName);
		
		Array.Copy(namebytes, 0, toSend, 3, namebytes.Length);
		
		serverSocket.BeginSend(toSend, 0, toSend.Length, 0, SendCallback, serverSocket);
	}
	
	private void SendCallback(IAsyncResult ar) {  
		try {  
			// Retrieve the socket from the state object.  
			Socket client = (Socket) ar.AsyncState;  

			// Complete sending the data to the remote device.
			int bytesSent = client.EndSend(ar);  
			Console.WriteLine("Sent {0} bytes to server.", bytesSent);  
		} catch (Exception e) {  
			Console.WriteLine(e.ToString());  
		}  
	}

	public void NewInfoFor(string robot, SensorInformation info)
	{
		_actions.Enqueue(delegate { sensorCallbacks[robot](info); });
	}

	public void TriggerEvent(IEvent simpleEvent, string moduleFilter = "")
	{
		TriggerEventPacket packet = gameObject.AddComponent<TriggerEventPacket>();
		packet.writePacket(this, simpleEvent, moduleFilter);
	}

	public void ListenFor<T>(UnityAction<T, string> callback) where T : IEvent, new()
	{
		//Extract event ID by create a temp event
		T temp = new T();
		byte id = temp.EventId;
		
		//Check the type cache
		if (!eventCache.ContainsKey(id))
		{
			eventCache.Add(id, typeof(T));
		}

		var callbackData = new EventCallback
		{
			method = callback.Method,
			target = callback.Target
		};
		
		eventCallbacks.Add(id, callbackData);
	}

	public void RaiseEvent(byte id, string json, string moduleOwner)
	{
		if (!eventCache.ContainsKey(id))
			return;

		Type t = eventCache[id];
		object obj = JsonConvert.DeserializeObject(json, t);
		//object obj = JsonUtility.FromJson(json, t);

		var callback = eventCallbacks[id];

		_actions.Enqueue(delegate
		{
			callback.method.Invoke(callback.target, new[] { obj, moduleOwner });
		});
	}
}
