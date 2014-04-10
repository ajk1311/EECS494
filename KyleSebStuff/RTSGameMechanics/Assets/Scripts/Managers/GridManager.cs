using UnityEngine;
using System.Collections.Generic;

public static class GridManager {
	private static List<List<List<int>>> grid;
	private const int gridSize = 2;
	private static Dictionary<int, int[]> indexCache = new Dictionary<int, int[]>();
	public static void init() {
		GameObject map = GameObject.Find ("NewMap");
		Vector3 mapDimensions = map.transform.localScale;
		grid = new List<List<List<int>>> ();
		for (int i = 0; i < (int) mapDimensions.x / gridSize; i++) {
			List<List<int>> innerList = new List<List<int>>();
			for(int j = 0; j < (int) mapDimensions.z / gridSize; j++) {
				innerList.Add(new List<int>());
			}
			grid.Add(innerList);
		}
	}

	public static void updatePosition(Vector3 currPos, int ID) {

	}

	public static void removeFromGrid(int ID) {

	}

	public static int[] getGridIndex(Vector3 pos) {

	}

	public static bool isGridOccupied(int x, int z) {

	}

	public static int[] getGridOccupants(int x, int z) {

	}
}
