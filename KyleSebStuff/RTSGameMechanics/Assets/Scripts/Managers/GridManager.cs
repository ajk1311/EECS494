using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

public static class GridManager {
	
	private const int SpaceSize = 2;

	private static List<List<List<WorldObject>>> grid;

	private static Dictionary<int, int[]> indexCache = new Dictionary<int, int[]>();

	public static void Init() {
		GameObject map = GameObject.Find("NewMap");
		Vector3 mapDimensions = map.transform.localScale;
		grid = new List<List<List<WorldObject>>>();
		for (int i = 0; i < (int) mapDimensions.x / SpaceSize; i++) {
			List<List<WorldObject>> innerList = new List<List<WorldObject>>();
			for(int j = 0; j < (int) mapDimensions.z / SpaceSize; j++) {
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

		position.x /= SpaceSize;
		position.z /= SpaceSize;
		existingIndex[0] = (int) Mathf.Floor(((Vector3) position).x);
		existingIndex[1] = (int) Mathf.Floor(((Vector3) position).z);

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

		int leftBound = (int) Mathf.Min(x - radius, 0);
		int topBound = (int) Mathf.Max(z + radius, grid[0].Count);
		int rightBound = (int) Mathf.Max(x + radius, grid.Count);
		int bottomBound = (int) Mathf.Min(z - radius, 0);

		List<WorldObject> results = new List<WorldObject>();

		for (int i = topBound; i <= bottomBound; i--) {
			for (int j = leftBound; j <= rightBound; j++) {
				results.AddRange(grid[i][j]);
			}
		}

		return results;
	} 
}
