using UnityEngine;
using Pathfinding;
using System.Collections;

public class UnitBitsScript : MonoBehaviour, SSGameManager.IUpdatable {
	public AssemblerScript assemblerScript;
	private float speed = 45;
	public string desiredUnit;
	private Int3 intPosition;

	void Start() {
		intPosition = (Int3) transform.position;
		SSGameManager.Register(this);
	}
	
	public void GameUpdate (float deltaTime) {
		intPosition += IntPhysics.DisplacementTo(intPosition, assemblerScript.intPosition,
		                                         IntPhysics.FloatSafeMultiply(speed, deltaTime));
		transform.position = (Vector3) intPosition;
		if (IntPhysics.IsCloseEnough(intPosition, assemblerScript.intPosition, 0.5f)) {
			assemblerScript.ReachedAssembler(desiredUnit);
			Destroy(gameObject);
		}
	}



	void OnDestroy() {
		SSGameManager.Unregister (this);
	}
}
