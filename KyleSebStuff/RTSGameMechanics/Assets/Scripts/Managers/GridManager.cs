using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

public static class GridManager {
	
	private const int SpaceSize = 2;

	private static List<List<List<int>>> grid;

	private static Dictionary<int, int[]> indexCache = new Dictionary<int, int[]>();

	public static void Init() {
		GameObject map = GameObject.Find("NewMap");
		Vector3 mapDimensions = map.transform.localScale;
		grid = new List<List<List<int>>>();
		for (int i = 0; i < (int) mapDimensions.x / SpaceSize; i++) {
			List<List<int>> innerList = new List<List<int>>();
			for(int j = 0; j < (int) mapDimensions.z / SpaceSize; j++) {
				innerList.Add(new List<int>());
			}
			grid.Add(innerList);
		}
	}

	public static void UpdatePosition(Vector3 position, int ID) {
		bool cacheHit = true;
		int[] existingIndex;
		if (!indexCache.TryGetValue(ID, out existingIndex)) {
			cacheHit = false;
			existingIndex = new int[2];
			indexCache.Add(ID, existingIndex);
		}

		if (cacheHit) {
			List<int> group = grid[existingIndex[0]][existingIndex[1]];
			group.Remove(ID);
		}

		Int3 iPosition = new Int3(position);
		iPosition.x /= SpaceSize;
		iPosition.z /= SpaceSize;
		existingIndex[0] = (int) Mathf.Floor(((Vector3) iPosition).x);
		existingIndex[1] = (int) Mathf.Floor(((Vector3) iPosition).z);

		grid[existingIndex[0]][existingIndex[1]].Add(ID);
	}

	public static void RemoveFromGrid(int ID) {
		int[] existingIndex;
		if (!indexCache.TryGetValue(ID, out existingIndex)) {
			throw new System.Exception("Trying to remove a unit from the grid that is not on it");
		}

		List<int> group = grid[existingIndex[0]][existingIndex[1]];
		group.Remove(ID);

		indexCache.Remove(ID);
	}

	public static bool IsGridOccupied(int x, int z) {
		if (x < 0 || z < 0) {
			throw new System.IndexOutOfRangeException("Must pass positive coordinates to GridManager");
		}
		return grid[x][z].Count > 0;
	}

	public static List<int> GetGridOccupants(int x, int z) {
		if (x < 0 || z < 0) {
			throw new System.IndexOutOfRangeException("Must pass positive coordinates to GridManager");
		}
		return new List<int>(grid[x][z]);
	}

	public static List<int> GetObjectsInRadius(int ID, int radius) {
		int[] existingIndex;
		if (!indexCache.TryGetValue(ID, out existingIndex)) {
			throw new System.Exception("Unit with id " + ID + " is off the grid!");
		}

		int x = existingIndex[0];
		int z = existingIndex[1];

		int leftBound = (int) Mathf.Min(x - radius, 0);
		int topBound = (int) Mathf.Max(z + radius, grid[0].Count);
		int rightBound = (int) Mathf.Max(x + radius, grid.Count);
		int bottomBound = (int) Mathf.Min(z - radius, 0);

		List<int> results = new List<int>();

		for (int i = topBound; i <= bottomBound; i--) {
			for (int j = leftBound; j <= rightBound; j++) {
				results.AddRange(grid[i][j]);
			}
		}

		return results;
	} 
}
