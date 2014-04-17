using UnityEngine;
using Pathfinding;
using System.Collections;

public class UnitBitsScript : MonoBehaviour, SSGameManager.IUpdatable {
	public AssemblerScript assemblerScript;
	private float speed = 45;
	public string desiredUnit;
	private Int3 intPosition;
	public Int3 destination;
	public int combinationID;

	void Start() {
		intPosition = (Int3) transform.position;
		SSGameManager.Register(this);
	}
	
	public void GameUpdate (float deltaTime) {
		intPosition += IntPhysics.DisplacementTo(intPosition, destination,
		                                         IntPhysics.FloatSafeMultiply(speed, deltaTime));
		transform.position = (Vector3) intPosition;
		if (IntPhysics.IsCloseEnough(intPosition, destination, 3.0f)) {
			assemblerScript.ReachedAssembler(combinationID, (Vector3)destination, desiredUnit);
			Destroy(gameObject);
		}
	}



	void OnDestroy() {
		SSGameManager.Unregister (this);
	}
}
