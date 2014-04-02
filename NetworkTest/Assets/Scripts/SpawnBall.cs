using UnityEngine;
using System.Collections;

public class SpawnBall : MonoBehaviour, GameManager.IGameUnit {

	public Object prefab1;
	public Object prefab2;

	private int	playerID;

	public void Start()
	{
		GameManager.RegisterGameUnit(this);
	}

	public void SetPlayerID(int playerID)
	{
		this.playerID = playerID;
	}

	public void GameUpdate(float deltaTime)
	{
		Debug.Log ("game update inside spawn point!");
		if(GameCommands.GetKeyDown(playerID, KeyCode.Space))
		{
			GameObject spawned = (GameObject) Instantiate(playerID == 1 ? prefab1 : prefab2);
			spawned.GetComponent<BallControl>().SetPlayerID(playerID);
		}
	}
}
