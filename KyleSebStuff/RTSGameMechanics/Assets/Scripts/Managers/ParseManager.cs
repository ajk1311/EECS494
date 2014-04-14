using UnityEngine;
using System.Collections;
using RTS;
using Parse;

public static class ParseManager {

	private static int playerID;
	private static int gameID;
	public static enum ParseEvent{ NewTierAchieved, BuildingDestroyed, Combination, Upgrade, TowerCapture, UnitCreation };

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
