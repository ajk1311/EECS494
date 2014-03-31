using System;
using System.Net.Sockets;
using System.Collections.Generic;

using GameProtoBufs;

namespace GameEvents 
{
    public static class GameEventType
    {
        public enum TypeId
        {
            Connection
        }

        public static readonly Dictionary<Type, TypeId> TypeToId = new Dictionary<Type, TypeId>
        {
            {typeof(GameConnectionEvent), TypeId.Connection}
        };
    }

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
