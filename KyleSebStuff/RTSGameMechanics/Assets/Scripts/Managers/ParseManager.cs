using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using RTS;
using Parse;

public static class ParseManager {

	private static int playerID;
	private static int gameID;
	public enum ParseEvent{ NewTierAchieved, BuildingDestroyed, Combination, Upgrade, TowerCapture, TowerLoss, UnitCreation };

	public static void Init(int player, int GameID) {
		gameID = GameID;
		playerID = player;
	}

	public static void LogEvent(ParseEvent parseEvent, int player, string arg1, string arg2 = null) {
		if(player != playerID) {
			Debug.Log ("id not the same");
			Debug.Log ("event playerID ");
			return;
		}
		switch(parseEvent) {
			case ParseEvent.NewTierAchieved:
				ParseObject tierAchievement = new ParseObject("NewTierAchieved");
				tierAchievement["tier"] = arg1;
				tierAchievement["player"] = playerID;
				tierAchievement["gameID"] = gameID;
				tierAchievement.SaveAsync();
				break;
			case ParseEvent.BuildingDestroyed:
				ParseObject buildingDestroyed = new ParseObject("BuildingDestroyed");
				buildingDestroyed["type"] = arg1;
				buildingDestroyed["player"] = playerID;
				buildingDestroyed["gameID"] = gameID;
				buildingDestroyed.SaveAsync();
				break;
			case ParseEvent.Combination:
				ParseObject combination = new ParseObject("combination");
				combination["type"] = arg1;
				combination["player"] = playerID;
				combination["gameID"] = gameID;
				combination.SaveAsync();
				break;

			case ParseEvent.Upgrade:
				ParseObject upgrade = new ParseObject("upgrade");
				upgrade["type"] = arg1;
				upgrade["player"] = playerID;
				upgrade.SaveAsync();
				break;
			case ParseEvent.TowerCapture:
				ParseObject towerCapture = new ParseObject("towerCapture");
				towerCapture["type"] = arg1;
				towerCapture["player"] = playerID;
				towerCapture["id"] = arg2;
				towerCapture["captured"] = DateTime.Now;
				towerCapture["gameID"] = gameID;
				break;
			case ParseEvent.TowerLoss:
				var query = ParseObject.GetQuery("towerCapture")
					.WhereEqualTo("player", playerID)
					.WhereEqualTo("gameID", gameID)
					.WhereEqualTo("type", arg1)
					.WhereEqualTo("lost", null)
					.Limit(1);
				query.FirstAsync().ContinueWith(t =>
				{
					ParseObject captureEvent = t.Result;
					captureEvent["lost"] = DateTime.Now;
					captureEvent.SaveAsync();
				});
				break;
			case ParseEvent.UnitCreation:
				ParseObject unitCreation = new ParseObject("unitCreation");
				unitCreation["type"] = arg1;
				unitCreation["method"] = arg2;
				unitCreation["player"] = playerID;
				unitCreation["gameID"] = gameID;
				Debug.Log ("got in here");
				unitCreation.SaveAsync();
				break;
			default:
				return;
		}
	}
}
