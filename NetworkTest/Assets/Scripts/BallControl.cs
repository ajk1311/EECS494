using UnityEngine;
using System.Collections;

public class BallControl : MonoBehaviour, GameManager.IGameUnit {

	private int playerID;
	public float speed = 2;
	private Vector3 destination = new Vector3(0, 0.05f, 0);

	void Start () {
		transform.position = destination;
	}

	public void SetPlayerID(int playerID)
	{
		this.playerID = playerID;
	}

	public void GameUpdate(float deltaTime)
	{
		//poll available commands for current tick
		if (GameCommands.GetKeyDown(playerID, KeyCode.RightArrow))
		{
			destination = new Vector3(destination.x + 2, destination.y, destination.z);
		}
		transform.position = Vector3.Lerp(transform.position, destination, speed * deltaTime);
	}
}
