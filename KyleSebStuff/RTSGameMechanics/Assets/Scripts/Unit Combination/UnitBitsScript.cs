using UnityEngine;
using System.Collections;

public class UnitBitsScript : MonoBehaviour, SSGameManager.IUpdatable {
	public Vector3 destination;
	private float speed = 5;
	public string desiredUnit;

	void Start() {
		SSGameManager.Register(this);
	}
	
	public void GameUpdate (float deltaTime) {
		transform.position = Vector3.Lerp(transform.position, destination, speed * deltaTime);
	}

	void OnDestroy() {
		SSGameManager.Unregister (this);
	}
}
