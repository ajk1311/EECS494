using UnityEngine;
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
	
	public static void combine(int playerID, string desiredUnit) {
		int amount = getValidComboAmount(comboRef[desiredUnit], SelectionManager.getUnitCounts(playerID));
		if(amount == 0) {
			return;
		}
		
		//It is a valid Combo so do it
		
	}
	
	private static int getValidComboAmount(List< KeyValuePair<string,int>> list, 
	                                       Dictionary<string,int> myUnitCounts) {
		int currentMax = 0;
		bool canCreate = true;
		
		foreach(KeyValuePair<string,int> pair in list) {
			if(myUnitCounts.ContainsKey(pair.Key)) {
				int amountSelected = myUnitCounts[pair.Key];
				int amountPossible = amountSelected / pair.Value;
				
				if(amountPossible <= currentMax) {
					currentMax = amountPossible;
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
		
	}
}
