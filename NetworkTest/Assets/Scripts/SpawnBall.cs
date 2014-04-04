using UnityEngine;
using System.Collections;

public class SpawnBall : MonoBehaviour, SSGameManager.IGameUnit {

	public Object prefab1;
	public Object prefab2;

	private int	playerID;

	public void Start()
	{
		SSGameManager.RegisterGameUnit(this);
	}

	public void SetPlayerID(int playerID)
	{
		this.playerID = playerID;
	}

	public void GameUpdate(float deltaTime)
	{
		//Debug.Log ("game update inside spawn point!");
		if(SSInput.GetKeyDown(playerID, SSKeyCode.Space))
		{
           // Debug.Log("Got space bar!");
			GameObject spawned = (GameObject) Instantiate(playerID == 1 ? prefab1 : prefab2);
			spawned.GetComponent<BallControl>().SetPlayerID(playerID);
		}
	}
}
