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
    }

    [ProtoContract]
    public class RemoteTransform
    {
        [ProtoMember(1)]
        public float x;

        [ProtoMember(2)]
        public float y;

        [ProtoMember(3)]
        public float z;
    }
}