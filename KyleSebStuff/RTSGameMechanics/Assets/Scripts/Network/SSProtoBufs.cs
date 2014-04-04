using UnityEngine;
using System;
using ProtoBuf;
using System.Collections.Generic;

namespace SSProtoBufs
{
    [ProtoContract]
    public class ClientInfo
    {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public string address;

        [ProtoMember(3)]
        public int port;

        [ProtoMember(4)]
        public bool isHost;

		[ProtoMember(5)]
		public int playerID;

		[ProtoMember(6)]
		public int opponentID;
    }

	[ProtoContract]
	public class PlayerReady
	{
		[ProtoMember(1)]
		public int playerID;
	}

	[ProtoContract]
	public class DataPacket
	{
		[ProtoMember(1)]
		public int playerID;

		[ProtoMember(2)]
		public bool isAck;
		
		[ProtoMember(3)]
		public int tick;

		[ProtoMember(4)]
		public List<Command> commands;

		public DataPacket()
		{
			commands = new List<Command>();
		}

		public void AddCommand(Command command)
		{
			commands.Add(command);
		}

		public void AddCommands(IEnumerable<Command> _commands)
		{
			commands.AddRange(_commands);
		}

		public override string ToString()
		{
			string str = "Packet with player id " + playerID + ":\n";
			if (isAck)
			{
				str += "\tIs Ack for tick " + tick;
			}
			else
			{
				str += "\tIs Input with commands:";
				foreach (Command cmd in commands)
				{
					str += "\t\t" + cmd.ToString();
				}
			}
			return str;
		}
	}

	[ProtoContract]
	public class Command
	{
		[ProtoMember(1)]
		public int tick;

		[ProtoMember(2)]
		public int keyCode;

		[ProtoMember(3)]
		public float x0;

		[ProtoMember(4)]
		public float y0;

		[ProtoMember(5)]
		public float z0;

		[ProtoMember(6)]
		public float x1;
		
		[ProtoMember(7)]
		public float y1;
		
		[ProtoMember(8)]
		public float z1;

		public override string ToString()
		{
			return "Tick=" + tick + ", KeyCode=" + keyCode;
		}

		public static Command NewEmptyCommand(int _tick)
		{
			return new Command { tick = _tick, keyCode = SSKeyCode.Empty };
		}
	}
}