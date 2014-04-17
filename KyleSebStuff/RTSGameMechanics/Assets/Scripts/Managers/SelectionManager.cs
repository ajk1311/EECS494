using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;
using RTS;

public static class SelectionManager {

    private static List<GUIModelManager.GUIModel> selectionModels;
    private static List<Vector3[]> selectedSpaces;
    private static List<List<GameObject>> currentlySelectedObjects;

    public static void Init() {
        selectionModels = new List<GUIModelManager.GUIModel>();
        selectionModels.Add(null);
        selectionModels.Add(null);
        selectedSpaces = new List<Vector3[]>();
        selectedSpaces.Add(null);
        selectedSpaces.Add(null);

        currentlySelectedObjects = new List<List<GameObject>>(2);
        currentlySelectedObjects.Add(new List<GameObject>());
        currentlySelectedObjects.Add(new List<GameObject>());
    }

    public static Vector3[] GetSelectedSpace(int playerID) {
        return selectedSpaces [playerID - 1];
    }

    public static void SetSelectedSpace(int playerID, Vector3[] space) {
        selectedSpaces [playerID - 1] = space;
    }

    public static int count(int playerID) {
        return currentlySelectedObjects [playerID - 1].Count;
    }

    public static void addSelectedGameObject(int playerID, GameObject gameObject) {
        if (currentlySelectedObjects [playerID - 1].Count == 0) {
            CreateGUIModel(playerID, gameObject.GetComponent<WorldObject>());
        } else {
            UpdateGUIModel(playerID, gameObject.GetComponent<WorldObject>());
        }
        currentlySelectedObjects [playerID - 1].Add(gameObject);
    }

    private static void CreateGUIModel(int playerID, WorldObject wo) {
        selectionModels [playerID - 1] = new GUIModelManager.GUIModel();
        selectionModels [playerID - 1].leftPanelColumns = 1;
        selectionModels [playerID - 1].centerPanelColumns = 4;
        UpdateGUIModel(playerID, wo);
        GUIModelManager.SetCurrentModel(playerID, selectionModels [playerID - 1]);
    }

	public static void addSelectedGameObject(int playerID, GameObject gameObject) {
		if(currentlySelectedObjects[playerID-1].Count == 0) {
			currentlySelectedObjects[playerID -1].Add(gameObject);
			CreateGUIModel(playerID, gameObject.GetComponent<WorldObject>());
		}
		else {
			currentlySelectedObjects[playerID -1].Add(gameObject);
			UpdateGUIModel(playerID, gameObject.GetComponent<WorldObject>());
		}
    }

	private static void CreateGUIModel(int playerID, WorldObject wo) {
		selectionModels[playerID-1] = new GUIModelManager.GUIModel();
		selectionModels[playerID-1].leftPanelColumns = 1;
		selectionModels[playerID-1].centerPanelColumns = 4;
		UpdateGUIModel (playerID, wo);
		GUIModelManager.SetCurrentModel (playerID, selectionModels [playerID - 1]);
	}

	private static void UpdateGUIModel(int playerID, WorldObject wo) {
		//check for new combos and add to left panel
		List<KeyValuePair<string,int>> availCombos = CombinationManager.getAvailableCombinations (playerID);
		Debug.Log ("available combos:" + availCombos.Count);
		selectionModels [playerID - 1].ClearButtons (0);
		foreach(KeyValuePair<string,int> pair in availCombos) {
			GUIModelManager.Button comboButton = new GUIModelManager.Button();
			comboButton.text = pair.Key + "x" + pair.Value;
			comboButton.clicked += () => 
			{
				CombinationManager.creatingCombination[playerID-1] = true;
				CombinationManager.desiredUnit[playerID-1] = pair.Key;
			};
			selectionModels[playerID-1].AddButton(0, comboButton);
		}
		GUIModelManager.Button button = new GUIModelManager.Button();
		button.text = wo.objectName; //get the unit name
		button.clicked += () => 
		{
			if(wo != null) {
				deselectAllGameObjects(playerID);
				wo.setCurrentlySelected(true);
			}
		};
		selectionModels[playerID-1].AddButton(1, button);
	}
	public static void deselectGameObject(int playerID, GameObject obj) {
		GUIModelManager.SetCurrentModel (playerID, null);
		if (!currentlySelectedObjects[playerID -1].Remove(obj)) {
			Debug.Log("Removed a non-selected object");
		}
    }

	public static void deselectAllGameObjects(int playerID) {
		GUIModelManager.SetCurrentModel (playerID, null);
        foreach (GameObject obj in currentlySelectedObjects[playerID-1]) {
            obj.GetComponent<WorldObject>().setCurrentlySelected(false);
        }
    }

	public static void moveUnits(int playerID, Vector3 destination, bool attackMove = false) {
		List<GameObject> selectedUnits = currentlySelectedObjects [playerID - 1];
		List<Int3> destinationCluster = GridManager.GetDestinationCluster ((Int3) destination, selectedUnits.Count);
		for(int i = 0; i < selectedUnits.Count; i++) {
			if(selectedUnits[i] != null) {
				selectedUnits[i].GetComponent<Unit>().IssueMoveCommand((Vector3)destinationCluster[i], attackMove);
			}
		}
    }

    public static void attackUnit(int playerID, WorldObject target) {
        foreach (GameObject obj in currentlySelectedObjects[playerID -1]) {
            if (obj != null) {
                obj.GetComponent<Unit>().IssueAttackCommand(target);
            }
        }
    }

    public static bool isSelected(int playerID, GameObject gameObject) {
        return currentlySelectedObjects [playerID - 1].Find(foundObj => foundObj == gameObject);
    }

    public static List<GameObject> getSelectedUnits(int playerID) {
        return currentlySelectedObjects [playerID - 1];
    }

    public static void removeUnitFromList(int playerID, GameObject gameObject) {
        currentlySelectedObjects [playerID - 1].Remove(gameObject);
    }

    public static Dictionary<string, int> getUnitCounts(int playerID) {
        Dictionary<string, int> unitCounts = new Dictionary<string, int>();
        
        foreach (GameObject unit in currentlySelectedObjects[playerID-1]) {
            string currentKey = unit.GetComponent<WorldObject>().objectName;
            if (unitCounts.ContainsKey(currentKey)) {
                unitCounts [currentKey]++;
            } else {
                unitCounts.Add(currentKey, 1);
            }
        }
        
        return unitCounts;
    }
}
