using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class AssemblerScript : MonoBehaviour, SSGameManager.IUpdatable {
	public int playerID;
	public Transform bitsUnitPrefab;

	//Units
	public Transform magentaHeapUnit;
	public Transform magentaStaticUnit;
	public Transform magentaBinaryTreeUnit;
	public Transform orangeStaticUnit;
	public Transform orangeHeapUnit;
	public Transform orangeBinaryTreeUnit;

	private Dictionary<string, KeyValuePair<int,int>> unitQueue;

	public Int3 intPosition;

	void Start() {
		intPosition = (Int3) transform.position;
		unitQueue = new Dictionary<string, KeyValuePair<int,int>> ();
		SSGameManager.Register(this);
	}
	
	public void GameUpdate (float deltaTime) {
		if(SSInput.GetKeyDown(playerID,SSKeyCode.Space)) {
			CombinationManager.combine(this, "MagentaHeapUnit");
			CombinationManager.combine(this, "MagentaStaticUnit");
			CombinationManager.combine(this, "MagentaBinaryTreeUnit");
			CombinationManager.combine(this, "OrangeStaticUnit");
			CombinationManager.combine(this, "OrangeHeapUnit");
			CombinationManager.combine(this, "OrangeBinaryTreeUnit");
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
		bitsUnit.GetComponent<UnitBitsScript>().assemblerScript = this;
		bitsUnit.GetComponent<UnitBitsScript>().desiredUnit = desiredUnit;
	}

	public void ReachedAssembler(string type) {
			int factor = unitQueue[type].Key;
			int newCurrentAmount = unitQueue[type].Value - 1;

			unitQueue[type] = new KeyValuePair<int,int>(factor, newCurrentAmount);

			if((newCurrentAmount % factor) == 0) {
				buildUnit(posToBuildUnit(), type);
		}
	}

	void buildUnit(Vector3 pos, string type) {
		//Instantiate new Unit
		//TODO Set unit to Selected if within view or always?
		if(type == "MagentaHeapUnit") {
			Transform unit = Instantiate(magentaHeapUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "MagentaStaticUnit") {
			Transform unit = Instantiate(magentaStaticUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "MagentaBinaryTreeUnit") {
			Transform unit = Instantiate(magentaBinaryTreeUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "OrangeHeapUnit") {
			Transform unit = Instantiate(orangeHeapUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "OrangeStaticUnit") {
			Transform unit = Instantiate(orangeStaticUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "OrangeBinaryTreeUnit") {
			Transform unit = Instantiate(orangeBinaryTreeUnit, pos, transform.rotation) as Transform;
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
