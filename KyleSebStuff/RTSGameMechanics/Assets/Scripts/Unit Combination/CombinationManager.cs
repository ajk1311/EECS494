﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CombinationManager {
	
	static Dictionary<string,  List< KeyValuePair<string,int>>> comboRef;
	
	public static void Init() {
		comboRef = new Dictionary<string,  List< KeyValuePair<string,int>>> ();
		generateCombinationReference ();
	}
	
	public static void getAvailableCombinations(int playerID) {
		List<KeyValuePair<string,int>> availableCombinations = new List<KeyValuePair<string,int>> ();
		Dictionary<string,int> myUnitCounts = SelectionManager.getUnitCounts(playerID);
		
		foreach(KeyValuePair<string,  List< KeyValuePair<string,int>>> obj in comboRef) {
			int amount = getValidComboAmount(obj.Value, myUnitCounts);
			if(amount != null) {
				KeyValuePair<string,int> pair = new KeyValuePair<string, int>(obj.Key, amount);
				availableCombinations.Add(pair);
			}
		}
	}
	
	public static bool combine(AssemblerScript script, string desiredUnit) {
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
		script.addUnitToQue (desiredUnit, unitCount);

		foreach(GameObject obj in comboUnits) {
			//Save position of Unit
			Vector3 pos = obj.transform.position;

			//Destroy Unit
			GameObject.Destroy(obj);

			//Instantiate unitBits with position of destroyed unit
			script.createUnitBits(pos, desiredUnit);
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
		List< KeyValuePair<string,int>> darkRedCubeList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairRedCube = new KeyValuePair<string, int>("RedCubeUnit", 3);
		darkRedCubeList.Add (pairRedCube);

		List< KeyValuePair<string,int>> darkRedSphereList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairRedSphere = new KeyValuePair<string, int>("RedSphereUnit", 3);
		darkRedSphereList.Add (pairRedSphere);

		comboRef.Add ("DarkRedCubeUnit", darkRedCubeList);
		comboRef.Add ("DarkRedSphereUnit", darkRedSphereList);

		List< KeyValuePair<string,int>> darkGreenCubeList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairGreenCube = new KeyValuePair<string, int>("GreenCubeUnit", 3);
		darkGreenCubeList.Add (pairGreenCube);
		
		List< KeyValuePair<string,int>> darkGreenSphereList = new List< KeyValuePair<string,int>> ();
		KeyValuePair<string, int> pairGreenSphere = new KeyValuePair<string, int>("GreenSphereUnit", 3);
		darkGreenSphereList.Add (pairGreenSphere);
		
		comboRef.Add ("DarkGreenCubeUnit", darkGreenCubeList);
		comboRef.Add ("DarkGreenSphereUnit", darkGreenSphereList);
	}
}
