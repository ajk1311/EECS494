using UnityEngine;
using System.Collections;
using ProtoBuf;
using GameProtobuf;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System;
using UnityThreading;
using System.Collections.Specialized;

public class ServerConnection : MonoBehaviour {

	Socket	listenSocket;
	int		listenPort;
	bool 	running = true;
	// Use this for initialization
	void Start () {
		listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		listenSocket.Bind (new IPEndPoint(IPAddress.Any, 0));
		listenPort = ((IPEndPoint)listenSocket.LocalEndPoint).Port;
		Debug.Log ("local info: port " + listenPort + " ip address: " + GetLocalIP());
		Thread.InBackground (() => {
			connect ("JJHSIUNG");
		});
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDestroy ()
	{
		running = false;
	}

	void connect(string username)
	{
		using (TcpClient client = new TcpClient("10.0.0.16", 9191))
		{
			ClientInfo clientInfo = new ClientInfo()
			{
				name = username,
				port = listenPort
			};
			
			using (NetworkStream s = client.GetStream())
			{
				Serializer.SerializeWithLengthPrefix(s, clientInfo, PrefixStyle.Base128);
				int ack = s.ReadByte();
				if(running)
					s.WriteByte((byte)ack);
				else
					s.WriteByte((byte)0);
				ClientInfo opponent = Serializer.DeserializeWithLengthPrefix<ClientInfo>(s, PrefixStyle.Base128);
				Debug.Log(clientInfo.name + "'s opponent is " + opponent.name + " " + opponent.address + " " + opponent.port + " " + opponent.isHost);
				ConnectToOpponent(opponent);
			}
		}
	}

	/** Returns the local IPv4 address of this machine */
	static String GetLocalIP()
	{
		IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress address in hostEntry.AddressList)
		{
			if (address.AddressFamily == AddressFamily.InterNetwork)
			{
				return address.ToString();
			}
		}
		return null;
	}

	private void ConnectToOpponent(ClientInfo opponentInfo)
	{
		ConnectInputStream (opponentInfo);
		ConnectOutputStream (opponentInfo);
	}

	private void ConnectOutputStream(ClientInfo opponentInfo) {
		Thread.InBackground (() => {
			using (TcpClient client = new TcpClient(opponentInfo.address, opponentInfo.port))
			{
				using (Stream stream = client.GetStream())
				{
					StreamWriter sw = new StreamWriter(stream);
					sw.AutoFlush = true;
					try{
						while(running){
							sw.WriteLine("jjhsiung sent this!");
							System.Threading.Thread.Sleep(5000);
						}
					} catch(IOException ioe){
						Debug.Log ("shit broke :(");
					}
				}
			}
		});
	}

	private void ConnectInputStream(ClientInfo opponentInfo)
	{
		Thread.InBackground (() => {
			listenSocket.Listen(50);
			Debug.Log ("step0");
			Socket reader = listenSocket.Accept();
			Debug.Log ("step1");
			StreamReader streamReader = new StreamReader(new NetworkStream(reader));
			Debug.Log ("step2");
			while(running)
			{
				string input = streamReader.ReadLine();
				if(input == null)
				{
					break;
				}
				Debug.Log (input);
			}
			Debug.Log("killing thread");
			streamReader.Close();
			reader.Close();
			listenSocket.Close();
		});
	}


}
