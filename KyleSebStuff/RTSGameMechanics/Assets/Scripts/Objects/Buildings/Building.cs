using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public abstract class Building : WorldObject  {

	private bool mCalledUnselected = false;

	public override void GameUpdate(float deltaTime) {
		base.GameUpdate(deltaTime);
		if (currentlySelected) {
			mCalledUnselected = false;
			GUIModelManager.SetCurrentModel(playerID, GetGUIModel());
		} else {
			if (!mCalledUnselected) {
				OnUnselected();
				mCalledUnselected = true;
			}
			GUIModelManager.SetCurrentModel(playerID, null);
		}
	}

	protected abstract GUIModelManager.GUIModel GetGUIModel();

	protected virtual void OnUnselected() {
	}
}
