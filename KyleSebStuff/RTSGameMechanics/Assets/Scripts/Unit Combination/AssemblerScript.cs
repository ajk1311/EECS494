using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssemblerScript : MonoBehaviour, SSGameManager.IUpdatable {
	public int playerID;
	public Transform bitsUnitPrefab;

	//Units
	public Transform darkGreenCubeUnit;
	public Transform darkGreenSphereUnit;
	public Transform darkRedCubeUnit;
	public Transform darkRedSphereUnit;

	private Dictionary<string, KeyValuePair<int,int>> unitQueue;

	public int PlayerID {
		get { return playerID; }
	}

	void Start() {
		unitQueue = new Dictionary<string, KeyValuePair<int,int>> ();
		SSGameManager.Register(this);
	}
	
	public void GameUpdate (float deltaTime) {
		if(SSInput.GetKeyDown(playerID,SSKeyCode.Space)) {
			CombinationManager.combine(this, "DarkGreenCubeUnit");
			CombinationManager.combine(this, "DarkGreenSphereUnit");
			CombinationManager.combine(this, "DarkRedCubeUnit");
			CombinationManager.combine(this, "DarkRedSphereUnit");
		}
	}

	public void addUnitToQue(string type, int amount) {
		if(unitQueue.ContainsKey(type)) {
			int currentAmount = unitQueue[type].Value;
			unitQueue[type] = new KeyValuePair<int,int>(amount, currentAmount + amount);
		}
		else {
			KeyValuePair<int,int>  pair2 =  new KeyValuePair<int, int>(amount,amount);
			unitQueue.Add(type, pair2);
		}
	}

	public void createUnitBits(Vector3 pos, string desiredUnit) {
		Transform bitsUnit = Instantiate (bitsUnitPrefab, pos, transform.rotation) as Transform;
		bitsUnit.GetComponent<UnitBitsScript> ().destination = transform.position;
		bitsUnit.GetComponent<UnitBitsScript> ().desiredUnit = desiredUnit;
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag == "UnitBits") {
			string type = other.gameObject.GetComponent<UnitBitsScript> ().desiredUnit;

			int factor = unitQueue[type].Key;
			int newCurrentAmount = unitQueue[type].Value - 1;

			unitQueue[type] = new KeyValuePair<int,int>(factor, newCurrentAmount);

			if((newCurrentAmount % factor) == 0) {
				buildUnit(posToBuildUnit(), type);
			}

			Destroy(other.gameObject);
		}
	}

	void buildUnit(Vector3 pos, string type) {
		//Instantiate new Unit
		//TODO Set unit to Selected if within view or always?
		if(type == "DarkGreenCubeUnit") {
			Transform unit = Instantiate(darkGreenCubeUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "DarkGreenSphereUnit") {
			Transform unit = Instantiate(darkGreenSphereUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "DarkRedCubeUnit") {
			Transform unit = Instantiate(darkRedCubeUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "DarkRedSphereUnit") {
			Transform unit = Instantiate(darkRedSphereUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
	}

	Vector3 posToBuildUnit() {
		return transform.position;
	}

	void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
