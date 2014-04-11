using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public abstract class Building : WorldObject  {

	private GUIModelManager.GUIModel guiModel;

	public override void GameUpdate(float deltaTime) {
		base.GameUpdate(deltaTime);
		if (currentlySelected) {
			GUIModelManager.SetCurrentModel(playerID, GetGUIModel());
		} else {
			GUIModelManager.SetCurrentModel(playerID, null);
		}
	}

	protected abstract GUIModelManager.GUIModel GetGUIModel();
}
