using UnityEngine;
using System.Collections;

namespace RTS{
    public static class MechanicResources{
        private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
        public static Vector3 InvalidPosition { get { return invalidPosition; } }
    }
}
