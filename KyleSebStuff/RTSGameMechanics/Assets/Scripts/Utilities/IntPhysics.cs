using UnityEngine;
using Pathfinding;

public static class IntPhysics {

	public static Int3 MoveTowards(Int3 source, Int3 destination, float maxDistance) {
		Int3 direction = Normalize(destination - source);
		return direction * maxDistance;
	}

	public static Int3 Normalize(Int3 vector) {
		float magn = vector.magnitude;

		if (magn == 0) {
			return vector;
		}

		vector.x = (int) System.Math.Round(vector.x / magn * Int3.FloatPrecision);
		vector.y = (int) System.Math.Round(vector.y / magn * Int3.FloatPrecision);
		vector.z = (int) System.Math.Round(vector.z / magn * Int3.FloatPrecision);

		return vector;
	}

	public static bool IsCloseEnough(Int3 source, Int3 target, float threshold) {
		long longThreshold = (long) System.Math.Round(threshold * Int3.FloatPrecision);
		Int3 distancVector = source - target;
		return distancVector.sqrMagnitudeLong >= (longThreshold * longThreshold);
	}
}
