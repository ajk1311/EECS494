using UnityEngine;
using System.Collections;

public class SpawnBall : MonoBehaviour, GameManager.IGameUnit {

	public Object prefab1;
	public Object prefab2;

	private int	playerID;

	public void SetPlayerID(int playerID)
	{
		this.playerID = playerID;
	}

	public void GameUpdate(float deltaTime)
	{
		if(GameCommands.GetKeyDown(playerID, KeyCode.Space))
		{
			GameObject spawned = (GameObject) Instantiate(playerID == 1 ? prefab1 : prefab2);
			spawned.GetComponent<BallControl>().SetPlayerID(playerID);
		}
	}
}
