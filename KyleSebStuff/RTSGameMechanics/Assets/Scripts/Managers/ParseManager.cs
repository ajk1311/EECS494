using UnityEngine;
using System.Collections;
using RTS;
using Parse;

public static class ParseManager {

	private int playerID;
	private int gameID;
	public enum ParseEvent{ NewTierAchieved, BuildingDestroyed, Combination, Upgrade, TowerCapture, UnitCreation };

	public void Init(int ID) {
		gameID = 
		playerID = ID;
	}

	public void LogEvent(ParseEvent parseEvent) {
		switch(parseEvent) {

			case ParseEvent.NewTierAchieved:
				
			case ParseEvent.BuildingDestroyed:

			case ParseEvent.Combination:

			case ParseEvent.Upgrade:

			case ParseEvent.TowerCapture:

			case ParseEvent.UnitCreation:

			default:
			return;
		}
	}
}
