using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class WorldObject : MonoBehaviour, SSGameManager.IUpdatable, SSGameManager.IWorldObjectProperties {
	
	public int uid;
	public int playerID;

    public int cost;
    public int hitPoints;
	public int maxHitPoints;
	public string objectName;
    
	protected bool alreadySelected;
	protected bool currentlySelected;
	protected Bounds selectionBounds;
    
	public Int3 intPosition;
	public Int3 lastPosition;

	public int ID {
		get { return uid; }
		set { uid = value; }
	}

	public int PlayerID {
		get { return playerID; }
		set { playerID = value; }
	}

	public int HitPoints {
		get { return hitPoints; }
		set { hitPoints = value; }
	}

	public Vector3 WorldPosition {
		get { return transform.position; }
		set { transform.position = value; }
	}

    /*** Game Engine methods, all can be overridden by subclass ***/

    protected virtual void Awake() {
        CalculateBounds();
    }
    
    protected virtual void Start() {
		currentlySelected = alreadySelected = false;
		intPosition = new Int3(transform.position);
		lastPosition = intPosition;
		SSGameManager.Register(this);
		GridManager.UpdatePosition(intPosition, this);
    }

	protected virtual void OnDestroy() {
		if(currentlySelected) {
			SelectionManager.removeUnitFromList(playerID, this.gameObject);
		}
		GridManager.RemoveFromGrid(this);
		SSGameManager.Unregister(this);
	}

	public virtual void GameUpdate(float deltaTime) {
		if(intPosition != lastPosition) {
			lastPosition = intPosition;
			GridManager.UpdatePosition(intPosition, this);
		}
		selectionLogic();
	}
    
    protected virtual void OnGUI() {
        if (currentlySelected && 
		    playerID == GameObject.Find("Player").GetComponent<PlayerScript>().id) {
            DrawSelection();
		}
    }
    
    public void setCurrentlySelected(bool currentlySelected) {
		currentlySelected = currentlySelected;
    }

    public bool isSelected() {
        return currentlySelected;
    }
	
    private void selectionLogic() {
        if (currentlySelected && !alreadySelected) {
			SelectionManager.addSelectedGameObject(PlayerID, this.gameObject);
            //draw gui
            //set other vars as need to true
        } else if (!currentlySelected && alreadySelected) {
			SelectionManager.deselectGameObject(PlayerID,this.gameObject);
            //Dont draw gui
            //set other vars to false
        }
        alreadySelected = currentlySelected;
    }
    
//  public virtual void SetHoverState(GameObject hoverObject) {
//      //only handle input if owned by a human player and currently selected
//      if(player && player.human && currentlySelected) {
//          if(hoverObject.name != "Ground") 
//              player.hud.SetCursorState(CursorState.Select);
//      }
//  }
    
    public void CalculateBounds() {
        selectionBounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
            selectionBounds.Encapsulate(r.bounds);
        }
    }
    
    private void DrawSelection() {
        GUI.skin = GUIResources.SelectBoxSkin;
        Rect selectBox = GUIResources.CalculateSelectionBox(selectionBounds);
        //Draw the selection box around the currently selected object, within the bounds of the playing area
        GUI.BeginGroup(GUIResources.PlayingArea);
        DrawSelectionBox(selectBox);
        GUI.EndGroup();
    }
    
    /* Internal worker methods that can be accessed by subclass */
    
    protected virtual void DrawSelectionBox(Rect selectBox) {
        GUI.Box(selectBox, "");
    }

    public virtual void TakeDamage(int damage) {
        hitPoints -= damage;
        if (hitPoints <= 0) {
            Destroy(gameObject);
        }
    }
}
