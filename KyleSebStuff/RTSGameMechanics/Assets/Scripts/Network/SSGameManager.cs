// From Unity
using UnityEngine;

// From System
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

// From us
using RTS;
using ProtoBuf;
using SSProtoBufs;

/**
 * Class that ensures synchronization between the game's players. It implements 
 * a Lockstep game loop that only proceeds when the necessary syncing conditions are met.
 * 
 * This class was implemented 100% ourselves, with inspiration from:
 * http://www.gamasutra.com/view/feature/3094/1500_archers_on_a_288_network_.php
 * http://clintonbrennan.com/2013/12/lockstep-implementation-in-unity3d/
 */
public class SSGameManager : MonoBehaviour {

	/**
	 * All game objects that need to be synchronized between the two
	 * players must implement this interface. It allows an object to
	 * be identified with one of the players and to update itself on
	 * an iteration of the Lockstep loop.
	 */
	public interface IUpdatable {

		/** Called once per object per successful iteration of the Lockstep loop */
		void GameUpdate(float deltaTime);
	}

	/**
	 * Interface that contains the properties that each visible, movable, interactable,
	 * world object should provide. 
	 */
	public interface IWorldObjectProperties {

		/** The unique id for this updatable unit */
		int ID { get; set; }
		
		/** The id of the player the object belongs to */
		int PlayerID { get; set; }
		
		/** The current health of this updatable unity */
		int HitPoints { get; set; }
		
		/** The updatable unit's position in the world */
		Vector3 WorldPosition { get; set; }
	}

	/**
	 * The default number of ticks that make up a communication turn. The scheduling and
	 * execution of commands lag by this many ticks so that the game thread does not have
	 * to block while we wait for opponent commands. 
	 */
	private static readonly int DefaultLatency = 4; // ticks

	/** The default length of one frame in the communication turn */
	private static readonly float DefaultFrameLength = 1f / 60f; // ms	

	/**
	 * When we don't receive acks for our commands, we must retry sending them to the
	 * opponent. We also don't want to retry for forever. After a certain period of time,
	 * the connection to the opponent will be considered dead and the game must end
	 */
	private static readonly int MaxTimeoutLoopCount = 200; // iterations

    /**
     * The amount of time to wait between sending host position correction packets. This number
     * can't be too small, otherwise the game will spend too much time rendering useless unit corrections.
     */
    private static readonly int CorrectionLoopLength = 5 * 1000; // ms

	/** Used to emulate null checking for Vector3s */
	private static readonly Vector3 InvalidPosition = 
		new Vector3(float.MinValue, float.MinValue, float.MinValue);

	/** All of the registered updatable units */
	private static List<IUpdatable> sUnits = new List<IUpdatable>();

	/** Where we are sending packets to */
	private IPEndPoint mRemoteEndpoint;

	/** Where we send packets for the resync protocol */ 
	private IPEndPoint mResyncEndpoint;

	/** The info about ourselves we got from the game server */
	private ClientInfo mPlayerInfo;

	/** The Sockets that send and receive packets */
	private Socket mSender, mReceiver;
	private Socket mResyncSender, mResyncReceiver;

	/** 
	 * Queues up the commands we have yet to be ack'd for per tick 
	 * NOTE: this must by locked when accessing because two threads do so
	 */
	private Dictionary<int, Queue<Command>> mPendingBuffer;

	/** Queues up the commands we receive from Unity input */
	private Dictionary<int, Queue<Command>>	mPlayerCmds;

	/** 
	 * Queues up the commands we receive from opponent player(s) 
	 * NOTE: this must by locked when accessing because two threads do so
	 */
	private Dictionary<int, Queue<Command>> mOpponentCmds;

	private int mLatency;
	private int mCurrentTick;
	// Max tick == the highest received ack
	private int mMaxTick;
	private float mTickLength;

	private int mCurrentGameFrame;
	// The number of frames that we allow to render during a communication turn
	private int mFramesPerTick;
	private float mFrameLength;
	private float mFrameMaxLength;

	// Controls timeouts and retry checks when the game is halted
	private int mTimeoutChecks = 0;

