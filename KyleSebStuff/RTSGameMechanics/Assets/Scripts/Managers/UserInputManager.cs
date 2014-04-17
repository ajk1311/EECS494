using UnityEngine;
using System.Collections;
using RTS;

public class UserInputManager : MonoBehaviour, SSGameManager.IUpdatable {
	
    public int playerID;
    public GUIManager guiManager;

    public int PlayerID {
        get { return playerID; }
    }

    void Start() {
        guiManager = this.GetComponent<GUIManager>();
        SSGameManager.Register(this);
    }
    
    public void GameUpdate(float deltaTime) {
        MouseActivity();
    }

    private void MouseActivity() {
        Vector3 position, position2;

        if (SSInput.GetKeyDown(playerID, SSKeyCode.DownArrow)) {
            Debug.Log("Down arrow");
        }

        if (SSInput.GetMouseClick(PlayerID, 0, out position)) {
            LeftMouseClickDown(position);
        }
        
        if (SSInput.GetMouseClick(PlayerID, 1, out position)) {
            RightMouseClick(position);
        }
        
        if (SSInput.GetMouseDragSelection(playerID, out position, out position2)) {
            LeftMouseDragSelection(position, position2);
        } else {
            SelectionManager.SetSelectedSpace(playerID, null);
        }
        
        if (SSInput.GetGUIClick(playerID, out position)) {
            GUIModelManager.ExecuteClick(playerID, position);
        }
        MouseHover(position);
    }

    private void MouseHover(Vector3 mousePosition) {
        if (GUIResources.MouseInPlayingArea()) {
            GameObject hoverObject = RTSGameMechanics.FindHitObject(mousePosition);
            if (hoverObject) {
                if (hoverObject.GetComponent<WorldObject>().playerID != playerID)
                    guiManager.SetCursorState(CursorState.HoverEnemy);
            }
        }
    }

    private void LeftMouseClickDown(Vector3 mousePosition) {
        //TODO If mouse in playing area
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
				//check if player is selecting a spawn point for a new combination unit
				if(CombinationManager.creatingCombination[playerID-1] == true) {
					GameObject assembler = GameObject.Find("assembler" + playerID.ToString());
					AssemblerScript script = assembler.GetComponent<AssemblerScript>();		
					CombinationManager.spawnPoint[playerID-1] = mousePosition;
					CombinationManager.combine(script, CombinationManager.desiredUnit[playerID-1]);
					CombinationManager.creatingCombination[playerID-1] = false;
				}
				//deselect all units
				SelectionManager.deselectAllGameObjects(PlayerID);
			}
		}
    }

    private void RightMouseClick(Vector3 mousePosition) {
		if (SelectionManager.count(PlayerID) > 0) {
			GameObject target = RTSGameMechanics.FindHitObject(mousePosition);
			// TODO npe
			if(target != null && target.tag != "Map") {
				PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
				FogScript fog = target.GetComponent<WorldObject>().currentFogTile.GetComponent<FogScript>();
				if ((player.id == playerID && fog.friendlyUnitCount > 0) ||
				    (player.id != playerID && fog.enemyUnitCount > 0)) {
					SelectionManager.attackUnit(PlayerID, target.GetComponent<WorldObject>());
					guiManager.SetCursorState(CursorState.Attack);
				}		
			} else {
				Debug.Log (mousePosition);
				Vector3 destination = mousePosition;
				if (destination != MechanicResources.InvalidPosition) {
					SelectionManager.moveUnits(PlayerID, destination);
					guiManager.SetDestination(destination);
					guiManager.SetCursorState(CursorState.Move);
				}
			}
		}
    }

    private void LeftMouseDragSelection(Vector3 downPosition, Vector3 upPosition) {
        SelectionManager.deselectAllGameObjects(playerID);
        SelectionManager.SetSelectedSpace(playerID, new Vector3[] {
            downPosition,
            upPosition
        });
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
