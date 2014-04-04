using UnityEngine;
using System.Collections;

public class BallControl : MonoBehaviour, SSGameManager.IGameUnit {

	private int playerID;
	public float speed = 2;
	private Vector3 destination = new Vector3(0, 0.05f, 0);

	void Start () {
		transform.position = destination;
		SSGameManager.RegisterGameUnit (this);
	}

	public void SetPlayerID(int playerID)
	{
		this.playerID = playerID;
	}

	public void GameUpdate(float deltaTime)
	{
		//poll available commands for current tick
		if (SSInput.GetKeyDown(playerID, SSKeyCode.RightArrow))
		{
			destination = new Vector3(destination.x + 2, destination.y, destination.z);
		}
        else if (SSInput.GetKeyDown(playerID, SSKeyCode.LeftArrow))
        {
            destination = new Vector3(destination.x - 2, destination.y, destination.z);
        }
        else if (SSInput.GetKeyDown(playerID, SSKeyCode.UpArrow))
        {
            destination = new Vector3(destination.x, destination.y, destination.z + 2);
        }
        else if (SSInput.GetKeyDown(playerID, SSKeyCode.DownArrow))
        {
            destination = new Vector3(destination.x, destination.y, destination.z - 2);
        }
		transform.position = Vector3.Lerp(transform.position, destination, speed * deltaTime);
	}
}
