using UnityEngine;
using System.Collections;
using RTS;

public class UserInputManager : MonoBehaviour {

    SelectionManager selectionManagerScript;
    PlayerScript player;    

    // Use this for initialization
    void Start() {
        selectionManagerScript = this.GetComponent<SelectionManager>();
        player = this.GetComponent<PlayerScript>();
    }   
    
    // Update is called once per frame
    void Update() {
        MouseActivity();
    }

    private void MouseActivity() {
        if (Input.GetMouseButtonDown(0)) {
            LeftMouseClickDown();
            Debug.Log(selectionManagerScript.count());
        } else if (Input.GetMouseButtonDown(1)) 
            RightMouseClick();

        //TODO Mouse Hover
    }

    private void LeftMouseClickDown() {
        //TODO If mouse in playing area
        if (GUIResources.MouseInPlayingArea()) {
            GameObject hitObject = RTSGameMechanics.FindHitObject();
    
            if (hitObject) {
                if (hitObject.tag != "Map") {
                    WorldObject worldObject = hitObject.GetComponent<WorldObject>();
                    if (worldObject) {
                        if (selectionManagerScript.isSelected(hitObject)) {
                            //ignore that selected object we own
                        } else {
                            selectionManagerScript.deselectAllGameObjects();
                            selectGameObject(hitObject);
                        }
                    }
                } else {
                    //deselect all units
                    selectionManagerScript.deselectAllGameObjects();
                }
            }
        } else {
            //TODO Not in Game bounds but in HUD/GUI
        }
    }

    private void RightMouseClick() {
        if (GUIResources.MouseInPlayingArea()) {
            if (selectionManagerScript.count() > 0) {
                Vector3 destination = RTSGameMechanics.FindHitPoint();
                if (destination != MechanicResources.InvalidPosition) {
                    selectionManagerScript.moveUnits(destination);
                }
            }
        }
    }

    private void selectGameObject(GameObject gameObject) {
        WorldObject worldObject = gameObject.GetComponent<WorldObject>();

        if (worldObject.getPlayer() == player) {
            worldObject.setCurrentlySelected(true);
        } else {
            //select enemy object
        }
    }
    
}
