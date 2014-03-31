﻿using UnityEngine;

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections;

using EventBus;
using ProtoBuf;
using GameEvents;
using GameProtoBufs;

public class GameSetup
{
    private static readonly string SERVER_IP = "10.0.0.17";

    private static readonly int SERVER_PORT = 9191;

    private static bool connected = false;

    private static bool connecting = false;

	private static object readyLock = new object();

	private static object waitingForLocal = new object();

	private static object waitingForRemote = new object();

	private static bool localReady = false;

	private static bool remoteReady = false;

	private static ClientInfo remoteInfo;

    public static void ConnectToGame(string name)
    {
        connecting = true;
        UnityThreading.Thread.InBackground(() =>
        {
            InitConnection(name);
        });
    }

    public static void CancelGameConnection()
    {
        connecting = false;
    }

    public static void DisconnectFromGame()
    {
        connected = false;
    }

	public static void Ready(int playerID)
	{
		UnityThreading.Thread.InBackground(() =>
		{
			SendReadyEvent(playerID);
		});
	}

    private static void InitConnection(string name)
    {
        MyClientInfo localInfo = GetLocalClientInfo(name);
        remoteInfo = GetRemoteClientInfo(localInfo.wrapped);
		GameConnectionEvent connectionEvent = new GameConnectionEvent
		{
			name = localInfo.wrapped.name,
			ID = localInfo.wrapped.playerID,
			opponentName = remoteInfo.name,
			opponentID = remoteInfo.playerID,
			success = true
		};
		Dispatcher.Instance.Post(connectionEvent);
		WaitForReady(localInfo);
    }

    private static MyClientInfo GetLocalClientInfo(string name)
    {
        Socket localSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);
        localSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        ClientInfo localInfo = new ClientInfo
        {
            name = name,
            port = ((IPEndPoint)localSocket.LocalEndPoint).Port
        };
        return new MyClientInfo
        {
            wrapped = localInfo,
            socket = localSocket
        };
    }

    private static ClientInfo GetRemoteClientInfo(ClientInfo localInfo)
    {
        using (TcpClient client = new TcpClient(SERVER_IP, SERVER_PORT))
        using (NetworkStream stream = client.GetStream())
        {
            Serializer.SerializeWithLengthPrefix(stream, localInfo, PrefixStyle.Base128);
            CarryOutConnectionHandshake(stream);
            return Serializer.DeserializeWithLengthPrefix<ClientInfo>(stream, PrefixStyle.Base128);
        }
    }

    private static void CarryOutConnectionHandshake(NetworkStream stream)
    {
        int check = stream.ReadByte();
        int ack = connecting ? check : 0;
        stream.WriteByte((byte)ack);
    }

	private static void WaitForReady(MyClientInfo localInfo)
	{
		byte[] buf = new byte[32];
		EndPoint endpoint = new IPEndPoint (
			IPAddress.Parse (remoteInfo.address), remoteInfo.port);
		int sz = localInfo.socket.ReceiveFrom(buf, ref endpoint);
		Debug.Log("Received ready message " + sz + " bytes long");
		try
		{
			PlayerReady msg = Serializer.Deserialize<PlayerReady>(new MemoryStream(buf));
			if (msg.playerID != remoteInfo.playerID)
			{
				throw new Exception("Player ID received does not match recorded remote id");
			}
		}
		catch (Exception e)
		{
			Debug.Log("Error receiving ready message from opponent:");
			Debug.Log(e.Message);
			BroadcastReady(false);
			return;
		}
		lock (readyLock) 
		{
			remoteReady = true;
			Monitor.Pulse(waitingForRemote);
			while (!localReady)
			{
				Monitor.Wait(waitingForLocal);
			}
		}
		BroadcastReady(true, localInfo);
	}

	private static void BroadcastReady(bool ready, MyClientInfo localInfo = null)
	{
		GameReadyEvent broadcast = new GameReadyEvent { success = ready };
		if (ready)
		{
			broadcast.receiverSocket = localInfo.socket;
			broadcast.playerInfo = localInfo.wrapped;
			broadcast.opponentInfo = remoteInfo;
		}
		Dispatcher.Instance.Post(broadcast);
	}

	private static void SendReadyEvent(int localPlayerID)
	{
		Socket socket = new Socket(
			AddressFamily.InterNetwork, 
			SocketType.Dgram,
			ProtocolType.Udp);
		PlayerReady readyEvent = new PlayerReady{ playerID = localPlayerID };
		using (MemoryStream stream = new MemoryStream())
		{
			Serializer.Serialize(stream, readyEvent);
			socket.SendTo(stream.ToArray(), new IPEndPoint(
				IPAddress.Parse(remoteInfo.address), remoteInfo.port));
		}
		lock (readyLock)
		{
			localReady = true;
			Monitor.Pulse(waitingForLocal);
			while (!remoteReady)
			{
				Monitor.Wait(waitingForRemote);
			}
		}
	}

    private class MyClientInfo
    {
        public ClientInfo wrapped;
        public Socket socket;
    }

}
