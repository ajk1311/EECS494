using UnityEngine;
using System.Collections.Generic;
using SSProtoBufs;

public static class SSInput
{
	public static readonly Vector3 Empty = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	private static List<Dictionary<int, Command>> commandDispatch;

	public static void Init()
	{
		commandDispatch = new List<Dictionary<int, Command>>(2);
		commandDispatch.Add(new Dictionary<int, Command>());
		commandDispatch.Add(new Dictionary<int, Command>());
	}

	public static void ClearInput()
	{
		foreach (Dictionary<int, Command> map in commandDispatch)
		{
			map.Clear();
		}
	}

	public static void AddInput(int playerID, Queue<Command> commands)
	{
		Dictionary<int, Command> map = commandDispatch[playerID - 1];
		foreach (Command cmd in commands)
		{
			if (!map.ContainsKey(cmd.keyCode))
			{
				map.Add(cmd.keyCode, cmd);
			}
		}
	}

	public static bool GetKeyDown(int playerID, int keyCode)
	{
		return commandDispatch[playerID - 1].ContainsKey(keyCode);
	}

	public static Vector3 GetMouseButtonDown(int playerID, int mouseButton)
	{
		int keyCode = mouseButton == 0 ? SSKeyCode.Mouse0Down : SSKeyCode.Mouse1Down;
		if (!commandDispatch[playerID - 1].ContainsKey(keyCode))
		{
			return Empty;
		}
		Command cmd = commandDispatch[playerID - 1][keyCode];
		return new Vector3(cmd.x, cmd.y, cmd.z);
	}

    public static Vector3 GetMouseButtonUp(int playerID, int mouseButton)
    {
        int keyCode = mouseButton == 0 ? SSKeyCode.Mouse0Up : SSKeyCode.Mouse1Up;
        if (!commandDispatch[playerID - 1].ContainsKey(keyCode))
        {
            return Empty;
        }
        Command cmd = commandDispatch[playerID - 1][keyCode];
        return new Vector3(cmd.x, cmd.y, cmd.z);
    }
}
