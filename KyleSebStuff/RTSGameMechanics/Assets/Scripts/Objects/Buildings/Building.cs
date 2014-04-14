using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public abstract class Building : WorldObject  {

	public override void OnSelectionChanged(bool selected) {
//		GUIModelManager.SetCurrentModel(playerID, selected ? GetGUIModel() : null);
	}

	protected abstract GUIModelManager.GUIModel GetGUIModel();
}
