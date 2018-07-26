using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketFactory : MonoBehaviour
{

	private static PacketFactory _instance;

	public List<Packet> Packets;
	private Dictionary<byte, Packet> cache = new Dictionary<byte, Packet>();

	public Packet this[byte opcode]
	{
		get { return cache[opcode]; }
	}

	void Awake()
	{	
		foreach (var packet in Packets)
		{
			cache.Add(packet.opcode, packet);
		}
	}
}
