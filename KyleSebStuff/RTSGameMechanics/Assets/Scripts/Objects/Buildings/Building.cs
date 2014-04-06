using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Building : WorldObject  {

	protected GUIModelManager.GUIModel guiModel;

	protected virtual void Start() {
		base.Start ();
		guiModel = new GUIModelManager.GUIModel ();
	}

	public virtual override void GameUpdate (float deltaTime)
	{
		base.GameUpdate (deltaTime);
		if (currentlySelected) {
			GUIModelManager.CurrentModel = guiModel;
		}
	}
	
	

}
