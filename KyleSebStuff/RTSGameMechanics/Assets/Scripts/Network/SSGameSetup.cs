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

public class SSGameSetup {
    private static readonly string SERVER_IP = "10.0.0.17";
    private static readonly int SERVER_PORT = 9191;

    private static bool mConnected = false;
    private static bool mConnecting = false;

	private static bool mLocalReady = false;
	private static bool mRemoteReady = false;
	private static object mReadyLock = new object();

	public static ClientInfo mRemoteInfo;

    public static bool Connected {
        get {
            return mConnected || mConnecting;
        }
    }

    public static void ConnectToGame(string name, bool mock = false) {
        mConnecting = true;
        UnityThreading.Thread.InBackground(() => {
            InitConnection(name, mock);
        });
    }

    public static void CancelGameConnection() {
        mConnecting = false;
    }

    public static void DisconnectFromGame() {
        mConnected = false;
    }

	public static void Ready(int playerID) {
		UnityThreading.Thread.InBackground(() => {
			SendReadyEvent(playerID);
		});
	}

    private static void InitConnection(string name, bool mock) {
        MyClientInfo localInfo = GetLocalClientInfo(name);
		localInfo.wrapped.opponentID = mock ? 1 : 0;
        mRemoteInfo = GetRemoteClientInfo(localInfo.wrapped);
		GameConnectionEvent connectionEvent = new GameConnectionEvent {
			name = localInfo.wrapped.name,
			ID = mRemoteInfo.playerID,
			opponentName = mRemoteInfo.name,
			opponentID = mRemoteInfo.opponentID,
			success = true
		};
		Dispatcher.Instance.Post(connectionEvent);
		WaitForReady(localInfo);
    }

    private static MyClientInfo GetLocalClientInfo(string name) {
        Socket localSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);
        localSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
        ClientInfo localInfo = new ClientInfo {
            name = name,
            port = ((IPEndPoint)localSocket.LocalEndPoint).Port
        };
        return new MyClientInfo {
            wrapped = localInfo,
            socket = localSocket
        };
    }

    private static ClientInfo GetRemoteClientInfo(ClientInfo localInfo) {
        using (TcpClient client = new TcpClient(SERVER_IP, SERVER_PORT))
        using (NetworkStream stream = client.GetStream()) {
            Serializer.SerializeWithLengthPrefix(stream, localInfo, PrefixStyle.Base128);
            CarryOutConnectionHandshake(stream);
            return Serializer.DeserializeWithLengthPrefix<ClientInfo>(stream, PrefixStyle.Base128);
        }
    }

    private static void CarryOutConnectionHandshake(NetworkStream stream) {
        int check = stream.ReadByte();
        int ack = mConnecting ? check : 0;
        stream.WriteByte((byte)ack);
    }

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
		lock (mReadyLock) {
			mRemoteReady = true;
			Monitor.Pulse(mReadyLock);
			while (!mLocalReady) {
				Monitor.Wait(mReadyLock);
			}
		}
		BroadcastReady(true, localInfo);
		UnityThreading.Thread.InForeground(() => {
			SSGameManager.Start(localInfo.socket, mRemoteInfo);
		});
	}

	private static void BroadcastReady(bool ready, MyClientInfo localInfo = null) {
		GameReadyEvent broadcast = new GameReadyEvent { success = ready };
		if (ready) {
			broadcast.receiverSocket = localInfo.socket;
			broadcast.playerInfo = localInfo.wrapped;
			broadcast.opponentInfo = mRemoteInfo;
		}
		Dispatcher.Instance.Post(broadcast);
	}

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
		lock (mReadyLock) {
			mLocalReady = true;
			Monitor.Pulse(mReadyLock);
			while (!mRemoteReady) {
				Monitor.Wait(mReadyLock);
			}
		}
	}

    private class MyClientInfo {
        public ClientInfo wrapped;
        public Socket socket;
    }
}
