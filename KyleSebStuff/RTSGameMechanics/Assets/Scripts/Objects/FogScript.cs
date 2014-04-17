using UnityEngine;
using System.Collections;
using Pathfinding;

public class FogScript : MonoBehaviour, SSGameManager.IUpdatable {
	public int friendlyUnitCount;
	public int enemyUnitCount;
	
	protected virtual void Start() {
		SSGameManager.Register(this);
		friendlyUnitCount = 0;
		enemyUnitCount = 0;
	}

	public void GameUpdate(float deltaTime) {
		if(friendlyUnitCount > 0) {
			hideFog();
		}
		else {
			showFog();
		}
	}

	private void showFog() {
		foreach (Light light in GetComponentsInChildren<Light>()) {
			light.enabled = false;
		}
	}

	private void hideFog() {
		foreach (Light light in GetComponentsInChildren<Light>()) {
			light.enabled = true;
		}
	}

	protected virtual void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
