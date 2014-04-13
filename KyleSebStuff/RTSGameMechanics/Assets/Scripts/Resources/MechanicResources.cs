using UnityEngine;
using Pathfinding;

namespace RTS{
    public static class MechanicResources{
        public static readonly Vector3 InvalidPosition = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		public static readonly Int3 InvalidIntPosition = new Int3(int.MinValue,      int.MinValue,   int.MinValue);
    }
}
