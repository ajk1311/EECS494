using UnityEngine;
using System.Collections;

public class DestructableBuilding : Building {

	public override void GameUpdate (float deltaTime)
	{
		base.GameUpdate (deltaTime);
		if(hitPoints <= 0) {
			Destroy();
		}
	}
	protected virtual void Destroy() {
		//change graphic
		//make sure the building unit can no longer be clicked
	}
}
