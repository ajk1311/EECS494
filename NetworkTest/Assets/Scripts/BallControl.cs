using UnityEngine;
using System.Collections;

public class BallControl : MonoBehaviour, GameManager.IGameUnit {

	private int playerID;
	public float speed = 2;
	private Vector3 destination = new Vector3(0, 0.05f, 0);

	void Start () {
		transform.position = destination;
		GameManager.RegisterGameUnit (this);
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
        else if (GameCommands.GetKeyDown(playerID, KeyCode.LeftArrow))
        {
            destination = new Vector3(destination.x - 2, destination.y, destination.z);
        }
        else if (GameCommands.GetKeyDown(playerID, KeyCode.UpArrow))
        {
            destination = new Vector3(destination.x, destination.y, destination.z + 2);
        }
        else if (GameCommands.GetKeyDown(playerID, KeyCode.DownArrow))
        {
            destination = new Vector3(destination.x, destination.y, destination.z - 2);
        }
		transform.position = Vector3.Lerp(transform.position, destination, speed * deltaTime);
	}
}