	// Handles mouse input calculation
	private bool mMouse0Down = false;
	private bool mMouse1Down = false;
	private Vector3 mMouse0DownVector = InvalidPosition;

	// Keeps track of the unit ids for each player
	private static int sUnitUid = 0;

	/** Begins the Lockstep loop */
	public static void Start(Socket recvSocket, Socket resyncSocket, ClientInfo playerInfo) {
		// Need Unity to call appropriate MonoBehaviour callbacks for us
		SSGameManager instance = new GameObject("GameManager").AddComponent<SSGameManager>();
		instance.mPlayerInfo = playerInfo;
		instance.mReceiver = recvSocket;
		instance.mSender = new Socket(
			AddressFamily.InterNetwork,
			SocketType.Dgram,
			ProtocolType.Udp);
		instance.mResyncReceiver = resyncSocket;
		instance.mResyncSender = new Socket(
			AddressFamily.InterNetwork,
			SocketType.Dgram,
			ProtocolType.Udp);
		instance.mRemoteEndpoint = new IPEndPoint(
			IPAddress.Parse(playerInfo.address), 
			playerInfo.port);
		instance.mResyncEndpoint = new IPEndPoint(
			IPAddress.Parse(playerInfo.address),
			playerInfo.resyncPort);
		SSInput.Init();
	}

	/** Called at the beginning of a Unity GameObject's lifetime */
	void Start() {
		mLatency = DefaultLatency;
		mCurrentTick = 0;
		mMaxTick = mCurrentTick;

		mCurrentGameFrame = 0;
		mFramesPerTick = mLatency;
		mFrameLength = 0f;
		mFrameMaxLength = DefaultFrameLength;
		Debug.Log("mFrameMaxLength=" + mFrameMaxLength);


		mPlayerCmds = new Dictionary<int,Queue<Command>>();
		mOpponentCmds = new Dictionary<int,Queue<Command>>();
		mPendingBuffer = new Dictionary<int,Queue<Command>>();

		/* Since every command is scheduled for mLatency ticks in the
		 * future, we must enqueue some dummy commands to make up for
		 * the start of the game */
		for (int i = 0; i < mLatency; i++) {
			Queue<Command> player = new Queue<Command>();
			player.Enqueue(Command.NewEmptyCommand(i));
			mPlayerCmds.Add(i, player);

			Queue<Command> pending = new Queue<Command>();
			pending.Enqueue(Command.NewEmptyCommand(i));
			mPendingBuffer.Add(i, pending);
		}

		// Start accepting commands in a background thread
		UnityThreading.Thread.InBackground(() => AcceptCommands());

		// Start listening for or sending resync data
//		if (mPlayerInfo.isHost) {
//			UnityThreading.Thread.InBackground(() => HostCorrectionLoop());
//		} else {
//			UnityThreading.Thread.InBackground(() => ClientCorrectionLoop());
//		}
	}

    /** Allows the game unit to be updated in the game loop */
    public static void Register(IUpdatable gameUnit) {
		if (gameUnit is IWorldObjectProperties) {
			((IWorldObjectProperties) gameUnit).ID = ++sUnitUid;
		}
        sUnits.Add(gameUnit);
    }

    /** Removes a unit from the game loop. Should be done OnDestory */
    public static void Unregister(IUpdatable gameUnit) {
        sUnits.Remove(gameUnit);
    }

