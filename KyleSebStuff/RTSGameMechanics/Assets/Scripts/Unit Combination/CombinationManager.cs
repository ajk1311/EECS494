using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public static class CombinationManager {
	
	static Dictionary<string,  List< KeyValuePair<string,int>>> comboRef;

	public static bool[] creatingCombination;
	public static string[] desiredUnit;
	public static Vector3[] spawnPoint;
	public static int currentCombinationID;
	
	public static void Init() {
		comboRef = new Dictionary<string,  List< KeyValuePair<string,int>>> ();
		creatingCombination = new bool[2];
		desiredUnit = new string[2];
		spawnPoint = new Vector3[2];
		generateCombinationReference ();
		currentCombinationID = 0;
	}
	
	public static List<KeyValuePair<string,int>> getAvailableCombinations(int playerID) {
		List<KeyValuePair<string,int>> availableCombinations = new List<KeyValuePair<string,int>> ();
		Dictionary<string,int> myUnitCounts = SelectionManager.getUnitCounts(playerID);
		
		foreach(KeyValuePair<string,  List< KeyValuePair<string,int>>> obj in comboRef) {
			int amount = getValidComboAmount(obj.Value, myUnitCounts);
			if(amount != 0) {
				KeyValuePair<string,int> pair = new KeyValuePair<string, int>(obj.Key, amount);
				availableCombinations.Add(pair);
			}
		}
		return availableCombinations;
	}

	public static bool combine(AssemblerScript script, string desiredUnit) {
		currentCombinationID++;

		int playerID = script.playerID;

		List<GameObject> selectedUnits = SelectionManager.getSelectedUnits(playerID);

		List<GameObject> comboUnits = new List<GameObject>();

		List< KeyValuePair<string,int>> comboCounts = comboRef [desiredUnit];

		int amount = getValidComboAmount(comboCounts, SelectionManager.getUnitCounts(playerID));
		if(amount == 0) {
			//Not A Valid Combo
			return false;
		}
		
		//Get all the Units to Combine
		//total units needed to create new unit
		int unitCount = 0;
		foreach(KeyValuePair<string,int> pair in comboCounts) {
			int unitsFound = 0;
			unitCount += pair.Value;
			foreach(GameObject obj in selectedUnits) {
				if(obj.GetComponent<WorldObject>().objectName == pair.Key && unitsFound !=pair.Value) {
					comboUnits.Add(obj);
					unitsFound++;
				}
			}
		}

		//Que a Unit for Assembler to Start looking to Create
		script.addUnitToQue (currentCombinationID, unitCount);

		if(GameObject.Find("Player").GetComponent<PlayerScript>().id == playerID) {
			GameObject.Find("Player").GetComponent<PlayerScript>().combinationMemoryUpdate(desiredUnit);
		}
		else {
			GameObject.Find("Opponent").GetComponent<PlayerScript>().combinationMemoryUpdate(desiredUnit);
		}

		foreach(GameObject obj in comboUnits) {
			//Save position of Unit
			Vector3 pos = obj.transform.position;

			//Destroy Unit
			obj.GetComponent<WorldObject>().DestroyInGame();

			//Instantiate unitBits with position of destroyed unit
			script.createUnitBits(pos, desiredUnit, currentCombinationID);
		}

		return true;
	}

	private static int getValidComboAmount(List< KeyValuePair<string,int>> list, 
	                                       Dictionary<string,int> myUnitCounts) {
		int currentMax = 100;
		bool canCreate = true;

		foreach(KeyValuePair<string,int> pair in list) {
			if(myUnitCounts.ContainsKey(pair.Key)) {
				int amountSelected = myUnitCounts[pair.Key];
				int amountPossible = amountSelected / pair.Value;
				
				if(amountPossible <= currentMax) {
					currentMax = amountPossible;
				}

				if(amountPossible == 0) {
					canCreate = false;
				}
			}
			else {
				canCreate = false;
			}
		}
		
		if(canCreate) {
			return currentMax;
		}
		
		return 0;
	}
	
	private static void generateCombinationReference() {
//		//Magenta Unit Combos
		List< KeyValuePair<string,int>> magentaPointerUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairMagentaIntUnit = new KeyValuePair<string, int>("MagentaIntUnit", 3);
		magentaPointerUnitList.Add (pairMagentaIntUnit);

		List< KeyValuePair<string,int>> magentaHeapUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairMagentaDoubleUnit = new KeyValuePair<string, int>("MagentaDoubleUnit", 3);
		magentaHeapUnitList.Add (pairMagentaDoubleUnit);

		List< KeyValuePair<string,int>> magentaFloatUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairMagentaLongUnit = new KeyValuePair<string, int>("MagentaLongUnit", 3);
		magentaFloatUnitList.Add (pairMagentaLongUnit);

		comboRef.Add ("MagentaPointerUnit", magentaPointerUnitList);
		comboRef.Add ("MagentaHeapUnit", magentaHeapUnitList);
		comboRef.Add ("MagentaFloatUnit", magentaFloatUnitList);

		List< KeyValuePair<string,int>> magentaBinaryTreeUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairMagentaPointerUnit = new KeyValuePair<string, int>("MagentaPointerUnit", 3);
		magentaBinaryTreeUnitList.Add (pairMagentaPointerUnit);
		
		List< KeyValuePair<string,int>> magentaArrayUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairMagentaHeapUnit = new KeyValuePair<string, int>("MagentaHeapUnit", 3);
		magentaArrayUnitList.Add (pairMagentaHeapUnit);
		
		List< KeyValuePair<string,int>> magentaStaticUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairMagentaFloatUnit = new KeyValuePair<string, int>("MagentaFloatUnit", 3);
		magentaStaticUnitList.Add (pairMagentaFloatUnit);
		
		comboRef.Add ("MagentaBinaryTreeUnit", magentaBinaryTreeUnitList);
		comboRef.Add ("MagentaArrayUnit", magentaArrayUnitList);
		comboRef.Add ("MagentaStaticUnit", magentaStaticUnitList);

		//Orange Unit Combos
		List< KeyValuePair<string,int>> orangePointerUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairOrangeIntUnit = new KeyValuePair<string, int>("OrangeIntUnit", 3);
		orangePointerUnitList.Add (pairOrangeIntUnit);
		
		List< KeyValuePair<string,int>> orangeHeapUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairOrangeDoubleUnit = new KeyValuePair<string, int>("OrangeDoubleUnit", 3);
		orangeHeapUnitList.Add (pairOrangeDoubleUnit);
		
		List< KeyValuePair<string,int>> orangeFloatUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairOrangeLongUnit = new KeyValuePair<string, int>("OrangeLongUnit", 3);
		orangeFloatUnitList.Add (pairOrangeLongUnit);
		
		comboRef.Add ("OrangePointerUnit", orangePointerUnitList);
		comboRef.Add ("OrangeHeapUnit", orangeHeapUnitList);
		comboRef.Add ("OrangeFloatUnit", orangeFloatUnitList);
		
		List< KeyValuePair<string,int>> orangeBinaryTreeUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairOrangePointerUnit = new KeyValuePair<string, int>("OrangePointerUnit", 3);
		orangeBinaryTreeUnitList.Add (pairOrangePointerUnit);
		
		List< KeyValuePair<string,int>> orangeArrayUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairOrangeHeapUnit = new KeyValuePair<string, int>("OrangeHeapUnit", 3);
		orangeArrayUnitList.Add (pairOrangeHeapUnit);
		
		List< KeyValuePair<string,int>> orangeStaticUnitList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairOrangeFloatUnit = new KeyValuePair<string, int>("OrangeFloatUnit", 3);
		orangeStaticUnitList.Add (pairOrangeFloatUnit);
		
		comboRef.Add ("OrangeBinaryTreeUnit", orangeBinaryTreeUnitList);
		comboRef.Add ("OrangeArrayUnit", orangeArrayUnitList);
		comboRef.Add ("OrangeStaticUnit", orangeStaticUnitList);
	}
}
