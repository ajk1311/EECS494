using UnityEngine;
using System.Collections;
using RTS;

public class UserInputManager : MonoBehaviour, SSGameManager.IUpdatable {

	public int playerID;

	public int PlayerID {
		get { return playerID; }
	}

    void Start() {
		SSGameManager.Register(this);
    }   
    
    public void GameUpdate(float deltaTime) {
        MouseActivity();
    }

    private void MouseActivity() {
		Vector3 position, position2;
        if (SSInput.GetMouseClick(PlayerID, 0, out position)) {
            LeftMouseClickDown(position);
        } else if (SSInput.GetMouseClick(PlayerID, 1, out position)) {
            RightMouseClick(position);
		} else if (SSInput.GetMouseDragSelection(playerID, out position, out position2)) {
			LeftMouseDragSelection(position, position2);
		} else {
			SelectionManager.selectedSpace = null;
		}

        //TODO Mouse Hover
    }

    private void LeftMouseClickDown(Vector3 mousePosition) {
        //TODO If mouse in playing area
        if (GUIResources.MouseInPlayingArea()) {
            GameObject hitObject = RTSGameMechanics.FindHitObject(mousePosition);
    
            if (hitObject) {
                if (hitObject.tag != "Map") {
                    WorldObject worldObject = hitObject.GetComponent<WorldObject>();
                    if (worldObject) {
                        if (SelectionManager.isSelected(PlayerID, hitObject)) {
                            //ignore that selected object we own
                        } else {
                            SelectionManager.deselectAllGameObjects(PlayerID);
                            selectGameObject(hitObject);
                        }
                    }
                } else {
                    //deselect all units
					SelectionManager.deselectAllGameObjects(PlayerID);
                }
            }
        } else {
            //TODO Not in Game bounds but in HUD/GUI
        }
    }

    private void RightMouseClick(Vector3 mousePosition) {
        if (GUIResources.MouseInPlayingArea()) {
			if (SelectionManager.count(PlayerID) > 0) {
                GameObject target = RTSGameMechanics.FindHitObject(mousePosition);
                if(target.tag != "Map") {
					SelectionManager.attackUnit(PlayerID, target);
                } else {
                    Vector3 destination = mousePosition;
                    if (destination != MechanicResources.InvalidPosition) {
						SelectionManager.moveUnits(PlayerID, destination);
                    }
                }
            }
        }
    }

	private void LeftMouseDragSelection(Vector3 downPosition, Vector3 upPosition) {
		// TODO selection not under GUI
		SelectionManager.deselectAllGameObjects(playerID);
		SelectionManager.selectedSpace = new Vector3[] { downPosition, upPosition };
	}

    private void selectGameObject(GameObject gameObject) {
        WorldObject worldObject = gameObject.GetComponent<WorldObject>();

		if (worldObject.PlayerID == playerID) {
            worldObject.setCurrentlySelected(true);
        } else {
            //select enemy object
        }
    }
    
}
