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
	private ClientInfo playerInfo, opponentInfo;
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

	public static void Start(Socket recvSocket, ClientInfo playerInfo, ClientInfo opponentInfo)
	{
		GameManager instance = new GameObject("GameManager").AddComponent<GameManager>();
		instance.playerInfo = playerInfo;
		instance.opponentInfo = opponentInfo;
		instance.recvSocket = recvSocket;
		instance.sendSocket = new Socket(
			AddressFamily.InterNetwork, 
			SocketType.Dgram, 
			ProtocolType.Udp);
		instance.remoteEndpoint = new IPEndPoint(
			IPAddress.Parse(opponentInfo.address), 
			opponentInfo.port);
		GameCommands.Init();
	}

	void Start() 
	{
		playerCmds = new Dictionary<int,Queue<Command>>();
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

		latency = DEFAULT_LATENCY;
		currTick = 0;
		maxTick = currTick + latency;
		tickLength = DEFAULT_TICK_LENGTH;

		gameFrame = 0;
		framesPerTick = latency;
		frameTime = 0f;
		frameLength = framesPerTick / tickLength;

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
			if (gameFrame == 0)
			{
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
						timeoutChecks++;
					}
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
		if(currTick <= maxTick && receivedOpponentCmds)
		{
			SendBufferedCommands();
			GameCommands.AddInput(playerInfo.playerID, playerCmds[currTick]);
			playerCmds.Remove(currTick);
			lock (opponentCmds)
			{
				GameCommands.AddInput(opponentInfo.playerID, opponentCmds[currTick]);
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
			if (!pendingBuffer.ContainsKey(currTick))
			{
				Queue<Command> q = new Queue<Command>();
				q.Enqueue(Command.NewEmptyCommand(currTick));
				pendingBuffer.Add(currTick, q);
			}
			foreach(KeyValuePair<int,Queue<Command>> entry in pendingBuffer)
			{
				packet.AddCommands(entry.Value);
			}
		}
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
		byte[] inputBuffer = new byte[64]; 
		while(true)
		{
			int sz = recvSocket.Receive(inputBuffer);
			DataPacket packet = Serializer.Deserialize<DataPacket>(new MemoryStream(inputBuffer, 0, sz));
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
					tick = packet.tick
				};
				using (MemoryStream stream = new MemoryStream())
				{
					Serializer.Serialize(stream, ack);
					sendSocket.SendTo(stream.ToArray(), remoteEndpoint); 
				}
				lock (opponentCmds)
				{
					if (!opponentCmds.ContainsKey(packet.tick))
					{
						opponentCmds.Add(packet.tick, new Queue<Command>());
					}
					foreach (Command command in packet.commands)
					{
						opponentCmds[command.tick].Enqueue(command);
					}
				}
			}
		}
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
