using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using GameProtoBufs;
using ProtoBuf;
using System.IO;

public class GameManager : MonoBehaviour {

	public interface IGameUnit
	{
		void SetPlayerID(int playerID);

		void GameUpdate(float deltaTime);
	}

	private static readonly int DEFAULT_LATENCY = 4; // ticks
	private static readonly float DEFAULT_TICK_LENGTH = 200; // ms	
	private static readonly int MAX_TIMEOUT_LOOP_COUNT = 200; // iterations

	private static List<IGameUnit> units = new List<IGameUnit>();

	private IPEndPoint remoteEndpoint;
	private ClientInfo playerInfo;
	private Socket 	sendSocket, recvSocket;

	private Dictionary<int, Queue<Command>> pendingBuffer;
	private Dictionary<int, Queue<Command>>	playerCmds;
	private Dictionary<int, Queue<Command>> opponentCmds;

	// Controls the communication turn, or, ticks
	private int	latency;
	private int currTick;
	private int maxTick;
	private float tickLength;

	// Controls the game loop, i.e. processing commands and rendering
	private int gameFrame;
	private int framesPerTick;
	private float frameTime;
	private float frameLength;

	// Controls timeouts and retry checks when the game is halted
	private int timeoutChecks = 0;

	public static void Start(Socket recvSocket, ClientInfo playerInfo)
	{
		GameManager instance = new GameObject("GameManager").AddComponent<GameManager>();
		instance.playerInfo = playerInfo;
		instance.recvSocket = recvSocket;
		instance.sendSocket = new Socket(
			AddressFamily.InterNetwork, 
			SocketType.Dgram, 
			ProtocolType.Udp);
		instance.remoteEndpoint = new IPEndPoint(
			IPAddress.Parse(playerInfo.address), 
			playerInfo.port);
		GameCommands.Init();
	}

	void Start() 
	{
		latency = DEFAULT_LATENCY;
		currTick = 0;
		maxTick = currTick;
		tickLength = DEFAULT_TICK_LENGTH;
		
		gameFrame = 0;
		framesPerTick = latency;
		frameTime = 0f;
		frameLength = framesPerTick / tickLength;

		playerCmds = new Dictionary<int,Queue<Command>>();
		opponentCmds = new Dictionary<int,Queue<Command>>();
		pendingBuffer = new Dictionary<int,Queue<Command>>();
		
		for (int i = 0; i < latency; i++)
		{
			Queue<Command> player = new Queue<Command>();
			player.Enqueue(Command.NewEmptyCommand(i));
			playerCmds.Add(i, player);

			Queue<Command> pending = new Queue<Command>();
			pending.Enqueue(Command.NewEmptyCommand(i));
			pendingBuffer.Add(i, pending);
		}

		UnityThreading.Thread.InBackground (() =>
		{
			AcceptCommands();
		});
	}

	void Update() 
	{
		frameTime += Time.deltaTime;
		if (frameTime >= frameLength)
		{
//			Debug.Log ("curr game frame: " + gameFrame);
			if (gameFrame == 0)
			{
//				Debug.Log ("Curr Tick: " + currTick);
				if (ProcessCommands())
				{
					AcceptInput();
					gameFrame++;
					timeoutChecks = 0;
				}
				else 
				{
					if (timeoutChecks == MAX_TIMEOUT_LOOP_COUNT)
					{
						// Connection timed out, game over
					}
					else if (timeoutChecks % 20 == 0)
					{
						SendBufferedCommands();
					}
					timeoutChecks++;
				}
			}
			else
			{
				AcceptInput();
				foreach (IGameUnit unit in units)
				{
					unit.GameUpdate(frameLength);
				}
				if (gameFrame == 1)
				{
					GameCommands.ClearInput();
				}
				gameFrame++;
				if (gameFrame == framesPerTick)
				{
					gameFrame = 0;
					currTick++;
				}
			}
		}
	}

	bool ProcessCommands()
	{
		bool receivedOpponentCmds = false;
		lock (opponentCmds)
		{
			receivedOpponentCmds = opponentCmds.ContainsKey(currTick);
		}
//		Debug.Log ("max tick is: " + maxTick);
//		foreach(KeyValuePair<int,Queue<Command>> pair in opponentCmds)
//		{
//			Debug.Log ("opponentCmds contains key: " + pair.Key);
//		}
//		foreach(KeyValuePair<int,Queue<Command>> pair in playerCmds)
//		{
//			Debug.Log ("playerCmds contains key: " + pair.Key);
//		}
		if(currTick <= maxTick && receivedOpponentCmds)
		{
			SendBufferedCommands();
			GameCommands.AddInput(playerInfo.playerID, playerCmds[currTick]);
			playerCmds.Remove(currTick);
			lock (opponentCmds)
			{
				GameCommands.AddInput(playerInfo.opponentID, opponentCmds[currTick]);
				opponentCmds.Remove(currTick);
			}
			return true;
		}
		return false;
	}

