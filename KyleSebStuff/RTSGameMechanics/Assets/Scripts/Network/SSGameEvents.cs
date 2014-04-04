using System;
using System.Net.Sockets;
using System.Collections.Generic;

using SSProtoBufs;

namespace SSGameEvents 
{
    public class GameConnectionEvent
    {
        public string name;

        public string opponentName;

		public int 	  ID;

		public int	  opponentID;

		public bool	  success;
    }

	public class GameReadyEvent
	{
		public bool		  success;

		public Socket	  receiverSocket;

		public ClientInfo playerInfo;

		public ClientInfo opponentInfo;
	}
}
