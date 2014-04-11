using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

public static class GridManager {
	
	private const int SpaceSize = 2;

	private static List<List<List<Unit>>> grid;

	private static Dictionary<int, int[]> indexCache = new Dictionary<int, int[]>();

	public static void Init() {
		GameObject origin = GameObject.Find("__Origin__");
		GameObject upperBound = GameObject.Find("__UpperMapBound__");

		float mapWidth = upperBound.transform.position.x - origin.transform.position.x;
		float mapHeight = upperBound.transform.position.z - origin.transform.position.z;

		int gridRows = (int) (mapWidth / SpaceSize);
		int gridColumns = (int) (mapHeight / SpaceSize);

		grid = new List<List<List<Unit>>>();

		for (int i = 0; i < gridRows; i++) {
			List<List<Unit>> innerList = new List<List<Unit>>();
			for(int j = 0; j < gridColumns; j++) {
				innerList.Add(new List<Unit>());
			}
			grid.Add(innerList);
		}
	}

	public static int[] UpdatePosition(Int3 position, Unit wo) {
		bool cacheHit = true;
		int[] existingIndex;
		if (!indexCache.TryGetValue(wo.ID, out existingIndex)) {
			cacheHit = false;
			existingIndex = new int[2];
			indexCache.Add(wo.ID, existingIndex);
		}

		if (cacheHit) {
			List<Unit> group = grid[existingIndex[0]][existingIndex[1]];
			group.Remove(wo);
		}

		existingIndex[0] = position.x / SpaceSize / Int3.Precision;
		existingIndex[1] = position.z / SpaceSize / Int3.Precision;

		grid[existingIndex[0]][existingIndex[1]].Add(wo);

		return new int[] { existingIndex[0], existingIndex[1] };
	}

	public static void RemoveFromGrid(Unit wo) {
		int[] existingIndex;
		if (!indexCache.TryGetValue(wo.ID, out existingIndex)) {
			throw new System.Exception("Trying to remove a unit from the grid that is not on it");
		}

		List<Unit> group = grid[existingIndex[0]][existingIndex[1]];
		group.Remove(wo);

		indexCache.Remove(wo.ID);
	}

	public static bool IsGridOccupied(int x, int z) {
		if (x < 0 || z < 0) {
			throw new System.IndexOutOfRangeException("Must pass positive coordinates to GridManager");
		}
		return grid[x][z].Count > 0;
	}

	public static List<Unit> GetGridOccupants(int x, int z) {
		if (x < 0 || z < 0) {
			throw new System.IndexOutOfRangeException("Must pass positive coordinates to GridManager");
		}
		return new List<Unit>(grid[x][z]);
	}

	public static List<Unit> GetObjectsInRadius(Unit wo, int radius) {
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

		List<Unit> results = new List<Unit>();

		for (int i = leftBound; i <= rightBound; i++) {
			for (int j = topBound; j >= bottomBound; j--) {
				results.AddRange(grid[i][j]);
			}
		}

		return results;
	} 

	public static void PrintGrid() {
		Debug.Log("================ Printing Grid ================");
		for (int i = 0, w = grid.Count; i < w; i++) {
			for (int j = 0, h = grid[i].Count; j < h; j++) {
				List<Unit> group = grid[i][j];
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
