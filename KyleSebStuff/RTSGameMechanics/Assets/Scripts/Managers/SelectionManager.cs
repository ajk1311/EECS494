using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class SelectionManager {

    private static List<List<GameObject>> currentlySelectedObjects;
    public static Rect selectedSpace = new Rect(0, 0, 0, 0);

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

	public static void deselectGameObject(int playerID, GameObject  gameObject) {
		if (!currentlySelectedObjects[playerID -1].Remove(gameObject)) {
            Debug.Log("Removed a non-selected object");
        }
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
}
