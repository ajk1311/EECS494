using System;
using System.Collections.Generic;

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
    }
}
