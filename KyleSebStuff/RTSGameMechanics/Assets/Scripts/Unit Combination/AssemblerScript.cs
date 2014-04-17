﻿using UnityEngine;
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

	private Dictionary<int, KeyValuePair<int,int>> unitQueue;

	public Int3 intPosition;

	void Start() {
		intPosition = (Int3) transform.position;
		unitQueue = new Dictionary<int, KeyValuePair<int,int>> ();
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

	public void addUnitToQue(int combinationID, int amount) {
		KeyValuePair<int,int>  pair2 =  new KeyValuePair<int, int>(amount,amount);
		unitQueue.Add(combinationID, pair2);
	}

	public void createUnitBits(Vector3 pos, string desiredUnit, int combinationID) {
		Transform bitsUnit = Instantiate (bitsUnitPrefab, pos, transform.rotation) as Transform;
		bitsUnit.GetComponent<UnitBitsScript>().assemblerScript = this;
		bitsUnit.GetComponent<UnitBitsScript>().desiredUnit = desiredUnit;
		bitsUnit.GetComponent<UnitBitsScript> ().destination = (Int3) CombinationManager.spawnPoint [playerID - 1];
		bitsUnit.GetComponent<UnitBitsScript> ().combinationID = combinationID;
	}

	public void ReachedAssembler(int id, Vector3 pos, string type) {
		int factor = unitQueue[id].Key;
		int newCurrentAmount = unitQueue[id].Value - 1;

		unitQueue[id] = new KeyValuePair<int,int>(factor, newCurrentAmount);

		if((newCurrentAmount % factor) == 0) {
			buildUnit(pos, type);
		}
	}

	public void buildUnit(Vector3 pos, string type) {
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
		return CombinationManager.spawnPoint[playerID-1];
	}

	void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