	/** We use the real Update() to simulate are own controlled game loop */
	void Update() {
		// We accept input on every real frame so we don't miss any mouse clicks
		AcceptInput();
		mFrameLength += Time.deltaTime;
		if (mFrameLength >= mFrameMaxLength) {
			mFrameLength = 0;
			if (mCurrentGameFrame == 0) {
				/* On the first frame of a communication turn, we must process
				 * all of the incoming, outgoing, and pending commands to make
				 * sure the conditions are met to continue rendering the game */
				if (!ProcessCommands()) {
					// If we can't proceed to the next frame, retry sending the commands
					if (mTimeoutChecks == MaxTimeoutLoopCount) {
						// Connection timed out, TODO game over
					} else if (mTimeoutChecks % 20 == 0) {
						// Only send every second
						SendBufferedCommands();
					}
					mTimeoutChecks++;
					return;
				}
				mTimeoutChecks = 0;
			}
			/* On every frame of the communication turn, we actually render the game.
			 * This is accomplished by calling GameUpdate on every updatable object that
			 * has registered. We also accept and schedule user input */
			foreach (IUpdatable unit in sUnits) {
				unit.GameUpdate(mFrameMaxLength);
			}
			// Input should only last for one frame
			SSInput.ClearInput();
			mCurrentGameFrame = ++mCurrentGameFrame % mFramesPerTick;
		}
	}

	/** 
	 * This method carries out the core of the Lockstep calculation. It ensures
	 * that the game doesn't move forward unless we have received acknowledgement 
	 * and opponent commands for the current communication turn.
	 */
	bool ProcessCommands() {
		if(mCurrentTick <= mMaxTick && ReceivedOpponentCommands()) {
			// If we can proceed, send out all of our commands
			SendBufferedCommands();
			// Then add the input so our game can query for it
			SSInput.AddInput(mPlayerInfo.playerID, mPlayerCmds[mCurrentTick]);
			// Lastly, remove the processed commands
			mPlayerCmds.Remove(mCurrentTick);
			lock (mOpponentCmds) {
				// Repeat for the opponent's commands
				SSInput.AddInput(mPlayerInfo.opponentID, mOpponentCmds[mCurrentTick]);
				mOpponentCmds.Remove(mCurrentTick);
            }
			// We successfully completed a communication turn, so continue to the next one
            mCurrentTick++;
			return true;
		}
		// If we got here, then the conditions for synchronization have not been met
		// Return false so the game halts rendering and waits for the opponent
		return false;
	}

	/** 
	 * Returns whether we have received commands from the opponent
	 * for the current communication turn
	 */
	bool ReceivedOpponentCommands() {
		lock (mOpponentCmds) {
			return mOpponentCmds.ContainsKey(mCurrentTick);
		}
	}

	/** Sends all the commands for the communication turns we haven't received acks for yet */
	void SendBufferedCommands() {
		// The outgoing packet containing all of the un-ack'd commands
		DataPacket packet = new DataPacket() { 
			playerID = mPlayerInfo.playerID 
		};
		lock (mPendingBuffer) {
			/* Make sure we we always send an empty command if we don't have
			 * a valid command scheduled already. The Lockstep loop depends on this.
			 * We do this here instead of in AcceptInput as to reduce the possible empty command
			 * packets we send over the wire. */
			if (!mPendingBuffer.ContainsKey(mCurrentTick + mLatency)) {
				Queue<Command> q = new Queue<Command>();
				q.Enqueue(Command.NewEmptyCommand(mCurrentTick + mLatency));
				mPendingBuffer.Add(mCurrentTick + mLatency, q);
			}
			if (!mPlayerCmds.ContainsKey(mCurrentTick + mLatency)) {
				Queue<Command> q = new Queue<Command>();
				q.Enqueue(Command.NewEmptyCommand(mCurrentTick + mLatency));
				mPlayerCmds.Add(mCurrentTick + mLatency, q);
			}
			foreach(KeyValuePair<int,Queue<Command>> entry in mPendingBuffer) {
				// Add the commands for each un-ack'd communication turn to the outgoing packet
				packet.AddCommands(entry.Value);
			}
		}
		using (MemoryStream stream = new MemoryStream()) {
			// Finally, send the packet
			Serializer.Serialize(stream, packet);
			mSender.SendTo(stream.ToArray(), mRemoteEndpoint);
		}
	}

