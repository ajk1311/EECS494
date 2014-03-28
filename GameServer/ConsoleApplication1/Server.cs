using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

using ProtoBuf;
using GameProtobuf;

namespace GameServer
{
    /**
     * Class that runs our dedicated matchmaking game server
     */
    class Server
    {
        /** The dedicated port for our game server */
        static readonly int SERVER_PORT = 9191;

        static readonly int SOCKET_TIMEOUT = 5000; // ms

        static void Main(string[] args)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(GetLocalIP(), SERVER_PORT);
                tcpListener.Start();
                while (true)
                {
                    // Until we shut the server down, accept client connections
                    Console.WriteLine("Waiting for client...");
                    Socket clientSocket = tcpListener.AcceptSocket();
                    ThreadPool.QueueUserWorkItem((x) => HandleClient(clientSocket));
                }
            }
            catch (SocketException se)
            {
                Console.Write(se.Message);
                // TODO handle error
            }
        }

        /** Queues up waiting clients */
        static readonly PlayerQueue PLAYER_QUEUE = new PlayerQueue();

        static void HandleClient(Socket clientSocket)
        {
            clientSocket.SendTimeout = clientSocket.ReceiveTimeout = SOCKET_TIMEOUT;
            using (NetworkStream socStream = new NetworkStream(clientSocket))
            {
                // Receive the connecting client's info
                ClientInfo incoming = 
                    Serializer.DeserializeWithLengthPrefix<ClientInfo>(socStream, PrefixStyle.Base128);
                incoming.address = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                Console.WriteLine("Incoming client: name=" + incoming.name + ", address=" +
                     incoming.address + ", port=" + incoming.port);
                
                // Try to connect to a waiting player
                ClientInfoWrapper waiting = PLAYER_QUEUE.Poll();
                
                if (waiting == null)
                {
                    // If there is no waiting player, add the incoming client to the queue
                    Console.WriteLine("No waiting players, adding to the wait queue.");
                    PLAYER_QUEUE.Put(new ClientInfoWrapper { info = incoming, socket = clientSocket });
                }
                else
                {
                    while (!SocketConnected(waiting.socket))
                    {
                        // While the front of the queue has disconnected, or the queue is empty
                        waiting = PLAYER_QUEUE.Poll();
                        if (waiting == null)
                        {
                            Console.WriteLine("No waiting players, adding to the wait queue.");
                            PLAYER_QUEUE.Put(new ClientInfoWrapper { info = incoming, socket = clientSocket });
                            break;
                        }
                    }
                    if (waiting != null && SocketConnected(clientSocket))
                    {
                        // Otherwise we found a match, so hook up the two clients
                        Console.WriteLine("Match found! Connecting " + incoming.name + " and " + waiting.info.name);
                        HookupPlayers(clientSocket, incoming, waiting);
                    }
                }
            }
        }

        static bool SocketConnected(Socket socket)
        {
            try
            {
                byte[] buf = new byte[1];
                buf[0] = (byte)1;
                socket.Send(buf);
                socket.Receive(buf);
                return buf[0] == 1;
            }
            catch (SocketException se)
            {
                Console.Write(se.Message);
                return false;
            }
        }

        /** Tells two players about one another. They engage in necessary game handshakes from then on */
        static void HookupPlayers(Socket incomingSocket, ClientInfo incoming, ClientInfoWrapper waiting)
        {
            // Send the game host his/her necessary information
            using (Socket hostSocket = incomingSocket)
            {
                ClientInfo hostInfo = new ClientInfo
                {
                    name = waiting.info.name,
                    address = waiting.info.address,
                    port = waiting.info.port,
                    isHost = true
                };

                using (NetworkStream hostStream = new NetworkStream(hostSocket))
                {
                    Console.WriteLine("Sending info about " + waiting.info.name + " to " + incoming.name);
                    Serializer.SerializeWithLengthPrefix(hostStream, hostInfo, PrefixStyle.Base128);
                    Console.WriteLine("Done!");
                }
            }

            // Send the game client his/her necessary information
            using (Socket clientSocket = waiting.socket)
            {
                ClientInfo clientInfo = new ClientInfo
                {
                    name = incoming.name,
                    address = incoming.address,
                    port = incoming.port,
                    isHost = false
                };

                using (NetworkStream clientStream = new NetworkStream(clientSocket))
                {
                    Console.WriteLine("Sending info about " + incoming.name + " to " + waiting.info.name);
                    Serializer.SerializeWithLengthPrefix(clientStream, clientInfo, PrefixStyle.Base128);
                    Console.WriteLine("Done!");
                }
            }
            Console.WriteLine("Connection started, game should be underway shortly.");
        }

        /** Returns the local IPv4 address of this machine */
        static IPAddress GetLocalIP()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress address in hostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address;
                }
            }
            return null;
        }
    }

    /** 
     * Small wrapper around a Queue<ClientInfoWrapper> 
     * that keeps the underlying queue synchronized. 
     */
    class PlayerQueue
    {
        private readonly Queue<ClientInfoWrapper> mQueue = new Queue<ClientInfoWrapper>();

        /** Returns the next waiting player or null if there are none */
        public ClientInfoWrapper Poll()
        {
            lock (mQueue)
            {
                return mQueue.Count == 0 ? null : mQueue.Dequeue();
            }
        }

        /** Atomically adds the player's info to the queue */
        public void Put(ClientInfoWrapper info)
        {
            lock (mQueue)
            {
                mQueue.Enqueue(info);
            }
        }
    }

    /** Small wrapper that includes an open socket in the client's info */
    class ClientInfoWrapper
    {
        public ClientInfo info;
        public Socket socket;
    }
}
