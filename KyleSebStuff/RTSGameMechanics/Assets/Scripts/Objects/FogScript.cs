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

		Color c = gameObject.renderer.material.color;
		c.a = 0.65f;
		gameObject.renderer.material.shader = Shader.Find("Transparent/Diffuse");
		gameObject.renderer.material.color = c;
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
		renderer.enabled = true;
	}

	private void hideFog() {
		renderer.enabled = false;
	}

	protected virtual void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
