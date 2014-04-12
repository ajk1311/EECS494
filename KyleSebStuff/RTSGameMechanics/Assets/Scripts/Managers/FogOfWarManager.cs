using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;

public static class FogOfWarManager {

	public static int playerID;

	private static int planeSize = 20;
	private static List<List<GameObject>> gridOfFog;

	private static List< List< List< List<int> > > > unitCount;

//	currentlySelectedObjects = new List<List<GameObject>>(2);
//	currentlySelectedObjects.Add(new List<GameObject>());
//	currentlySelectedObjects.Add(new List<GameObject>());

	public static void Init() {
		//init all the planes on the map
		GameObject origin = GameObject.Find("__Origin__");
		GameObject upperBound = GameObject.Find("__UpperMapBound__");
		
		float mapWidth = upperBound.transform.position.x - origin.transform.position.x;
		float mapHeight = upperBound.transform.position.z - origin.transform.position.z;
		
		int gridRows = (int) (mapWidth / planeSize);
		int gridColumns = (int) (mapHeight / planeSize);
		
		gridOfFog = new List<List<List<WorldObject>>>();
		
		for (int i = 0; i < gridRows; i++) {
			List<GameObject> innerList = new List<GameObject>();
			for(int j = 0; j < gridColumns; j++) {
				//instantiate the new plane with correct positionCenter
			}
			gridOfFog.Add(innerList);
		}
	}

	//Given Unit position, Player ID, Unit ID
	public static Int3 getMyFogTile(Int3 pos, int ID, int unitID) {
		//Return the Fog Tile the unit is in

		return pos;
	}

	public static void updateFogTileUnitCount(Int3 oldFogTilePos, Int3 newFogTilePos, int ID, int unitID) {
		//remove that unit from oldFogTile
		removeUnitFromFogTile (oldFogTilePos, unitID);
		//add unit to newFogTile
		addUnitToFogTile (newFogTilePos, unitID);
	}

	private static void removeUnitFromFogTile(Int3 fogTilePos, int unitID) {

	}

	private static void addUnitToFogTile(Int3 fogTilePos, int unitID) {
		
	}

	public static int getUnitCountForFogTile(Int3 fogTilePos, int ID) {
		return 0;
	}

}
