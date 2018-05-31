using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketFactory : MonoBehaviour
{
	public static PacketFactory Instance
	{
		get { return _instance; }
	}

	private static PacketFactory _instance;

	public List<Packet> Packets;
	private Dictionary<byte, Packet> cache = new Dictionary<byte, Packet>();

	public Packet this[byte opcode]
	{
		get { return cache[opcode]; }
	}

	void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
	
	// Use this for initialization
	void Start () {
		foreach (var packet in Packets)
		{
			cache.Add(packet.opcode, packet);
		}
	}
}
