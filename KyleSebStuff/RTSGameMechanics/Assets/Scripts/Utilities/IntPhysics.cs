using UnityEngine;
using Pathfinding;

public static class IntPhysics {

	public static Int3 DisplacementTo(Int3 source, Int3 destination, float maxDistance) {
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
		return distancVector.sqrMagnitudeLong <= (longThreshold * longThreshold);
	}

	public static float FloatSafeMultiply(float f1, float f2) {
		int i1 = (int) System.Math.Round(f1 * Int3.FloatPrecision);
		int i2 = (int) System.Math.Round(f2 * Int3.FloatPrecision);
		int product = i1 * i2;
		return (float) product / Int3.FloatPrecision / Int3.FloatPrecision;
	}

    public static float FloatSafeDivide(float f1, float f2) {
        int i1 = (int)System.Math.Round(f1 * Int3.FloatPrecision);
        int i2 = (int)System.Math.Round(f2 * Int3.FloatPrecision);
        int product = i1 / i2;
        return (float)product / Int3.FloatPrecision / Int3.FloatPrecision;
    }
}
