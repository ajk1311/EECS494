using System;
using ProtoBuf;

namespace GameProtoBufs
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
    }

	[ProtoContract]
	public class PlayerReady
	{
		[ProtoMember(1)]
		public int playerID;
	}

	[ProtoContract]
	public class InputEvent
	{
		[ProtoMember(1)]
		public int playerID;

		[ProtoMember(2)]
		public int keyCode;
	}
}