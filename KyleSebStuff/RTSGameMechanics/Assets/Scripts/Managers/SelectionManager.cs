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
        return selectedSpaces[playerID - 1];
    }

    public static void SetSelectedSpace(int playerID, Vector3[] space) {
        selectedSpaces[playerID - 1] = space;
    }

    public static int count(int playerID) {
        return currentlySelectedObjects [playerID - 1].Count;
    }

	public static void addSelectedGameObject(int playerID, GameObject gameObject) {
		currentlySelectedObjects[playerID -1].Add(gameObject);
        UpdateGUIModel(playerID);
    }

	private static void UpdateGUIModel(int playerID) {
        GUIModelManager.GUIModel model = new GUIModelManager.GUIModel();
        selectionModels[playerID-1] = model;

        model.leftPanelColumns = 1;
        model.leftPanelTitle = "Available Combinations";
        model.centerPanelColumns = 3;
        model.centerPanelTitle = "Selected Units";

        //check for new combos and add to left panel
		List<KeyValuePair<string,int>> availCombos = CombinationManager.getAvailableCombinations(playerID);
		foreach (KeyValuePair<string, int> pair in availCombos) {
			GUIModelManager.Button comboButton = new GUIModelManager.Button();
			comboButton.text = pair.Key + " x " + pair.Value;
            comboButton.hint = "Combine 3 units into 1 " + pair.Key;
            string desiredUnit = pair.Key;
			comboButton.clicked += () => 
			{
				CombinationManager.creatingCombination[playerID-1] = true;
				CombinationManager.desiredUnit[playerID-1] = desiredUnit;
			};
			model.AddButton(0, comboButton);
		}
        foreach (GameObject obj in currentlySelectedObjects[playerID - 1]) {
            GUIModelManager.Button button = new GUIModelManager.Button();
            WorldObject wo = obj.GetComponent<WorldObject>();
            button.icon = wo.buttonIcon;
            button.hint = "Select this unit";
            button.clicked += () => 
            {
                if(wo != null) {
                    deselectAllGameObjects(playerID);
                    wo.setCurrentlySelected(true);
                }
            };
            model.AddButton(1, button);
        }
        GUIModelManager.SetCurrentModel(playerID, model);
	}
	public static void deselectGameObject(int playerID, GameObject obj) {
        currentlySelectedObjects[playerID -1].Remove(obj);
    }

	public static void deselectAllGameObjects(int playerID) {
        if (GUIModelManager.GetCurrentModel(playerID) == selectionModels[playerID - 1]) {
            GUIModelManager.SetCurrentModel(playerID, null);
        }
        foreach (GameObject obj in currentlySelectedObjects[playerID-1]) {
            obj.GetComponent<WorldObject>().setCurrentlySelected(false);
        }
    }

	public static void moveUnits(int playerID, Vector3 destination, bool attackMove = false) {
		List<GameObject> selectedUnits = currentlySelectedObjects [playerID - 1];
		List<Int3> destinationCluster = GridManager.GetDestinationCluster ((Int3) destination, selectedUnits.Count);
		for(int i = 0; i < selectedUnits.Count; i++) {
			if(selectedUnits[i] != null) {
                Unit unit = selectedUnits[i].GetComponent<Unit>();
                if (unit != null) {
                    unit.IssueMoveCommand((Vector3)destinationCluster[i], attackMove);
                }
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
