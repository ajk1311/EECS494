using UnityEngine;

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

public class GameManager 
{
    private static readonly string SERVER_IP = "";

    private static readonly int SERVER_PORT = 9191;

    private static readonly object emptyQueue = new object();

    private static readonly Queue eventQueue = new Queue();

    private static readonly object connectionLock = new object();

    private static bool writerReady = false;

    private static readonly object writerWaiting = new object();

    private static bool readerReady = false;

    private static readonly object readerWaiting = new object();

    private static bool connected = false;

    private static bool connecting = false;

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

    public static void PostGameEvent<TEvent>(TEvent gameEvent)
    {
        lock (eventQueue)
        {
            eventQueue.Enqueue(gameEvent);
            Monitor.Pulse(emptyQueue);
        }
    }

    private static void InitConnection(string name)
    {
        MyClientInfo localInfo = GetLocalClientInfo(name);
        ClientInfo remoteInfo = GetRemoteClientInfo(localInfo.wrapped);

    }

    private static MyClientInfo GetLocalClientInfo(string name)
    {
        Socket localSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);
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

    private static void InitWriterThread(ClientInfo remoteInfo)
    {
        UnityThreading.Thread.InBackground(() =>
        {
            using (TcpClient client = new TcpClient(remoteInfo.address, remoteInfo.port))
            using (NetworkStream stream = client.GetStream())
            {
                lock (connectionLock)
                {
                    writerReady = true;
                    Monitor.Pulse(readerWaiting);
                    while (!readerReady)
                    {
                        Monitor.Wait(writerWaiting);
                    }
                }
                Dispatcher.Instance.Post(new GameConnectionEvent());
                while (connected)
                {
                    object gameEvent = BlockingGetGameEvent();
                    int typeId = (int) GameEventType.TypeToId[gameEvent.GetType()];
                    stream.Write(BitConverter.GetBytes(typeId), 0, 4);
                    Serializer.SerializeWithLengthPrefix(stream, gameEvent, PrefixStyle.Base128);
                }
            }
        });
    }

    private static object BlockingGetGameEvent()
    {
        lock (eventQueue)
        {
            while (eventQueue.Count == 0)
            {
                Monitor.Wait(emptyQueue);
            }
            return eventQueue.Dequeue();
        }
    }

    private static void InitReaderThread(MyClientInfo localInfo)
    {
        UnityThreading.Thread.InBackground(() =>
        {
            using (Socket localSocket = localInfo.socket)
            {
                localSocket.Listen(50);
                using (Socket remoteSocket = localSocket.Accept())
                using (NetworkStream stream = new NetworkStream(localSocket))
                {
                    lock (connectionLock)
                    {
                        readerReady = true;
                        Monitor.Pulse(writerWaiting);
                        while (!writerReady)
                        {
                            Monitor.Wait(readerWaiting);
                        }
                    }

                    while (connected)
                    {
                        byte[] typeBuf = new byte[4];
                        stream.Read(typeBuf, 0, 4);
                        GameEventType.TypeId typeId = 
                            (GameEventType.TypeId) BitConverter.ToInt32(typeBuf, 0);
                        switch (typeId)
                        {
                            case GameEventType.TypeId.Connection:
                                Dispatcher.Instance.Post(Serializer
                                    .DeserializeWithLengthPrefix<GameConnectionEvent>(stream, PrefixStyle.Base128));
                                break;
                        }
                        
                    }
                }
            }
        });
    }

    private class MyClientInfo
    {
        public ClientInfo wrapped;
        public Socket socket;
    }
}
