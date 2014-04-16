using UnityEngine;
using Pathfinding;
using System.Collections;

public class UnitBitsScript : MonoBehaviour, SSGameManager.IUpdatable {
	public AssemblerScript assemblerScript;
	private float speed = 5;
	public string desiredUnit;
	private Int3 intPosition;
	public Int3 destination;

	void Start() {
		intPosition = (Int3) transform.position;
		SSGameManager.Register(this);
	}
	
	public void GameUpdate (float deltaTime) {
		intPosition += IntPhysics.DisplacementTo(intPosition, destination,
		                                         IntPhysics.FloatSafeMultiply(speed, deltaTime));
		transform.position = (Vector3) intPosition;
		if (IntPhysics.IsCloseEnough(intPosition, destination, 3.0f)) {
			assemblerScript.ReachedAssembler(desiredUnit);
			Destroy(gameObject);
		}
	}



	void OnDestroy() {
		SSGameManager.Unregister (this);
	}
}
