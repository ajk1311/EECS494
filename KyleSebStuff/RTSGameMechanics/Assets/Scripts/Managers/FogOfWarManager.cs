using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathfinding;

public static class FogOfWarManager {

	public static int playerID;

	private static int planeSizeX;
	private static int planeSizeZ;
	private static int planeAmount;
	private static float fogHeight;

	private static List<List<GameObject>> gridOfFog;

	private static GameObject fogObject;

	private static int gridRows;
	private static int gridColumns;

	public static void Init() {
		fogObject = (GameObject) Resources.Load("FogOfWar/Fog");

		//init all the planes on the map
		GameObject origin = GameObject.Find("__Origin__");
		GameObject upperBound = GameObject.Find("__UpperMapBound__");
		
		float mapWidth = upperBound.transform.position.x - origin.transform.position.x;
		float mapHeight = upperBound.transform.position.z - origin.transform.position.z; 

		planeAmount = 7;
		fogHeight = 0.25f;
		planeSizeX = (int) (mapWidth / planeAmount);
		planeSizeZ = (int) (mapHeight / planeAmount);

		gridOfFog = new List<List<GameObject>>();

		int initialX = planeSizeX / 2;
		int initialZ = planeSizeZ / 2;
		for (int i = 0; i < planeAmount; i++) {
			List<GameObject> innerList = new List<GameObject>();
			for(int j = 0; j < planeAmount; j++) {
				Vector3 pos = new Vector3(initialX + j*planeSizeX, 0.25f, initialZ + i*planeSizeZ);
				GameObject fogTile = GameObject.Instantiate (fogObject, pos, Quaternion.identity) as GameObject;
				fogTile.transform.localScale = new Vector3(planeSizeX*fogTile.transform.localScale.x, 0.25f, planeSizeZ*fogTile.transform.localScale.z);
				innerList.Add(fogTile);
			}
			gridOfFog.Add(innerList);
		}
	}

	public static GameObject getMyFogTile(Int3 pos) {

		int zCord = pos.z / planeSizeZ / Int3.Precision;
		int xCord = pos.x / planeSizeX / Int3.Precision;

		Debug.Log ("zCord: " + zCord);
		Debug.Log ("xCord: " + xCord);

		return gridOfFog[zCord][xCord];
	}

	public static void updateFogTileUnitCount(GameObject oldFogTile, GameObject newFogTile, int pID) {
		if (oldFogTile != null) {
			removeUnitFromFogTile (oldFogTile, pID);
		}
		addUnitToFogTile (newFogTile, pID);
	}

	private static void removeUnitFromFogTile(GameObject fogTile, int pID) {
		if(pID == playerID) {
			if(fogTile.GetComponent<FogScript> ().friendlyUnitCount != 0) {
				fogTile.GetComponent<FogScript> ().friendlyUnitCount--;
			}
		}
		else {
			if(fogTile.GetComponent<FogScript> ().enemyUnitCount != 0) {
				fogTile.GetComponent<FogScript> ().enemyUnitCount--;
			}
		}
	}

	private static void addUnitToFogTile(GameObject fogTile, int pID) {
		if(pID == playerID) {
			fogTile.GetComponent<FogScript> ().friendlyUnitCount++;
		}
		else {
			fogTile.GetComponent<FogScript> ().enemyUnitCount++;
		}
	}

	public static bool isVisible(GameObject fogTile, int ID) {
		int count = fogTile.GetComponent<FogScript> ().friendlyUnitCount;

		if(ID == playerID) {
			//always show friendly units
			return true;
		}
		else if(count > 0) {
			//show enemey units if you have friendly units in same fog tile
			return true;
		}
		else {
			//no friendlies in fog tile so hide enemey units
			return false;
		}
	}

}
