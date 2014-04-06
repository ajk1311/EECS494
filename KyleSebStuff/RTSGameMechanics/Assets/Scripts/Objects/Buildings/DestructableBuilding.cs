using UnityEngine;
using System.Collections;

public class DestructableBuilding : Building {

	protected virtual void OnDestroy() {
		//change graphic
		//make sure the building unit can no longer be clicked
	}
}
