using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public abstract class Building : WorldObject  {

	public Object explosionPrefab;

	public override void OnSelectionChanged(bool selected) {
//		GUIModelManager.SetCurrentModel(playerID, selected ? GetGUIModel() : null);
	}

	protected abstract GUIModelManager.GUIModel GetGUIModel();

	protected override void OnDestroy() {
		base.OnDestroy ();
		GameObject obj = (GameObject)Instantiate (explosionPrefab, transform.position, Quaternion.identity);
	}
}
