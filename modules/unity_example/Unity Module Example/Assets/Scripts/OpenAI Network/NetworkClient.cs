
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

public class NetworkClient : AsyncMonoBehavior, IClient
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

    [HideInInspector]
    public TcpClient client;

    public Socket socket
    {
        get { return client != null ? client.Client : null; }
    }

    public bool Connected
    {
        get { return socket != null && socket.Connected; }
    }

    //private Dictionary<string, UnityAction<SensorInformation>> sensorCallbacks = new Dictionary<string, UnityAction<SensorInformation>>();
    //private Dictionary<byte, EventCallback> eventCallbacks = new Dictionary<byte, EventCallback>();
    //private Dictionary<byte, Type> eventCache = new Dictionary<byte, Type>();
	
    private Queue<Action> _actions = new Queue<Action>();

    void Start()
    {
        PostBackgroundJob(delegate
        {
            Debug.Log("Is Connected? " + Connected);
            while (Connected)
            {
                byte[] opcode = new byte[1];
                int r = socket.Receive(opcode, 0, 1, 0);

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
    
    void FixedUpdate()
    {
        while (_actions.Count > 0)
        {
            _actions.Dequeue()();
        }
    }
    
    void OnDestroy() {
        if (socket != null && socket.Connected)
        {
            socket.Disconnect(false);
            socket.Close();
        }
    }

    private void OnApplicationQuit()
    {
        if (socket != null && socket.Connected)
        {
            socket.Disconnect(false);
            socket.Close();
        }
    }

    public void Send(byte[] data)
    {
        socket.BeginSend(data, 0, data.Length, 0, SendCallback, socket);
    }
    
    private void SendCallback(IAsyncResult ar) {  
        try {  
            // Retrieve the socket from the state object.  
            Socket client = (Socket) ar.AsyncState;  

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);  
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);  
        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
    }
}