using UnityEngine;
using System.Collections;
using RTS;
using Parse;

public static class ParseManager {

	private static int playerID;
	private static int gameID;
	public enum ParseEvent{ NewTierAchieved, BuildingDestroyed, Combination, Upgrade, TowerCapture, UnitCreation };

	public static void Init(int player, int GameID) {
		gameID = GameID;
		playerID = player;
	}

	public static void LogEvent(ParseEvent parseEvent) {
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
