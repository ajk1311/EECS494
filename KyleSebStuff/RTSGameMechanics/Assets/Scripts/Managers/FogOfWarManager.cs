using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;

public static class FogOfWarManager {
	public int NUM_OF_ROWS = 4;
	public int NUM_OF_COLUMNS = 4;
	public int mapWidth;
	public int mapHeight;

	public static int playerID;
	
	private static List<List<GameObject>> gridOfFog;

//	currentlySelectedObjects = new List<List<GameObject>>(2);
//	currentlySelectedObjects.Add(new List<GameObject>());
//	currentlySelectedObjects.Add(new List<GameObject>());

	public static void Init() {
		//init all the planes on the map
		GameObject origin = GameObject.Find("__Origin__");
		GameObject upperBound = GameObject.Find("__UpperMapBound__");
		
		mapWidth = (int)upperBound.transform.position.x - (int)origin.transform.position.x;
		mapHeight = (int)upperBound.transform.position.z - (int)origin.transform.position.z;
		
		gridOfFog = new List<List<GameObject>>();
		
		for (int i = 0; i < NUM_OF_ROWS; i++) {
			List<GameObject> innerList = new List<GameObject>();
			for(int j = 0; j < NUM_OF_COLUMNS; j++) {
				//instantiate the new plane with correct positionCenter
			}
			gridOfFog.Add(innerList);
		}
	}

	//Given Unit position, Player ID, Unit ID
	public static  GameObject getMyFogTile(Int3 pos) {
		//Return the Fog Tile the unit is in
		int row = pos.z * NUM_OF_ROWS / mapHeight;
		int column = pos.x * NUM_OF_COLUMNS / mapWidth;
		return gridOfFog [row] [column];
	}

	public static void updateFogTileUnitCount(GameObject oldTile, GameObject newTile, int playerID) {
		//remove that unit from oldFogTile
		removeUnitFromFogTile (oldTile, playerID);
		//add unit to newFogTile
		addUnitToFogTile (newTile, playerID);
	}

	public static void IsVisible(Int3 pos, int playerID){
        FogScript fog = getMyFogTile(pos).GetComponent<FogScript>();
        if (playerID == fog.getPlayerID())
            return true;
        else {
            if(fog.getFriendyUnitCount() > 0)
                return true;
            else
                return false;
        }
	}

	private static void removeUnitFromFogTile(GameObject fogTile, int playerID) {
        FogScript fog = fogTile.GetComponent<FogScript>();
		if (playerID == fog.getPlayerID()) {
            fog.decrementCounter();
	    }
	}

	private static void addUnitToFogTile(GameObject fogTile, int playerID) {
        FogScript fog = fogTile.GetComponent<FogScript>();
        if (playerID == fog.getPlayerID ()) {
            fog.incrementCounter();
        }
	}
}
