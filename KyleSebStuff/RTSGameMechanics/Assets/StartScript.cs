using UnityEngine;
using System.Collections;
using EventBus;
using SSGameEvents;

public class StartScript : MonoBehaviour {

	public Object greenUnit;
	public Object blueUnit;

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

		GameObject myUnit;
		GameObject opponentUnit;
		if (connectionEvent.ID == 1) {
			myUnit = (GameObject)Instantiate(blueUnit, new Vector3(18, 2.5f, 27), Quaternion.identity);
			opponentUnit = (GameObject)Instantiate (greenUnit, new Vector3(20, 2.5f, 30), Quaternion.identity);
		} else {
			opponentUnit = (GameObject)Instantiate(blueUnit, new Vector3(18, 2.5f, 27), Quaternion.identity);
			myUnit = (GameObject)Instantiate (greenUnit, new Vector3(20, 2.5f, 30), Quaternion.identity);
		}

		myUnit.GetComponent<WorldObject>().playerID = connectionEvent.ID;
		opponentUnit.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
