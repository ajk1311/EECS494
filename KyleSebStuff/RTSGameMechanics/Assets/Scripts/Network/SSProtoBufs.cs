using UnityEngine;
using System;
using ProtoBuf;
using System.Collections.Generic;

/**
 * Contains all of the types of packets we will send between us, the server, and the opponent.
 * Uses Google's awesome Protocol Buffer library, and the even cooler C# library, protobuf-net:
 * https://developers.google.com/protocol-buffers/
 * https://code.google.com/p/protobuf-net/
 */
namespace SSProtoBufs {

	/** Represents the connection information send to and from the server */
    [ProtoContract]
    public class ClientInfo {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(2)]
        public string address;

        [ProtoMember(3)]
        public int port;

		[ProtoMember(4)]
		public int resyncPort;

        [ProtoMember(5)]
        public bool isHost;

		[ProtoMember(6)]
		public int playerID;

		[ProtoMember(7)]
		public int opponentID;
    }

	/** Signal telling players that the game is ready */
	[ProtoContract]
	public class PlayerReady {
		[ProtoMember(1)]
		public int playerID;
	}

	/**
	 * The standard data packet send on every turn of the Lockstep loop in game manager.
	 * To avoid expensive type sending and checking, the packet can act as an input packet or an ack.
	 * We only send input to keep the bandwidth very low, while maximizing the game rendering speed.
	 */
	[ProtoContract]
	public class DataPacket {
		[ProtoMember(1)]
		public int playerID;

		[ProtoMember(2)]
		public bool isAck;
		
		[ProtoMember(3)]
		public int tick;

		[ProtoMember(4)]
		public List<Command> commands;

		public DataPacket() {
			commands = new List<Command>();
		}

		/** Adds a single command to the packet's list */
		public void AddCommand(Command command) {
			commands.Add(command);
		}

		/** Adds the whole collection of commands to the packet */
		public void AddCommands(IEnumerable<Command> _commands) {
			commands.AddRange(_commands);
		}

		/** Prints a human-readable message about this packet */
		public override string ToString() {
			string str = "Packet with player id " + playerID + ":\n";
			if (isAck) {
				str += "\tIs Ack for tick " + tick;
			} else {
				str += "\tIs Input with commands:";
				foreach (Command cmd in commands) {
					str += "\t\t" + cmd.ToString();
				}
			}
			return str;
		}
	}

	/** Represents a command from user input */
	[ProtoContract]
	public class Command {
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

		public override string ToString() {
			return "Tick=" + tick + ", KeyCode=" + keyCode;
		}

		/** Generates an empty command for the given tick */
		public static Command NewEmptyCommand(int _tick) {
			return new Command { tick = _tick, keyCode = SSKeyCode.Empty };
		}
	}

	[ProtoContract]
	public class Resync {
		[ProtoMember(1)]
		public List<UnitInfo> units;

		public Resync() {
			units = new List<UnitInfo>();
		}
	}
	
	[ProtoContract]
	public class UnitInfo {
		[ProtoMember(1)]
		public long id;

		[ProtoMember(2)]
		public int playerID;

		[ProtoMember(3)]
		public int hp;

		[ProtoMember(4)]
		public float x;

		[ProtoMember(5)]
		public float y;

		[ProtoMember(6)]
		public float z;
	}
}