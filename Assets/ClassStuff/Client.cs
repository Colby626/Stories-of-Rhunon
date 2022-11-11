using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Player
{
	public string playerName;
	public GameObject avatar;
	public int connectionId;
}

public class Client : MonoBehaviour 
{
	private const int MAX_CONNECTION = 100;

	private int port = 7777;

	private int hostId;
	private int webHostId;

	private int reliableChannel;
	private int unrealiableChannel;

	
	private int connectionId;

	private float connectionTime;
	private bool isConnected = false;
	private byte error = 0;

	private string playerName;

	private float lastMovementUpdate;
	public float movementUpdateRate = 0.1f;

	private string cmsg1 = " ";
	private string cmsg2 = " ";
	private string cmsg3 = " ";
	private string cmsg4 = " ";
	private string cmsg5 = " ";
	private string cmsg6 = " ";
	private string cmsg7 = " ";

	public void Connect()
	{
		//Check if player has a name
		string pName = GameObject.Find("InputField (TMP)").GetComponent<TMP_InputField>().text;
		if (pName == "") 
		{
			Debug.Log ("You must enter a name!");
			//return;
		}

		playerName = pName;

		NetworkTransport.Init ();
		ConnectionConfig cc = new ConnectionConfig ();


		reliableChannel = cc.AddChannel (QosType.Reliable); // TCP
		unrealiableChannel = cc.AddChannel (QosType.Unreliable); // UDP

		HostTopology topo = new HostTopology (cc, MAX_CONNECTION);

		hostId = NetworkTransport.AddHost (topo, 0);
		connectionId = NetworkTransport.Connect (hostId, "10.115.3.242", port, 0, out error);

		connectionTime = Time.time;
		isConnected = true;

		SendDebugMessage ("Player : " + playerName + " has joined");
	}

	public void SendMessage()
	{
		string inputMessage =  "MESSAGETOSERVER|" + playerName +'|'+ GameObject.Find("MessageField").GetComponent<TMP_InputField>().text;
		
		Send(inputMessage, unrealiableChannel);
	}

	private void SendDebugMessage(string dm)
	{
		string debugMessage = "MESSAGETOSERVER|Server|" + dm;

		Send (debugMessage, unrealiableChannel);
	}

	public void ReceiveMessage(string msg)
	{
		cmsg7 = cmsg6;
		cmsg6 = cmsg5;
		cmsg5 = cmsg4;
		cmsg4 = cmsg3;
		cmsg3 = cmsg2;
		cmsg2 = cmsg1;
		cmsg1 = msg;
		GameObject.Find("Message1").GetComponent<TMP_Text>().text = cmsg1;
		GameObject.Find("Message2").GetComponent<TMP_Text>().text = cmsg2;
		GameObject.Find("Message3").GetComponent<TMP_Text>().text = cmsg3;
		GameObject.Find("Message4").GetComponent<TMP_Text>().text = cmsg4;
		GameObject.Find("Message5").GetComponent<TMP_Text>().text = cmsg5;
		GameObject.Find("Message6").GetComponent<TMP_Text>().text = cmsg6;
		GameObject.Find("Message7").GetComponent<TMP_Text>().text = cmsg7;
	}

	private void Update()
	{
		if (!isConnected)
			return;

		int recHostId; 
		int connectionId; 
		int channelId; 
		byte[] recBuffer = new byte[1024]; 
		int bufferSize = 1024;
		int dataSize;
		//byte error;
		NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
		switch (recData)
		{
		case NetworkEventType.Nothing:         //1
			break;
		case NetworkEventType.ConnectEvent:    //2
			break;
		case NetworkEventType.DataEvent:       //3
			string msg = System.Text.Encoding.Unicode.GetString(recBuffer, 0, dataSize);
			Debug.Log("Receiving : " + msg);
			//this is cool shit
			string[] splitData = msg.Split('|');

			switch(splitData[0])
			{
				case "ChatFromServer":
                    ReceiveMessage(splitData[1]);
                    break;
				default:
					Debug.Log("Inalid message : " + msg);
					break;
			}
			break;
		case NetworkEventType.DisconnectEvent: //4
			break;
		}
	}

	private void Send(string message, int channelId)
	{
		Debug.Log("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);
		NetworkTransport.Send(hostId,connectionId,channelId,msg,message.Length * sizeof(char), out error);
	}
}