using UnityEngine;
using System.Collections.Generic;
using GameProtoBufs;

public static class GameCommands
{
	public static readonly Vector3 Empty = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	private static List<Dictionary<KeyCode, Command>> commandDispatch;

	public static void Init()
	{
		commandDispatch = new List<Dictionary<KeyCode, Command>>(2);
		commandDispatch.Add(new Dictionary<KeyCode, Command>());
		commandDispatch.Add(new Dictionary<KeyCode, Command>());
	}

	public static void ClearInput()
	{
		foreach (Dictionary<KeyCode, Command> map in commandDispatch)
		{
			map.Clear();
		}
	}

	public static void AddInput(int playerID, Queue<Command> commands)
	{
		Dictionary<KeyCode, Command> map = commandDispatch[playerID - 1];
		foreach (Command cmd in commands)
		{
			if (!map.ContainsKey(cmd.KeyCode))
			{
				map.Add(cmd.KeyCode, cmd);
			}
		}
	}

	public static bool GetKeyDown(int playerID, KeyCode keyCode)
	{
		return commandDispatch[playerID - 1].ContainsKey(keyCode);
	}

	public static Vector3 GetMouseButtonClick(int playerID, int mouseButton)
	{
		KeyCode keyCode = mouseButton == 0 ? KeyCode.Mouse0 : KeyCode.Mouse1;
		if (!commandDispatch[playerID - 1].ContainsKey(keyCode))
		{
			return Empty;
		}
		Command cmd = commandDispatch[playerID - 1][keyCode];
		return new Vector3(cmd.x0, cmd.y0, cmd.z0);
	}

	public static Vector3[] GetMouseDragSelection(int playerID)
	{
		if (!commandDispatch[playerID - 1].ContainsKey(KeyCode.Mouse2))
		{
			return null;
		}
		Command cmd = commandDispatch[playerID - 1][KeyCode.Mouse2];
		Vector3[] selection = new Vector3[2];
		selection[0] = new Vector3(cmd.x0, cmd.y0, cmd.z0);
		selection[1] = new Vector3(cmd.x1, cmd.y1, cmd.z1);
		return selection;
	}
}
