using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;
using MyMinimap;

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

	public GameObject currentFogTile;

	public Renderer objectRenderer;

	public MapMarker marker;
	public Texture magentaTexture;
	public Texture orangeTexture;

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
		marker = new MapMarker (this.gameObject, playerID == 1? orangeTexture : magentaTexture);
		GameObject.Find ("MapCenter").GetComponent<MapManager> ().register (marker);
		GridManager.UpdatePosition(intPosition, this);
		currentFogTile = FogOfWarManager.getMyFogTile (intPosition);
		FogOfWarManager.updateFogTileUnitCount (null, currentFogTile, playerID);
		objectRenderer = GetComponentInChildren<Renderer>();
    }

	protected virtual void OnDestroy() {
		if(currentlySelected) {
			SelectionManager.removeUnitFromList(playerID, this.gameObject);
		}
		GridManager.RemoveFromGrid(this);
		SSGameManager.Unregister(this);
		GameObject.Find ("MapCenter").GetComponent<MapManager> ().unregister (marker);
		FogOfWarManager.updateFogTileUnitCount (currentFogTile, null, playerID);
	}

	public virtual void GameUpdate(float deltaTime) {
		if (intPosition != lastPosition) {
			lastPosition = intPosition;
			GridManager.UpdatePosition(intPosition, this);
		}
		selectionLogic();
		fogOfWarLogic();
	}
    
    protected virtual void OnGUI() {
        if (currentlySelected && 
		    playerID == GameObject.Find("Player").GetComponent<PlayerScript>().id) {
            DrawSelection();
		}
    }
    
    public void setCurrentlySelected(bool currentlySelected) {
		this.currentlySelected = currentlySelected;
    }

    public bool isSelected() {
        return currentlySelected;
    }
	
    private void selectionLogic() {
        if (currentlySelected && !alreadySelected) {
			SelectionManager.addSelectedGameObject(PlayerID, this.gameObject);
			OnSelectionChanged(true);
            //draw gui
            //set other vars as need to true
        } else if (!currentlySelected && alreadySelected) {
			SelectionManager.deselectGameObject(PlayerID,this.gameObject);
			OnSelectionChanged(false);
            //Dont draw gui
            //set other vars to false
        }
        alreadySelected = currentlySelected;
    }

	public virtual void OnSelectionChanged(bool selected) {
	}
    
	private void fogOfWarLogic() {
		GameObject fogTileCheck = FogOfWarManager.getMyFogTile (intPosition);
		if(fogTileCheck != currentFogTile) {
			FogOfWarManager.updateFogTileUnitCount(currentFogTile, fogTileCheck, playerID);
			currentFogTile = fogTileCheck;
		}

		if(FogOfWarManager.isVisible(currentFogTile, playerID)) {
			objectRenderer.enabled = true;
		}
		else {
			objectRenderer.enabled = false;
		}
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
