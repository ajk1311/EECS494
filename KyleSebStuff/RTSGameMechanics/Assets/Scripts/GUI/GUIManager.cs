using UnityEngine;
using System.Collections;
using RTS;

public class GUIManager : MonoBehaviour {

	public GUISkin SELECT_BOX_SKIN;

	// Use this for initialization
	void Start () {
        GUIResources.setSelectBoxSkin(SELECT_BOX_SKIN);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
