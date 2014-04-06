using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public abstract class Building : WorldObject  {

	private GUIModelManager.GUIModel guiModel;

	protected virtual void Start() {
		base.Start ();
		guiModel = GetGUIModel();
	}

	public virtual void GameUpdate(float deltaTime) {
		base.GameUpdate (deltaTime);
		if (currentlySelected) {
			GUIModelManager.CurrentModel = guiModel;
		}
	}

	protected abstract GUIModelManager.GUIModel GetGUIModel();
}
