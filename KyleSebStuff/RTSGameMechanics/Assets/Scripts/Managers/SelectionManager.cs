using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SelectionManager : MonoBehaviour {

	private List<GameObject> currentlySelectedObjects;

	public Rect selectedSpace = new Rect(0,0,0,0);

	// Use this for initialization
	void Start () {
		currentlySelectedObjects = new List<GameObject>();
	}

	public int count() {
		return currentlySelectedObjects.Count;
	}

	public void addSelectedGameObject(GameObject gameObject) {
		currentlySelectedObjects.Add (gameObject);
	}

	public void deselectGameObject(GameObject  gameObject) {
		if(!currentlySelectedObjects.Remove(gameObject)) {
			Debug.Log("Removed a non-selected object");
		}
	}

	public void deselectAllGameObjects() {
		foreach (GameObject obj in currentlySelectedObjects) {
			obj.GetComponent<WorldObject>().setCurrentlySelected(false);
		}
	}

    public void moveUnits(Vector3 destination){
        foreach(GameObject obj in currentlySelectedObjects){
            obj.GetComponent<Unit>().startMovement(destination);
        }
    }

	public bool isSelected(GameObject gameObject) {
		return currentlySelectedObjects.Find(foundObj => foundObj == gameObject);
	}
}
