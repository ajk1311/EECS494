using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class UserInputManager : MonoBehaviour, SSGameManager.IUpdatable {

	public AudioClip clickSound;

    public int playerID;

    public int PlayerID {
        get {
            return playerID;
        }
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
    }

    private void LeftMouseClickDown(Vector3 mousePosition) {
        PlayerScript playerScript = GameObject.Find("Player").GetComponent<PlayerScript>();
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
            } else if (SSInput.GetKey(playerID, SSKeyCode.A) && SelectionManager.getSelectedUnits(playerID).Count > 0) {
                // Issue attack move
                Vector3 destination = mousePosition;
                if (destination != MechanicResources.InvalidPosition) {
                    if(playerScript.id == playerID) {
                        AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);
                    }
                    SelectionManager.moveUnits(playerID, destination, true);
                }
            } else {
                //check if player is selecting a spawn point for a new combination unit
                if (CombinationManager.creatingCombination[playerID - 1]) {
					PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();

					GameObject combMapCheck = RTSGameMechanics.FindHitObject(mousePosition);

					if(mousePosition.y > 1 && combMapCheck.transform.tag == "Map") {
						//Not a Valid Area of the Map
					} else {
						FogScript fog = FogOfWarManager.getMyFogTile((Int3)mousePosition).GetComponent<FogScript>();
						if ((player.id == playerID && fog.friendlyUnitCount > 0) ||
						    (player.id != playerID && fog.enemyUnitCount > 0)) {
		                    GameObject assembler = GameObject.Find("assembler" + playerID.ToString());
		                    AssemblerScript script = assembler.GetComponent<AssemblerScript>();
		                    CombinationManager.spawnPoint[playerID - 1] = mousePosition;
		                    CombinationManager.combine(script, CombinationManager.desiredUnit[playerID - 1]);
                            ParseManager.LogEvent(ParseManager.ParseEvent.Combination, playerID, CombinationManager.desiredUnit[playerID - 1]);
						}
					}
                    CombinationManager.creatingCombination[playerID - 1] = false;
                }
                //deselect all units
                SelectionManager.deselectAllGameObjects(PlayerID);
            }
        }
    }

    private void RightMouseClick(Vector3 mousePosition) {
        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
		// Check if there is units selected to issue commands to
        if (SelectionManager.count(PlayerID) > 0) {
            // Check if there is an object that we clicked on
            GameObject target = RTSGameMechanics.FindHitObject(mousePosition);
            if (target != null && target.tag != "Map") {
                FogScript fog = target.GetComponent<WorldObject>().currentFogTile.GetComponent<FogScript>();
                // Check if we have vision in the fog tile
                if ((player.id == playerID && fog.friendlyUnitCount > 0) ||
                        (player.id != playerID && fog.enemyUnitCount > 0)) {
                    //Check if the target is an enemy
                    if (player.id != target.GetComponent<WorldObject>().playerID) {
                        // Issue attack command to all selected units
						if(player.id == playerID) {
							AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);
						}
                        SelectionManager.attackUnit(PlayerID, target.GetComponent<WorldObject>());
                    }
                }
            } else {
                // Since we did not click on a target we assume it is a move command
                Vector3 destination = mousePosition;
                if (destination != MechanicResources.InvalidPosition) {
					if(player.id == playerID) {
						AudioSource.PlayClipAtPoint(clickSound, Vector3.zero);
					}
                    SelectionManager.moveUnits(PlayerID, destination);
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