	void SendBufferedCommands()
	{
		DataPacket packet = new DataPacket();
		packet.playerID = playerInfo.playerID;
		lock (pendingBuffer) 
		{
			if (!pendingBuffer.ContainsKey(currTick + latency))
			{
				Queue<Command> q = new Queue<Command>();
				q.Enqueue(Command.NewEmptyCommand(currTick + latency));
				pendingBuffer.Add(currTick + latency, q);
			}
			if (!playerCmds.ContainsKey(currTick + latency))
			{
				Queue<Command> q = new Queue<Command>();
				q.Enqueue(Command.NewEmptyCommand(currTick + latency));
				playerCmds.Add(currTick + latency, q);
			}
//			Debug.Log("SENDING PACKET WITH THE FOLLOWING CONTENTS: ");
			foreach(KeyValuePair<int,Queue<Command>> entry in pendingBuffer)
			{
//				Debug.Log ("putting into pending buffer with tick: " + entry.Key);
//				Debug.Log ("putting into pending buffer with queue of length: " + entry.Value.Count);
				packet.AddCommands(entry.Value);
			}
		}
		Debug.Log ("sending out packet: " + packet.ToString ());
		using (MemoryStream stream = new MemoryStream())
		{
			Serializer.Serialize(stream, packet);
			sendSocket.SendTo(stream.ToArray(), remoteEndpoint);
		}
	}

	void AcceptInput()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			ScheduleCommand(0);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ScheduleCommand(1);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ScheduleCommand(2);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			ScheduleCommand(3);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			ScheduleCommand(4);
		}
	}

	void ScheduleCommand(int keyCode)
	{
		Command cmd = new Command();
		cmd.keyCode = keyCode;
		cmd.tick = currTick + latency;
		lock (pendingBuffer)
		{
			if (!pendingBuffer.ContainsKey(cmd.tick))
			{
				pendingBuffer.Add(cmd.tick, new Queue<Command>());
			}
			pendingBuffer[cmd.tick].Enqueue(cmd);
		}
		if (!playerCmds.ContainsKey(cmd.tick))
		{
			playerCmds.Add(cmd.tick, new Queue<Command>());
		}
		playerCmds[cmd.tick].Enqueue(cmd);
	}

	void AcceptCommands()
	{
		byte[] inputBuffer = new byte[128]; 
		while(true)
		{
			int sz = recvSocket.Receive(inputBuffer);
			DataPacket packet = Serializer.Deserialize<DataPacket>(new MemoryStream(inputBuffer, 0, sz));
//			Debug.Log ("incoming packet contents: " + packet.ToString());
			if (packet.isAck) 
			{
				int prevMaxTick = maxTick;
				maxTick = packet.tick;
				lock (pendingBuffer)
				{
					for (int i = prevMaxTick; i <= maxTick; i++)
					{
						pendingBuffer.Remove(i);
					}
				}
			}
			else
			{
				DataPacket ack = new DataPacket
				{
					playerID = playerInfo.playerID,
					isAck = true,
					tick = maxPacketTick(packet.commands)
				};
				using (MemoryStream stream = new MemoryStream())
				{
					Serializer.Serialize(stream, ack);
					sendSocket.SendTo(stream.ToArray(), remoteEndpoint); 
				}
				lock (opponentCmds)
				{
					foreach (Command command in packet.commands)
					{
						if (!opponentCmds.ContainsKey(command.tick))
						{
							opponentCmds.Add(command.tick, new Queue<Command>());
						}
						opponentCmds[command.tick].Enqueue(command);
					}
				}
			}
		}
	}

	private int maxPacketTick(List<Command> cmds)
	{
		int curr_max = 0;
		for(int i = 0; i < cmds.Count; i++)
		{
			if(curr_max < cmds[i].tick)
			{
				curr_max = cmds[i].tick;
			}
		}
		return curr_max;
	}

	public static void RegisterGameUnit(IGameUnit gameUnit)
	{
		units.Add(gameUnit);
	}

	public static void UnregisterGameUnit(IGameUnit gameUnit)
	{
		units.Remove(gameUnit);
	}
}
