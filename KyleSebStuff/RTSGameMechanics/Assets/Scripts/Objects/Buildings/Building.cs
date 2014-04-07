using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public abstract class Building : WorldObject  {

	private GUIModelManager.GUIModel guiModel;

	protected override void Start() {
		base.Start ();
		guiModel = GetGUIModel();
	}

	public override void GameUpdate(float deltaTime) {
		base.GameUpdate (deltaTime);
		if (currentlySelected) {
			GUIModelManager.CurrentModel = guiModel;
		}
		else {
			GUIModelManager.CurrentModel = null;
		}
	}

	protected abstract GUIModelManager.GUIModel GetGUIModel();
}
