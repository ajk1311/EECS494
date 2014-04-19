using UnityEngine;
using System.Collections.Generic;
using SSProtoBufs;

/**
 * Acts just like Unity's Input class, allowing objects to query for input on any given frame
 */
public static class SSInput {
	/** Empty coordinate. Used for out paraters */
	public static readonly Vector3 Empty = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	/** Keeps an input dispatch table for each player */
	private static List<Dictionary<int, Command>> sDispatchTables;

	/** Called when the game manager starts */
	public static void Init() {
		sDispatchTables = new List<Dictionary<int, Command>>(2);
		sDispatchTables.Add(new Dictionary<int, Command>());
		sDispatchTables.Add(new Dictionary<int, Command>());
	}

	/** Remvoes all input for the frame */
	public static void ClearInput() {
		foreach (Dictionary<int, Command> table in sDispatchTables) {
			table.Clear();
		}
	}

	/** Offers up input for a frame */
	public static void AddInput(int playerID, Queue<Command> commands) {
		Dictionary<int, Command> table = sDispatchTables[playerID - 1];
		foreach (Command cmd in commands) {
			if (!table.ContainsKey(cmd.keyCode)) {
				table.Add(cmd.keyCode, cmd);
			}
		}
	}

	/** Returns true if the player with playerID pressed the key with keyCode for a frame */
	public static bool GetKey(int playerID, int keyCode) {
		return sDispatchTables[playerID - 1].ContainsKey(keyCode);
	}

	/** 
	 * Returns true if the player with playerID clicked the given mouseButton for a frame.
	 * Puts the coordinates of the click into mousePosition.
	 */
	public static bool GetMouseClick(int playerID, int mouseButton, out Vector3 mousePosition) {
		// Assign the keyCode ourselves so that the function acts like Input.GetMouseButtonDown(int)
		int keyCode = mouseButton == 0 ? SSKeyCode.Mouse0Click : SSKeyCode.Mouse1Click;
		Command input;
		if (!sDispatchTables[playerID - 1].TryGetValue(keyCode, out input)) {
			mousePosition = Empty;
			return false;
		}
		mousePosition = new Vector3(input.x0, input.y0, input.z0);
		return true;
	}

	/**
	 * Returns true if the player with playerID dragged the mouse and made a selection for a frame.
	 * Puts the coordinates of the beginning and end of the drag into downPosition and upPosition, respectively.
	 */
    public static bool GetMouseDragSelection(int playerID, out Vector3 downPosition, out Vector3 upPosition) {
		Command input;
		if (!sDispatchTables[playerID - 1].TryGetValue(SSKeyCode.Mouse0Select, out input)) {
			downPosition = upPosition = Empty;
			return false;
		}
		downPosition = new Vector3(input.x0, input.y0, input.z0);
		upPosition = new Vector3(input.x1, input.y1, input.z1);
		return true;
    }

	/**
	 * Returns true if the player clicked a GUI button. Puts the panel and index of the button into clickPosition.
	 */
	public static bool GetGUIClick(int playerID, out Vector3 clickPosition) {
		Command input;
		if (!sDispatchTables[playerID - 1].TryGetValue(SSKeyCode.GUIClick, out input)) {
			clickPosition = Empty;
			return false;
		}
		clickPosition = new Vector3(input.x0, input.y0, 0);
		return true;
	}
}
