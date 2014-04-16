using UnityEngine;

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections;

using EventBus;
using ProtoBuf;
using SSGameEvents;
using SSProtoBufs;

/**
 * Carries out the protocol for setting up the game.
 * 1) Connect to the game server
 * 2) Send your connection information (IP, Port, name, etc.)
 * 3) Receive your opponent's connection information
 * 4) Wait for your opponent's ready signal, while sending your own
 * 5) The game is ready, start the game manager for the Locksetp loop
 */
public class SSGameSetup {
	private static readonly string ServerIP = "10.0.0.19";
    private static readonly int ServerPort = 9191;

    private static bool mConnected = false;
    private static bool mConnecting = false;

	private static bool mLocalReady = false;
	private static bool mRemoteReady = false;
	private static object mReadyLock = new object();

	public static ClientInfo mRemoteInfo;

    public static bool Connected {
        get { return mConnected || mConnecting; }
    }

	/** 
	 * Tells the server you want to play against someone. If mock is true,
	 * the server will spawn a dummy player that always and only sends empty commands.
	 */
    public static void ConnectToGame(string name, bool mock = false) {
        mConnecting = true;
        UnityThreading.Thread.InBackground(() => InitConnection(name, mock) );
    }

	/** Cancels an ongoing connection */
    public static void CancelGameConnection() {
        mConnecting = false;
    }

	/** Severs the connection to the opponent */
    public static void DisconnectFromGame() {
        mConnected = false;
    }

	/** Sends a message to the opponent that you are ready, thus holding up your end of the protocol */
	public static void Ready(int playerID) {
		UnityThreading.Thread.InBackground(() => SendReadyEvent(playerID) );
	}

	/** Carries out the section of the protocol involving the server */
    private static void InitConnection(string name, bool mock) {
        MyClientInfo localInfo = GetLocalClientInfo(name);
		// The server knows how to handle this
		localInfo.wrapped.opponentID = mock ? 1 : 0;
        mRemoteInfo = GetRemoteClientInfo(localInfo.wrapped);
		GameConnectionEvent connectionEvent = new GameConnectionEvent {
			name = localInfo.wrapped.name,
			ID = mRemoteInfo.playerID,
			opponentName = mRemoteInfo.name,
			opponentID = mRemoteInfo.opponentID,
			success = true,
			randomSeed = mRemoteInfo.seed
		};
		Dispatcher.Instance.Post(connectionEvent);
		WaitForReady(localInfo);
    }

	/** Creates and binds a socket to eventually receive opponent commands over */
    private static MyClientInfo GetLocalClientInfo(string name) {
        Socket localSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);
		Socket localResyncSocket = new Socket(
			AddressFamily.InterNetwork,
			SocketType.Dgram,
			ProtocolType.Udp);
        localSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
		localResyncSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        ClientInfo localInfo = new ClientInfo {
            name = name,
            port = ((IPEndPoint) localSocket.LocalEndPoint).Port,
			resyncPort = ((IPEndPoint) localResyncSocket.LocalEndPoint).Port
        };
        return new MyClientInfo {
            wrapped = localInfo,
            socket = localSocket,
			resyncSocket = localResyncSocket
        };
    }

	/** Receives the opponent's info from the server */
    private static ClientInfo GetRemoteClientInfo(ClientInfo localInfo) {
        using (TcpClient client = new TcpClient(ServerIP, ServerPort))
        using (NetworkStream stream = client.GetStream()) {
            Serializer.SerializeWithLengthPrefix(stream, localInfo, PrefixStyle.Base128);
            CarryOutConnectionHandshake(stream);
            return Serializer.DeserializeWithLengthPrefix<ClientInfo>(stream, PrefixStyle.Base128);
        }
    }

	/** Ensures the server that we are still connected */
    private static void CarryOutConnectionHandshake(NetworkStream stream) {
        int check = stream.ReadByte();
        int ack = mConnecting ? check : 0;
        stream.WriteByte((byte)ack);
    }

	/** Waits in the background for the incoming ready signal */
	private static void WaitForReady(MyClientInfo localInfo) {
		byte[] buf = new byte[32];
		int sz = localInfo.socket.Receive(buf);
		try {
			PlayerReady msg = Serializer.Deserialize<PlayerReady>(new MemoryStream(buf, 0, sz));
			if (msg.playerID != mRemoteInfo.opponentID) {
				throw new Exception("Player ID received does not match recorded remote id");
			}
		} catch (Exception e) {
			Debug.Log("Error receiving ready message from opponent:");
			Debug.Log(e.Message);
			BroadcastReady(false);
			return;
		}
		// Ensure we don't prematurely start the game manager
		lock (mReadyLock) {
			mRemoteReady = true;
			Monitor.Pulse(mReadyLock);
			while (!mLocalReady) {
				Monitor.Wait(mReadyLock);
			}
		}
		BroadcastReady(true, localInfo);
		UnityThreading.Thread.InForeground(() =>
			SSGameManager.Start(localInfo.socket, localInfo.resyncSocket, mRemoteInfo) );
	}

	/** Tells anyone listening that the game is ready to start */
	private static void BroadcastReady(bool ready, MyClientInfo localInfo = null) {
		GameReadyEvent broadcast = new GameReadyEvent { success = ready };
		if (ready) {
			// Only need to include connection info on success
			broadcast.receiverSocket = localInfo.socket;
			broadcast.playerInfo = localInfo.wrapped;
			broadcast.opponentInfo = mRemoteInfo;
		}
		Dispatcher.Instance.Post(broadcast);
	}

	/** Sends the opponent your ready event */
	private static void SendReadyEvent(int localPlayerID) {
		Socket socket = new Socket(
			AddressFamily.InterNetwork, 
			SocketType.Dgram,
			ProtocolType.Udp);
		PlayerReady readyEvent = new PlayerReady{ playerID = localPlayerID };
		using (MemoryStream stream = new MemoryStream()) {
			Serializer.Serialize(stream, readyEvent);
			socket.SendTo(stream.ToArray(), new IPEndPoint(
				IPAddress.Parse(mRemoteInfo.address), mRemoteInfo.port));
		}
		// Ensure we don't prematurely start the game manager
		lock (mReadyLock) {
			mLocalReady = true;
			Monitor.Pulse(mReadyLock);
			while (!mRemoteReady) {
				Monitor.Wait(mReadyLock);
			}
		}
	}

	/** Small wrapper that binds a client to its socket */
    private class MyClientInfo {
        public ClientInfo wrapped;
        public Socket socket;
		public Socket resyncSocket;
    }
}
