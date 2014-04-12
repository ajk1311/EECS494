using UnityEngine;
using System.Collections;
using Pathfinding;

public class FogScript : MonoBehaviour, SSGameManager.IUpdatable {
	Int3 position;
	int playerID;

	protected virtual void Start() {
		SSGameManager.Register(this);
	}

	public void GameUpdate(float deltaTime) {
		if(FogOfWarManager.getUnitCountForFogTile(position, playerID) > 0) {
			hideFog();
		}
		else {
			showFog();
		}
	}

	private void showFog() {
		//render to true
	}

	private void hideFog() {
		//render false
	}

	protected virtual void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
