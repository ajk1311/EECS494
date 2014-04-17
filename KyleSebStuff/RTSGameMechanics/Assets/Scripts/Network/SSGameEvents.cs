using System;
using System.Net.Sockets;
using System.Collections.Generic;

using SSProtoBufs;

/**
 * Contains some events that are critical for game setup
 */
namespace SSGameEvents {
	/**
	 * Event that is signaled when the we have received 
	 * the opponent's information from the server.
	 */
	public class GameConnectionEvent {
		public bool success;
		public int ID;
		public string name;
		public int opponentID;
        public string opponentName;
		public int randomSeed;
        public int gameID;
    }

	/**
	 * Event that is signaled when the player and the opponent
	 * has said he/she is ready for the game to begin
	 */
	public class GameReadyEvent {
		public bool success;
		public Socket receiverSocket;
		public ClientInfo playerInfo;
		public ClientInfo opponentInfo;
	}
}
