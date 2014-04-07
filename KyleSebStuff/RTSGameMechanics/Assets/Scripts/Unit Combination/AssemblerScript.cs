using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssemblerScript : MonoBehaviour, SSGameManager.IUpdatable {
	public int playerID;
	public Transform bitsUnitPrefab;

	//Units
	public Transform blueUnit;
	public Transform greenUnit;

	private Dictionary<string, KeyValuePair<int,int>> unitQueue;

	public int PlayerID {
		get { return playerID; }
	}

	void Start() {
		unitQueue = new Dictionary<string, KeyValuePair<int,int>> ();
		SSGameManager.Register(this);
	}

	//TODO FOR TESTING
	public void GameUpdate (float deltaTime) {
			if(Input.GetKeyDown(KeyCode.B)) {
				CombinationManager.combine(playerID,"Blue");
			}

		if(Input.GetKeyDown(KeyCode.G)) {
			CombinationManager.combine(playerID,"Green");
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
		string type = other.gameObject.GetComponent<UnitBitsScript> ().desiredUnit;

		int factor = unitQueue[type].Key;
		int newCurrentAmount = unitQueue[type].Value - 1;

		unitQueue[type] = new KeyValuePair<int,int>(factor, newCurrentAmount);

		if((newCurrentAmount % factor) == 0) {
			buildUnit(posToBuildUnit(), type);
		}

		Destroy(other.gameObject);
	}

	void buildUnit(Vector3 pos, string type) {
		//Instantiate new Unit
		//Set unit to Selected if within view or always?
		if(type == "Blue") {
			Transform unit = Instantiate(blueUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "Green") {
			Transform unit = Instantiate(greenUnit, pos, transform.rotation) as Transform;
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
