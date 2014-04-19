﻿using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using RTS;

public static class GridManager {
	
	private const int SpaceSize = 2;

	private static List<List<List<WorldObject>>> grid;

	private static Dictionary<int, int[]> indexCache = new Dictionary<int, int[]>();

	public static void Init() {
		GameObject origin = GameObject.Find("__Origin__");
		GameObject upperBound = GameObject.Find("__UpperMapBound__");

		float mapWidth = upperBound.transform.position.x - origin.transform.position.x;
		float mapHeight = upperBound.transform.position.z - origin.transform.position.z;

		int gridRows = (int) (mapWidth / SpaceSize);
		int gridColumns = (int) (mapHeight / SpaceSize);

		grid = new List<List<List<WorldObject>>>();

		for (int i = 0; i < gridRows; i++) {
			List<List<WorldObject>> innerList = new List<List<WorldObject>>();
			for(int j = 0; j < gridColumns; j++) {
				innerList.Add(new List<WorldObject>());
			}
			grid.Add(innerList);
		}
	}

	public static void UpdatePosition(Int3 position, WorldObject wo) {
		bool cacheHit = true;
		int[] existingIndex;
		if (!indexCache.TryGetValue(wo.ID, out existingIndex)) {
			cacheHit = false;
			existingIndex = new int[2];
			indexCache.Add(wo.ID, existingIndex);
		}

		if (cacheHit) {
			List<WorldObject> group = grid[existingIndex[0]][existingIndex[1]];
			group.Remove(wo);
		}

		existingIndex[0] = position.x / SpaceSize / Int3.Precision;
		existingIndex[1] = position.z / SpaceSize / Int3.Precision;

		grid[existingIndex[0]][existingIndex[1]].Add(wo);
	}

	public static void RemoveFromGrid(WorldObject wo) {
		int[] existingIndex;
		if (!indexCache.TryGetValue(wo.ID, out existingIndex)) {
			throw new System.Exception("Trying to remove a unit from the grid that is not on it");
		}

		List<WorldObject> group = grid[existingIndex[0]][existingIndex[1]];
		group.Remove(wo);

		indexCache.Remove(wo.ID);
	}

	public static bool IsGridOccupied(int x, int z) {
		if (x < 0 || z < 0) {
			throw new System.IndexOutOfRangeException("Must pass positive coordinates to GridManager");
		}
		return grid[x][z].Count > 0;
	}

	public static List<WorldObject> GetGridOccupants(int x, int z) {
		if (x < 0 || z < 0) {
			throw new System.IndexOutOfRangeException("Must pass positive coordinates to GridManager");
		}
		return new List<WorldObject>(grid[x][z]);
	}

	public static List<WorldObject> GetObjectsInRadius(WorldObject wo, int radius) {
		int[] existingIndex;
		if (!indexCache.TryGetValue(wo.ID, out existingIndex)) {
			throw new System.Exception("Unit with id " + wo.ID + " is off the grid!");
		}

		int x = existingIndex[0];
		int z = existingIndex[1];

		int leftBound = System.Math.Max(x - radius, 0);
		int topBound = System.Math.Min(z + radius, grid[0].Count);
		int rightBound = System.Math.Min(x + radius, grid.Count);
		int bottomBound = System.Math.Max(z - radius, 0);

		List<WorldObject> results = new List<WorldObject>();

		for (int i = leftBound; i <= rightBound; i++) {
			for (int j = topBound; j >= bottomBound; j--) {
				results.AddRange(grid[i][j]);
			}
		}

		return results;
	}

	public static bool IsWithinRange(WorldObject source, WorldObject target, int range) {
		int[] sourceIndex;
		if (!indexCache.TryGetValue(source.ID, out sourceIndex)) {
			throw new System.Exception("Unit with id " + source.ID + " is off the grid!");
		}
		int[] targetIndex;
		if (!indexCache.TryGetValue(target.ID, out targetIndex)) {
			throw new System.Exception("Unit with id " + target.ID + " is off the grid!");
		}

		int sourceX = sourceIndex[0];
		int sourceZ = sourceIndex[1];

		int leftBound = sourceX - range;
		int topBound = sourceZ + range;
		int rightBound = sourceX + range;
		int bottomBound = sourceZ - range;

		int targetX = targetIndex[0];
		int targetZ = targetIndex[1];
		bool xWithinRange = false;
		bool zWithinRange = false;

		if (targetX >= leftBound && targetX <= rightBound) {
			xWithinRange = true;
		}
		if (targetZ <= topBound && targetZ >= bottomBound) {
			zWithinRange = true;
		}
		return xWithinRange && zWithinRange;
	}

	public static Int3 FindNextAvailPos(Int3 source, int width, int playerID) {
		int x = source.x / 2 / Int3.Precision;
		int z = source.z / 2 / Int3.Precision;

		int growthDirection = playerID == 1? 1 : -1;

		while(true) {
			if(x >= (int)RTSGameMechanics.GetMapSizes ().x) {
				break;
			}
			for(int i = 0; i < width/2; i++) {
				if(z+i > (int)RTSGameMechanics.GetMapSizes().z) {
					break;
				}
				if(grid[x][z+i].Count == 0) {
					return new Int3(x*2 + 1, 1, (z+i)*2 + 1) * Int3.Precision;
				}
				if(z-i < 0) {
					break;
				}
				else if (grid[x][z-i].Count == 0){
					return new Int3(x*2 + 1, 1, (z-i)*2 + 1) * Int3.Precision;
				}
			}
			x+= growthDirection;
		}
		return source + new Int3(1, 0, 1) * Int3.Precision;
	}

	public static List<Int3> GetDestinationCluster(Int3 destination, int unitCount) {
		int spacing = 2;
		int edgeLength = Mathf.CeilToInt(Mathf.Sqrt (unitCount));
		List<Int3> destinationCluster = new List<Int3> ();
		int x = Mathf.Max((destination.x / 2 / Int3.Precision) - edgeLength/2, 0);
		int z = Mathf.Max((destination.z / 2 / Int3.Precision) - edgeLength/2, 0);

		int xCoord = 0;
		int zCoord = 0;
		for(int i = 0; i < edgeLength; i++) {
			for(int j = 0; j < edgeLength; j++) {
				xCoord = Mathf.Min((int)RTSGameMechanics.GetMapSizes().x, (x + i*spacing)*2 + 1);
				zCoord = Mathf.Min((int)RTSGameMechanics.GetMapSizes().z, (z + j*spacing)*2 + 1);
				destinationCluster.Add(new Int3(xCoord, 1, zCoord) * Int3.Precision);
			}
		}
		return destinationCluster;
	}

	public static void PrintGrid() {
		Debug.Log("================ Printing Grid ================");
		for (int i = 0, w = grid.Count; i < w; i++) {
			for (int j = 0, h = grid[i].Count; j < h; j++) {
				List<WorldObject> group = grid[i][j];
				if (group.Count > 0) {
					string message = "[" + i + ", " + j + "]: ";
					for (int k = 0, g = grid[i][j].Count; k < g; k++) {
						message += grid[i][j][k].ID;
						if (k == g - 1) message += ", ";
					}
					Debug.Log(message);
				}
			}
		}
	}
}
