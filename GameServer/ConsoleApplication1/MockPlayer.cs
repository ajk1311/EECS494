using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using SSProtoBufs;

namespace GameServer {
	public class MockPlayer {
		private ClientInfo mPlayerInfo;
		private IPEndPoint mRemoteEndpoint;

		private Socket mSendSocket;
		private Socket mRecvSocket;

		private int mTick = 0;
		private int mMaxTick = 0;
		private HashSet<int> mCommands = new HashSet<int>();

        private bool mRunning = false;

		public void Start(Socket recvSocket, ClientInfo playerInfo) {
            mRunning = true;
			mRecvSocket = recvSocket;
            mRecvSocket.ReceiveTimeout = 60 * 1000;
			mPlayerInfo = playerInfo;
			mSendSocket = new Socket(
				AddressFamily.InterNetwork, 
				SocketType.Dgram, 
				ProtocolType.Udp);
            mSendSocket.SendTimeout = 60 * 1000;
			mRemoteEndpoint = new IPEndPoint(
				IPAddress.Parse(mPlayerInfo.address), 
				mPlayerInfo.port);
			ThreadPool.QueueUserWorkItem(x => WaitForReady() );
		}

		private void WaitForReady() {
            try
            {
                byte[] buf = new byte[128];
                int sz = mRecvSocket.Receive(buf);
                Serializer.Deserialize<PlayerReady>(new MemoryStream(buf, 0, sz));
                ThreadPool.QueueUserWorkItem(x => Sender());
                ThreadPool.QueueUserWorkItem(x => Receiver());
                SendGameReady();
            }
            catch (SocketException e)
            {
                return;
            }
		}

		private void SendGameReady() {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    PlayerReady ready = new PlayerReady
                    {
                        playerID = mPlayerInfo.playerID
                    };
                    Serializer.Serialize(stream, ready);
                    mSendSocket.SendTo(stream.ToArray(), mRemoteEndpoint);
                }
            }
            catch (SocketException e)
            {
                return;
            }
		}

		private void Sender() {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    SendEmptyInput(i);
                }
            }
            catch (SocketException e)
            {
                mRunning = false;
                return;
            }
			
            while (mRunning)
            {
				bool commandReady = false;
				lock (mCommands) {
					commandReady = mCommands.Contains(mTick);
				}
				if (mTick <= mMaxTick && commandReady) {
                    try
                    {
                        SendEmptyInput(mTick + 4);
                    }
                    catch (SocketException e)
                    {
                        mRunning = false;
                        return;
                    }
					lock (mCommands) {
						mCommands.Remove(mTick);
					}
					mTick++;
				}
			}
		}

		private void SendEmptyInput(int tick) {
			using (MemoryStream stream = new MemoryStream()) {
				DataPacket packet = new DataPacket {
					playerID = mPlayerInfo.playerID
				};
				packet.AddCommand(Command.NewEmptyCommand(tick));
				Serializer.Serialize(stream, packet);
				mSendSocket.SendTo(stream.ToArray(), mRemoteEndpoint);
			}
		}

		private void Receiver() {
			byte[] buf = new byte[1024];
			while (mRunning) {
                try
                {
                    int sz = mRecvSocket.Receive(buf);
                    DataPacket packet = Serializer.Deserialize<DataPacket>(new MemoryStream(buf, 0, sz));
                    if (packet.isAck)
                    {
                        mMaxTick = packet.tick;
                    }
                    else
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            DataPacket ack = new DataPacket
                            {
                                playerID = mPlayerInfo.playerID,
                                isAck = true,
                                tick = MaxPacketTick(packet.commands)
                            };
                            Serializer.Serialize(stream, ack);
                            mSendSocket.SendTo(stream.ToArray(), mRemoteEndpoint);
                        }
                        lock (mCommands)
                        {
                            foreach (Command cmd in packet.commands)
                            {
                                mCommands.Add(cmd.tick);
                            }
                        }
                    }
                }
                catch (SocketException e)
                {
                    mRunning = false;
                    return;
                }
			}
		}

		private int MaxPacketTick(List<Command> cmds) {
			int current, max = 0;
			for (int i = 0, sz = cmds.Count; i < sz; i++) {
				current = cmds[i].tick;
				max = current > max ? current : max;
			}
			return max;
		}
	}
}

