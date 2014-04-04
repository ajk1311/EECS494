﻿using UnityEngine;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using SSProtoBufs;
using ProtoBuf;
using System.IO;

public class SSGameManager : MonoBehaviour {

	public interface IGameUnit
	{
		void SetPlayerID(int playerID);

		void GameUpdate(float deltaTime);
	}

	private static readonly int DEFAULT_LATENCY = 4; // ticks
	private static readonly float DEFAULT_TICK_LENGTH = 400; // ms	
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
		SSGameManager instance = new GameObject("GameManager").AddComponent<SSGameManager>();
		instance.playerInfo = playerInfo;
		instance.recvSocket = recvSocket;
		instance.sendSocket = new Socket(
			AddressFamily.InterNetwork, 
			SocketType.Dgram, 
			ProtocolType.Udp);
		instance.remoteEndpoint = new IPEndPoint(
			IPAddress.Parse(playerInfo.address), 
			playerInfo.port);
		SSInput.Init();
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
            frameTime = 0;
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
					SSInput.ClearInput();
				}
				gameFrame = ++gameFrame % framesPerTick;
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
			SSInput.AddInput(playerInfo.playerID, playerCmds[currTick]);
			playerCmds.Remove(currTick);
			lock (opponentCmds)
			{
				SSInput.AddInput(playerInfo.opponentID, opponentCmds[currTick]);
				opponentCmds.Remove(currTick);
            }
            currTick++;
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
			foreach(KeyValuePair<int,Queue<Command>> entry in pendingBuffer)
			{
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
			ScheduleCommand(SSKeyCode.Space);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			ScheduleCommand(SSKeyCode.LeftArrow);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			ScheduleCommand(SSKeyCode.RightArrow);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			ScheduleCommand(SSKeyCode.UpArrow);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			ScheduleCommand(SSKeyCode.DownArrow);
		}
        else if (Input.GetMouseButtonDown(0))
        {
            ScheduleCommand(SSKeyCode.Mouse0Down);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ScheduleCommand(SSKeyCode.Mouse0Up);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ScheduleCommand(SSKeyCode.Mouse1Up);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            ScheduleCommand(SSKeyCode.Mouse1Down);
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
			//Debug.Log ("incoming packet contents: " + packet.ToString());
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