	/** Detects and process user input from the Unity engine */
	void AcceptInput() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			ScheduleCommand(SSKeyCode.Space);
		} else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			ScheduleCommand(SSKeyCode.LeftArrow);
		} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
			ScheduleCommand(SSKeyCode.RightArrow);
		} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
			ScheduleCommand(SSKeyCode.UpArrow);
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
			ScheduleCommand(SSKeyCode.DownArrow);
		} else {
			// Handling mouse input is trickier, so it gets its own method
			HandleMouseInput();
		}
	}

	/** Detects and schedules mouse clicks and drags */
	// TODO add support for GUI clicks
	void HandleMouseInput() {
		if (Input.GetMouseButtonDown(0)) {
			// Start tracking mouse 0
			mMouse0Down = true;
			mMouse0DownVector = Input.mousePosition;
		} else if (Input.GetMouseButtonUp(0)) {
			if (mMouse0Down) {
				// If we were tracking mouse 0
				if (Mathf.Abs (Input.mousePosition.x - mMouse0DownVector.x) > 2 &&
				    Mathf.Abs (Input.mousePosition.y - mMouse0DownVector.y) > 2) {
					/* If the user dragged the mouse the allowed distance from the start,
					 * then the action is considered a drag */
					float padding = GUIResources.GetScaledPixelSize(4);
					// Adjust the mouse position we will send based on the playing area
					Vector3 mouseInPlayingArea = new Vector3(
						Mathf.Min(Mathf.Max(Input.mousePosition.x, padding), Screen.width - padding),
						Mathf.Min(Mathf.Max(Input.mousePosition.y, 
					                    GUIResources.OrdersBarHeight + padding), Screen.height - padding),
						Input.mousePosition.z);
					// Get the universal world coordinates of the action
					Vector3 downHit = RTSGameMechanics.FindHitPointOnMap(mMouse0DownVector);
					Vector3 upHit = RTSGameMechanics.FindHitPointOnMap(mouseInPlayingArea);
					// Finally, schedule the command
					ScheduleCommand(SSKeyCode.Mouse0Select, 
					                downHit.x, downHit.y, downHit.z,
					                upHit.x, upHit.y, upHit.z);
				} else {
					/* If we got here, the up action was close enough 
					 * to the down action to be considered a click */
					if(GUIResources.MouseInPlayingArea()) {
						Vector3 hit = RTSGameMechanics.FindHitPoint();
						ScheduleCommand(SSKeyCode.Mouse0Click, hit.x, hit.y, hit.z);
					} else {
						int[] button = GUIManager.GetButtonID(Input.mousePosition);
						if(button != null) {
							ScheduleCommand(SSKeyCode.GUIClick, button[0], button[1]);
						}
					}
				}
				// Reset
				mMouse0Down = false;
				mMouse0DownVector = InvalidPosition;
			}
		} 
		// Do the same for mouse 1, but ignore drags
		else if (Input.GetMouseButtonDown(1)) {
			mMouse1Down = true;
		} else if (Input.GetMouseButtonUp(1)) {
			if (mMouse1Down) {
				Vector3 hit = RTSGameMechanics.FindHitPoint();
				ScheduleCommand(SSKeyCode.Mouse1Click, hit.x, hit.y, hit.z);
				mMouse1Down = false;
			}
		}
	}
	
	/** Puts the command into the appropriate queue(s) */
	void ScheduleCommand(int _keyCode,
	                     float x0 = 0, float y0 = 0, float z0 = 0,
	                     float x1 = 0, float y1 = 0, float z1 = 0) {
		Command cmd = new Command() {
			keyCode = _keyCode,
			// Always schedule for mLatency ticks in the future
			tick = mCurrentTick + mLatency
		};

		// Only clicks will need the coordinates to be passed
		if (_keyCode == SSKeyCode.Mouse0Click ||
		    _keyCode == SSKeyCode.Mouse1Click ||
		    _keyCode == SSKeyCode.Mouse0Select ||
		    _keyCode == SSKeyCode.GUIClick) {
			cmd.x0 = x0;
			cmd.y0 = y0;
			cmd.z0 = z0;
			if (_keyCode == SSKeyCode.Mouse0Select) {
				// And only drags will need the second one to be passed
				cmd.x1 = x1;
				cmd.y1 = y1;
				cmd.z1 = z1;
			}
		}

		// Add the command to both the un-ack'd and execution queues
		lock (mPendingBuffer) {
			Queue<Command> pending;
			if (!mPendingBuffer.TryGetValue(cmd.tick, out pending)) {
				pending = new Queue<Command>();
				mPendingBuffer.Add(cmd.tick, pending);
			}
			pending.Enqueue(cmd);
		}
		Queue<Command> player;
		if (!mPlayerCmds.TryGetValue(cmd.tick, out player)) {
			player = new Queue<Command>();
			mPlayerCmds.Add(cmd.tick, player);
		}
		player.Enqueue(cmd);
	}

	/** Listens for and process opponent command and ack packets */
	void AcceptCommands() {
		// Sufficient byte buffer
		byte[] inputBuffer = new byte[128]; 
		// TODO end condition
		while (true) {
			int sz = mReceiver.Receive(inputBuffer);
			DataPacket packet = Serializer.Deserialize<DataPacket>(new MemoryStream(inputBuffer, 0, sz));
			if (packet.isAck) {
				int prevMaxTick = mMaxTick;
				mMaxTick = packet.tick;
				lock (mPendingBuffer) {
					// Remove all of the commands that were just ack'd from the queue
					for (int i = prevMaxTick; i <= mMaxTick; i++) {
						mPendingBuffer.Remove(i);
					}
				}
			} else {
				// We received an input packet from the opponent, so acknowledge
				using (MemoryStream stream = new MemoryStream()) {
					DataPacket ack = new DataPacket {
						playerID = mPlayerInfo.playerID,
						isAck = true,
						tick = MaxPacketTick(packet.commands)
					};
					Serializer.Serialize(stream, ack);
					mSender.SendTo(stream.ToArray(), mRemoteEndpoint); 
				}
				lock (mOpponentCmds) {
					// Queue up each received command to be turned into game input
					foreach (Command cmd in packet.commands) {
						Queue<Command> opponent;
						if (!mOpponentCmds.TryGetValue(cmd.tick, out opponent)) {
							opponent = new Queue<Command>();
							mOpponentCmds.Add(cmd.tick, opponent);
						}
						opponent.Enqueue(cmd);
					}
				}
			}
		}
	}

	/** Given a list of commands, returns the highest tick */
	int MaxPacketTick(List<Command> cmds) {
		int cur, max = 0;
		for (int i = 0, sz = cmds.Count; i < sz; i++) {
			cur = cmds[i].tick;
			max = cur > max ? cur : max;
		}
		return max;
	}

	/** Network resynchronization **/

	/** Loop that runs on the host machine that periodically sends a world checksum */
    void HostCorrectionLoop() {
        // TODO end condition
		byte[] buf = new byte[1];
        while (true) {
			// Only send the resync periodically
            Thread.Sleep(CorrectionLoopLength);
			mResyncSender.SendTo(GenerateWorldHash(), mResyncEndpoint);
			mResyncReceiver.Receive(buf);
			if (buf[0] == 1) {
				// The client is out of sync, we need to send a resync
				SendResync();
			}
        }
    }

	/** Sends the information about the entire game world to the client */
	void SendResync() {
		Resync resync = new Resync();
		foreach (IUpdatable unit in sUnits) {
			if (unit is IWorldObjectProperties) {
				IWorldObjectProperties properties = (IWorldObjectProperties) unit;
				Vector3 position = properties.WorldPosition;
				UnitInfo info = new UnitInfo() {
					id = properties.ID,
					playerID = properties.PlayerID,
					hp = properties.HitPoints,
					x = position.x,
					y = position.y,
					z = position.z
				};
				resync.units.Add(info);
			}
		}
		using (MemoryStream stream = new MemoryStream()) {
			Serializer.SerializeWithLengthPrefix(stream, resync, PrefixStyle.Base128);
			mResyncSender.SendTo(stream.ToArray(), mResyncEndpoint);
		}
	}

	/** Loop that runs on the client machine that receives world checksums and determines desyncs */
    void ClientCorrectionLoop() {
        // TODO end condition
        byte[] buf = new byte[32 * sizeof(char)];
        while (true) {
			// Read the hash of the host's world state
            mResyncReceiver.Receive(buf);
			// Generate our own world's hash
            byte[] hash = GenerateWorldHash();
            if (!HashesEqual(buf, hash)) {
                // The two worlds differ, so we need to let the host know
				mResyncSender.SendTo(new byte[] { 1 }, mResyncEndpoint);
				DoResync();
            } else {
                // Otherwise, the simulation is equal and we can proceed
				mResyncSender.SendTo(new byte[] { 0 }, mResyncEndpoint);
            }
        }
    }

	/** Performs the synchronization of this game world with the host's game world */
	void DoResync() {
		byte[] buf = new byte[4096];
		// Receive the host's world information
		int sz = mResyncReceiver.Receive(buf);
		using (MemoryStream stream = new MemoryStream(buf, 0, sz)) {
			// Deserialize into a nice and friendly protobuf object
			Resync resync = 
				Serializer.DeserializeWithLengthPrefix<Resync>(stream, PrefixStyle.Base128);

			// Maps the unit info we receive for a merge operation
			Dictionary<int, UnitInfo> hostWorld = new Dictionary<int, UnitInfo>();
			// Keeps track of the units that need to be destroyed
			List<IWorldObjectProperties> toDestroy = new List<IWorldObjectProperties>();

			/** Execute the merge **/

			foreach (UnitInfo info in resync.units) {
				hostWorld.Add(info.id, info);
			}

			UnitInfo match;
			foreach (IUpdatable unit in sUnits) {
				if (unit is IWorldObjectProperties) {
					IWorldObjectProperties properties = (IWorldObjectProperties) unit;
					if (hostWorld.TryGetValue(properties.ID, out match)) {
						hostWorld.Remove(match.id);
						// Update the existing unit
						UnityThreading.Thread.InForeground(() => {
							properties.HitPoints = match.hp;
							properties.WorldPosition = new Vector3(match.x, match.y, match.z);
						});
					} else {
						// The unit doesn't exist in the host world, so delete it
						toDestroy.Add(properties);
					}
				}
			}

			if (hostWorld.Count > 0) {
				// If there are any remaining units in the host world, we need to create them
				foreach (KeyValuePair<int, UnitInfo> hostWorldObj in hostWorld) {
					UnityThreading.Thread.InForeground(() => {
						// TODO create unit
					});
				}
			}

			if (toDestroy.Count > 0) {
				// Destroy any units that don't belong in the world
				foreach (IWorldObjectProperties properties in toDestroy) {
					properties.HitPoints = 0;
				}
			}
		}
	}

	/** Creates a checksum value that represents the state of all the units in the world */
    byte[] GenerateWorldHash() {
        string worldState = "";
        foreach (IUpdatable unit in sUnits) {
			if (unit is IWorldObjectProperties) {
				worldState += GenerateUnitHash((IWorldObjectProperties) unit);
			}
        }
        using (MD5 hashSlingingSlasher = MD5.Create()) {
            return hashSlingingSlasher
                .ComputeHash(Encoding.UTF8.GetBytes(worldState));
        }
    }

	static readonly string FloatStringFormat = "F10";

	/** Creates a checksum for an individual unit */
	string GenerateUnitHash(IWorldObjectProperties properties) {
		return properties.HitPoints + 
			properties.WorldPosition.x.ToString(FloatStringFormat) +
			properties.WorldPosition.y.ToString(FloatStringFormat) +
			properties.WorldPosition.z.ToString(FloatStringFormat);
	}

	/**
	 * Returns if the contents of two byte array hashes are equal. It's a
	 * simple for loop, so it's probably not the most efficient, but it works.
	 */
	bool HashesEqual(byte[] lhs, byte[] rhs) {
		if (lhs == rhs) {
			return true;
		}

		if (lhs == null || rhs == null) {
			return false;
		}

		int len = lhs.Length;

		if (len != rhs.Length) {
			return false;
		}

		for (int i = 0; i < len; i++) {
			if (lhs[i] != rhs[i]) {
				return false;
			}
		}

		return true;
	}
}
