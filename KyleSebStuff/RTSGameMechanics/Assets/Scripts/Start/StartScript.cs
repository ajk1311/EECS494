using UnityEngine;
using System.Collections;
using EventBus;
using SSGameEvents;
using RTS;

public class StartScript : MonoBehaviour {
	private Vector3 cameraStartPosition1 = new Vector3(4.7f, 55.45f, -191.2f);
	private Vector3 cameraStartPosition2 = new Vector3(4.7f, 55.45f,  117.7f);

	public Object redCpuPrefab;
	public Object greenCpuPrefab;

	void Start() {
		Dispatcher.Instance.Register(this);
		SSGameSetup.ConnectToGame("akausejr", true);
	}
	
	[HandlesEvent]
	public void OnGameConnection(GameConnectionEvent connectionEvent) {
		Debug.Log("Game Connected, opponent is " + connectionEvent.opponentName);
		
		GUIModelManager.Init();
		SelectionManager.Init();
		CombinationManager.Init();

		GameObject playerObject = GameObject.Find("Player");
		playerObject.GetComponent<PlayerScript>().id = connectionEvent.ID;
		playerObject.GetComponent<UserInputManager>().playerID = connectionEvent.ID;
		playerObject.GetComponentInChildren<AssemblerScript>().playerID = connectionEvent.ID;

		GameObject opponentObject = GameObject.Find("Opponent");
		opponentObject.GetComponent<UserInputManager>().playerID = connectionEvent.opponentID;
		opponentObject.GetComponentInChildren<AssemblerScript>().playerID = connectionEvent.opponentID;
		
		GameObject redCpu = (GameObject) Instantiate(redCpuPrefab);
		GameObject greenCpu = (GameObject) Instantiate(greenCpuPrefab);

		if (connectionEvent.ID == 1) {
			greenCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			redCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			Camera.main.transform.position = cameraStartPosition1;
		} else {
			greenCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			redCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			Camera.main.transform.position = cameraStartPosition2;
		}

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent) {
		Debug.Log("Game is ready");
	}
}
