using UnityEngine;
using System.Collections;
using EventBus;
using SSGameEvents;

public class StartScript : MonoBehaviour {

	public Object greenUnit;
	public Object blueUnit;

	private Vector3 startPosition = new Vector3(18, 2.5f, 27);
	private Vector3 opponentStartPosition = new Vector3(20, 2.5f, 30);

	void Start () {
		Dispatcher.Instance.Register (this);
		SSGameSetup.ConnectToGame("jjhsiung");
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

		GameObject myUnit, myUnit2;
		GameObject opponentUnit, opponentUnit2;
		if (connectionEvent.ID == 1) {
			myUnit = (GameObject)Instantiate(blueUnit, startPosition, Quaternion.identity);
			myUnit2 = (GameObject)Instantiate(blueUnit, startPosition + new Vector3(-1.5f, 0, 0), Quaternion.identity);
			opponentUnit = (GameObject)Instantiate(greenUnit, opponentStartPosition, Quaternion.identity);
			opponentUnit2 = (GameObject)Instantiate(greenUnit, opponentStartPosition + new Vector3(1.5f, 0, 0), Quaternion.identity);
		} else {
			myUnit = (GameObject)Instantiate(blueUnit, opponentStartPosition, Quaternion.identity);
			myUnit2 = (GameObject)Instantiate(blueUnit, opponentStartPosition + new Vector3(1.5f, 0, 0), Quaternion.identity);
			opponentUnit = (GameObject)Instantiate(blueUnit, startPosition, Quaternion.identity);
			opponentUnit2 = (GameObject)Instantiate(blueUnit, startPosition + new Vector3(-1.5f, 0, 0), Quaternion.identity);
		}

		myUnit.GetComponent<WorldObject>().playerID = connectionEvent.ID;
		myUnit2.GetComponent<WorldObject>().playerID = connectionEvent.ID;
		opponentUnit.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
		opponentUnit2.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
