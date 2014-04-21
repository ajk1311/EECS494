using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class AssemblerScript : MonoBehaviour {
	public int playerID;
	public Transform bitsUnitPrefab;

	//Units
	public Transform magentaHeapUnit;
	public Transform magentaStaticUnit;
	public Transform magentaBinaryTreeUnit;
	public Transform magentaArrayUnit;
	public Transform magentaPointerUnit;
	public Transform magentaFloatUnit;
	public Transform orangeStaticUnit;
	public Transform orangeHeapUnit;
	public Transform orangeBinaryTreeUnit;
	public Transform orangeArrayUnit;
	public Transform orangePointerUnit;
	public Transform orangeFloatUnit;

	private Dictionary<int, KeyValuePair<int,int>> unitQueue;

	public Int3 intPosition;

	void Start() {
		intPosition = (Int3) transform.position;
		unitQueue = new Dictionary<int, KeyValuePair<int,int>> ();
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
		else if(type == "OrangeArrayUnit") {
			Transform unit = Instantiate(orangeArrayUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "MagentaArrayUnit") {
			Transform unit = Instantiate(orangeBinaryTreeUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "OrangePointerUnit") {
			Transform unit = Instantiate(orangePointerUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "MagentaPointerUnit") {
			Transform unit = Instantiate(magentaPointerUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "OrangeFloatUnit") {
			Transform unit = Instantiate(orangeFloatUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
		else if(type == "MagentaPointerUnit") {
			Transform unit = Instantiate(magentaPointerUnit, pos, transform.rotation) as Transform;
			unit.GetComponent<WorldObject>().playerID = playerID;
		}
	}

	Vector3 posToBuildUnit() {
		return CombinationManager.spawnPoint[playerID-1];
	}
}
