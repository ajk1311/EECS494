using UnityEngine;
using System.Collections;
using EventBus;
using SSGameEvents;

public class _SebastianStartSCript : MonoBehaviour {
	public Object greenUnit;
	public Object blueUnit;
	
	private Vector3 startPosition = new Vector3(18, 2.5f, 27);
	private Vector3 opponentStartPosition = new Vector3(20, 2.5f, 30);


	
	void Start () {
		Dispatcher.Instance.Register (this);
		SSGameSetup.ConnectToGame("akausejr", true);
	}
	
	[HandlesEvent]
	public void OnGameConnection(GameConnectionEvent connectionEvent)
	{
		Debug.Log("Game Connected, opponent is " + connectionEvent.opponentName);
		
		SelectionManager.Init();
		CombinationManager.Init ();
		
		GameObject playerObject = GameObject.Find("Player");
		playerObject.GetComponent<PlayerScript>().id = connectionEvent.ID;
		playerObject.GetComponent<UserInputManager>().playerID = connectionEvent.ID;
		
		GameObject opponentObject = GameObject.Find("Opponent");
		opponentObject.GetComponent<UserInputManager>().playerID = connectionEvent.opponentID;

		GameObject assemblerObject = GameObject.Find("Assembler");;
		assemblerObject.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
		
		GameObject myUnit, myUnit2, myUnit3;
		GameObject opponentUnit, opponentUnit2, opponentUnit3;
		if (connectionEvent.ID == 1) {
			myUnit = (GameObject)Instantiate(blueUnit, startPosition, Quaternion.identity);
			myUnit2 = (GameObject)Instantiate(blueUnit, startPosition + new Vector3(-1.5f, 0, 0), Quaternion.identity);
			myUnit3 = (GameObject)Instantiate(blueUnit, startPosition + new Vector3(-2.5f, 0, 0), Quaternion.identity);

			myUnit.GetComponent<WorldObject>().objectName = "Blue";
			myUnit2.GetComponent<WorldObject>().objectName = "Blue";
			myUnit3.GetComponent<WorldObject>().objectName = "Blue";


			opponentUnit = (GameObject)Instantiate(greenUnit, opponentStartPosition, Quaternion.identity);
			opponentUnit2 = (GameObject)Instantiate(greenUnit, opponentStartPosition + new Vector3(1.5f, 0, 0), Quaternion.identity);
			opponentUnit3 = (GameObject)Instantiate(greenUnit, opponentStartPosition + new Vector3(2.5f, 0, 0), Quaternion.identity);

			opponentUnit.GetComponent<WorldObject>().objectName = "Green";
			opponentUnit2.GetComponent<WorldObject>().objectName = "Green";
			opponentUnit3.GetComponent<WorldObject>().objectName = "Green";
		} else {
			myUnit = (GameObject)Instantiate(greenUnit, opponentStartPosition, Quaternion.identity);
			myUnit2 = (GameObject)Instantiate(greenUnit, opponentStartPosition + new Vector3(1.5f, 0, 0), Quaternion.identity);
			myUnit3 = (GameObject)Instantiate(greenUnit, opponentStartPosition + new Vector3(2.5f, 0, 0), Quaternion.identity);

			myUnit.GetComponent<WorldObject>().objectName = "Green";
			myUnit2.GetComponent<WorldObject>().objectName = "Green";
			myUnit3.GetComponent<WorldObject>().objectName = "Green";

			opponentUnit = (GameObject)Instantiate(blueUnit, startPosition, Quaternion.identity);
			opponentUnit2 = (GameObject)Instantiate(blueUnit, startPosition + new Vector3(-1.5f, 0, 0), Quaternion.identity);
			opponentUnit3 = (GameObject)Instantiate(blueUnit, startPosition + new Vector3(-2.5f, 0, 0), Quaternion.identity);

			opponentUnit.GetComponent<WorldObject>().objectName = "Blue";
			opponentUnit2.GetComponent<WorldObject>().objectName = "Blue";
			opponentUnit3.GetComponent<WorldObject>().objectName = "Blue";
		}
		
		myUnit.GetComponent<WorldObject>().playerID = connectionEvent.ID;
		myUnit2.GetComponent<WorldObject>().playerID = connectionEvent.ID;
		myUnit3.GetComponent<WorldObject> ().playerID = connectionEvent.ID;

		opponentUnit.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
		opponentUnit2.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
		opponentUnit3.GetComponent<WorldObject> ().playerID = connectionEvent.opponentID;
		
		SSGameSetup.Ready(connectionEvent.ID);
	}

	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
