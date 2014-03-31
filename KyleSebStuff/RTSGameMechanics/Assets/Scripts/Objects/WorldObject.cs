using UnityEngine;
using System.Collections;
using RTS;

public class WorldObject : MonoBehaviour {
	
	//Public variables
	public string objectName;
	public int cost;
	public int hitPoints; 
	public int maxHitPoints;
	
	//Variables accessible by subclass

	//TODO player scripts
	protected PlayerScript player;
	protected Bounds selectionBounds;
	protected bool currentlySelected = false;
	protected bool alreadySelected = false;
	protected SelectionManager selectionManagerScript; 
	
	/*** Game Engine methods, all can be overridden by subclass ***/
	
	protected virtual void Awake() {
		CalculateBounds();
	}
	
	protected virtual void Start () {
		//TODO need to remove this, its for testing only
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerScript> ();

		selectionManagerScript = player.gameObject.GetComponent<SelectionManager> ();
	}
	
	protected virtual void Update () {
		selectionLogic();
	}
	
	protected virtual void OnGUI() {
		if(currentlySelected) 
			DrawSelection();
	}
	
	/*** Public methods ***/

	public PlayerScript getPlayer() {
		return player;
	}

	public void setPlayer(PlayerScript script) {
		this.player = script;
	}

	public void setCurrentlySelected(bool data) {
		currentlySelected = data;
	}

	public bool isSelected() {
		return currentlySelected;
	}

	//TODO Selected Logic, stuff from Sebs Notes

	private void selectionLogic() {

		if(RTSGameMechanics.IsWithin(this.gameObject, selectionManagerScript.selectedSpace)) {
			Debug.Log("-----Select GameObject for Drag------");
			currentlySelected = true;
		}

		if(currentlySelected && !alreadySelected) {
			Debug.Log("-----Select GameObject------");
			selectionManagerScript.addSelectedGameObject(this.gameObject);
			//draw gui
			//set other vars as need to true
		}
		else if(!currentlySelected && alreadySelected) {
			Debug.Log("-----Deselect GameObject------");
			selectionManagerScript.deselectGameObject(this.gameObject);
			//Dont draw gui
			//set other vars to false
		}
		alreadySelected = currentlySelected;
	}
	
//	public virtual void SetHoverState(GameObject hoverObject) {
//		//only handle input if owned by a human player and currently selected
//		if(player && player.human && currentlySelected) {
//			if(hoverObject.name != "Ground") 
//				player.hud.SetCursorState(CursorState.Select);
//		}
//	}
	
	public void CalculateBounds() {
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			selectionBounds.Encapsulate(r.bounds);
		}
	}
	
	private void DrawSelection() {
//		GUI.skin = ResourceManager.SelectBoxSkin;
//		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
//		//Draw the selection box around the currently selected object, within the bounds of the playing area
//		GUI.BeginGroup(playingArea);
//		DrawSelectionBox(selectBox);
//		GUI.EndGroup();
	}
	
	/* Internal worker methods that can be accessed by subclass */
	
	protected virtual void DrawSelectionBox(Rect selectBox) {
//		GUI.Box(selectBox, "");
	}
}
