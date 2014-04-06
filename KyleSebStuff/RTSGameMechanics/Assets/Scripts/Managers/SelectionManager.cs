using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class SelectionManager {
	
	public static Vector3[] selectedSpace;
    private static List<List<GameObject>> currentlySelectedObjects;

    public static void Init() {
        currentlySelectedObjects = new List<List<GameObject>>(2);
		currentlySelectedObjects.Add(new List<GameObject>());
		currentlySelectedObjects.Add(new List<GameObject>());
    }

    public static int count(int playerID) {
        return currentlySelectedObjects[playerID -1].Count;
    }

	public static void addSelectedGameObject(int playerID, GameObject gameObject) {
		currentlySelectedObjects[playerID -1].Add(gameObject);
    }

	public static void deselectGameObject(int playerID, GameObject obj) {
		obj.GetComponent<WorldObject>().setCurrentlySelected(false);
    }

	public static void deselectAllGameObjects(int playerID) {
        foreach (GameObject obj in currentlySelectedObjects[playerID-1]) {
            obj.GetComponent<WorldObject>().setCurrentlySelected(false);
        }
    }

	public static void moveUnits(int playerID, Vector3 destination) {
        foreach (GameObject obj in currentlySelectedObjects[playerID-1]) {
            obj.GetComponent<Unit>().IssueMoveCommand(destination);
        }
    }

	public static void attackUnit(int playerID, GameObject target) {
        foreach (GameObject obj in currentlySelectedObjects[playerID -1]) {
            obj.GetComponent<Unit>().IssueAttackCommand(target);
        }
    }

	public static bool isSelected(int playerID, GameObject gameObject) {
        return currentlySelectedObjects[playerID-1].Find(foundObj => foundObj == gameObject);
    }

	public static List<GameObject> getSelectedUnits(int playerID) {
		return currentlySelectedObjects[playerID-1];
	}

	public static void removeUnitFromList(int playerID, GameObject gameObject) {
		currentlySelectedObjects [playerID - 1].Remove (gameObject);
	}

	public static Dictionary<string, int> getUnitCounts(int playerID) {
		Dictionary<string, int> unitCounts = new Dictionary<string, int> ();
		
		foreach(GameObject unit in currentlySelectedObjects[playerID-1]) {
			string currentKey = unit.GetComponent<WorldObject>().objectName;
			
			if(unitCounts.ContainsKey(currentKey)) {
				unitCounts[currentKey]++;
			}
			else {
				unitCounts.Add(currentKey, 1);
			}
		}
		
		return unitCounts;
	}
}
