using UnityEngine;
using System.Collections;
using EventBus;
using SSGameEvents;

public class StartScript : MonoBehaviour {

	public Object opponentUnit;
	public Object unitPrefab;

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

		GameObject unit = (GameObject)Instantiate (unitPrefab, new Vector3(18, 2.5f, 27), Quaternion.identity);
		unit.GetComponent<WorldObject>().playerID = connectionEvent.ID;


		GameObject opponentObject = GameObject.Find("Opponent");
		opponentObject.GetComponent<UserInputManager>().playerID = connectionEvent.opponentID;

		
		GameObject unit2 = (GameObject)Instantiate (opponentUnit, new Vector3(20, 2.5f, 30), Quaternion.identity);
		unit2.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
