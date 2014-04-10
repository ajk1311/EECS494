using UnityEngine;
using System.Collections;
using RTS;

public class WorldObject : MonoBehaviour, SSGameManager.IUpdatable, SSGameManager.IWorldObjectProperties {
    
    //Public variables
    public string objectName;
    public int cost;
    public int hitPoints;
    public int maxHitPoints;
    
    //Variables accessible by subclass

    //TODO player scripts
    public int playerID;
    protected Bounds selectionBounds;
    protected bool currentlySelected = false;
    protected bool alreadySelected = false;
    
	private long uid;

	public long ID {
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
		SSGameManager.Register(this);
    }

	protected virtual void OnDestroy() {
		if(currentlySelected) {
			SelectionManager.removeUnitFromList(playerID, this.gameObject);
		}

		SSGameManager.Unregister(this);
	}

	public virtual void GameUpdate(float deltaTime) {
		selectionLogic();
	}
    
    protected virtual void OnGUI() {
        if (currentlySelected && 
		    playerID == GameObject.Find("Player").GetComponent<PlayerScript>().id) {
            DrawSelection();
		}
    }
    
    /*** Public methods ***/

    public void setCurrentlySelected(bool data) {
        currentlySelected = data;
    }

    public bool isSelected() {
        return currentlySelected;
    }

    //TODO Selected Logic, stuff from Sebs Notes

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

    public virtual void TakeDamage(int damage){
        hitPoints -= damage;
        if(hitPoints <= 0){
            Destroy(gameObject);
        }
    }
}
