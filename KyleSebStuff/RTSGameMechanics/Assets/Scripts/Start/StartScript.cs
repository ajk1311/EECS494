using UnityEngine;
using System.Collections;
using EventBus;
using SSGameEvents;

public class StartScript : MonoBehaviour {
	private Vector3 cameraStartPosition1 = new Vector3(0, 0, 0);
	private Vector3 cameraStartPosition2 = new Vector3(0, 0, 0);

	void Start () {
		Dispatcher.Instance.Register (this);
		SSGameSetup.ConnectToGame("akausejr", true);
	}
	
	[HandlesEvent]
	public void OnGameConnection(GameConnectionEvent connectionEvent)
	{
		Debug.Log("Game Connected, opponent is " + connectionEvent.opponentName);

		SelectionManager.Init();

		GameObject playerObject = GameObject.Find("Player");
		playerObject.GetComponent<PlayerScript>().id = connectionEvent.ID;
		playerObject.GetComponent<UserInputManager>().playerID = connectionEvent.ID;

		GameObject opponentObject = GameObject.Find("Opponent");
		opponentObject.GetComponent<UserInputManager>().playerID = connectionEvent.opponentID;

		if (connectionEvent.ID == 1) {
			Camera.main.transform.position = cameraStartPosition1;
		} 
		else {
			Camera.main.transform.position = cameraStartPosition2;
		}

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
