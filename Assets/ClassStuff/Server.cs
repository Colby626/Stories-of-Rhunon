/*using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ServerClient
{
	public int connectionId;
	public string playerName;
	public Vector3 position;
	public Quaternion rotation;
}

public class Server : MonoBehaviour 
{
	private const int MAX_CONNECTION = 100;
	public Rigidbody2D rigidbody2D;

	private int port = 7777;

	private int hostId;

	private int reliableChannel;
	private int unrealiableChannel;

	private bool isStarted = false;
	private byte error = 0;

	private List<ServerClient> clients = new List<ServerClient>();

	private void Start()
	{

		NetworkTransport.Init ();
		ConnectionConfig cc = new ConnectionConfig ();


		reliableChannel = cc.AddChannel (QosType.Reliable); //TCP
		unrealiableChannel = cc.AddChannel (QosType.Unreliable); //UDP

		HostTopology topo = new HostTopology (cc, MAX_CONNECTION);

		hostId = NetworkTransport.AddHost (topo, port, null);

		isStarted = true;
	}

	private void Update()
	{
		if (!isStarted)
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
			Debug.Log("Player" + connectionId + "has connected");
			OnConnection(connectionId);
			break;
		case NetworkEventType.DataEvent:       //3
			string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
			Debug.Log("Receiving from " + connectionId + ": " + msg);

			//this is cool stuff
			string[] splitData = msg.Split('|');

			switch(splitData[0])
			{
				case "MESSAGETOSERVER":
                    OnGetMessage(splitData[1],splitData[2]);
                    break;
				default:
					Debug.Log("Inalid message : " + msg);
					break;
			}
			break;
		case NetworkEventType.DisconnectEvent: //4
			Debug.Log("Player" + connectionId + "has disconnected");
			break;
		}
	}
	private void OnGetMessage(string htifrom, string msg)
	{
		string c = "ChatFromServer|";
		foreach (ServerClient sc in clients)
			c += htifrom + ": " + msg + '|';
		c += c.Trim('|');
		Send(c, unrealiableChannel, clients);
	}
	private void OnConnection(int cnnId)
	{
		//Add him to a list
		ServerClient c = new ServerClient();
		c.connectionId = cnnId;
		c.playerName = "TEMP";
		clients.Add(c);

		//When the player joins give him a id

		//Request his name
		string msg = "ASKNAME|" + cnnId + "|"; 
		//Also send the name of all the other players
		foreach (ServerClient sc in clients)
			msg += sc.playerName + '%' + sc.connectionId + '|';
		//trimming the last pipe
		msg = msg.Trim('|');
		//Now to send the message
		Send(msg,reliableChannel,cnnId);

	}
	private void Send(string message, int channelId, int cnnId)
	{
		List<ServerClient> c = new List<ServerClient>();
		c.Add(clients.Find(x => x.connectionId == cnnId));
		Send(message, channelId, c);
	}
	//overloading
	private void Send(string message, int channelId, List<ServerClient> c)
	{
		Debug.Log("Sending : " + message);
		byte[] msg = Encoding.Unicode.GetBytes(message);
		foreach(ServerClient sc in c)
		{
			NetworkTransport.Send(hostId,sc.connectionId,channelId,msg,message.Length * sizeof(char), out error);
		}
	}
}*/