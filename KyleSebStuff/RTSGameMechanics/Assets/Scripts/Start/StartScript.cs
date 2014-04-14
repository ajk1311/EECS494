﻿using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventBus;
using SSGameEvents;
using RTS;
using Parse;

public class StartScript : MonoBehaviour {
	private Vector3 cameraStartPosition1 = new Vector3(200, 55, 35);
	private Vector3 cameraStartPosition2 = new Vector3(200, 55, 330);

	private Vector3 assembler1Pos = new Vector3 (200, 0, 45);
	private Vector3 assembler2Pos = new Vector3 (200, 0, 355);

	public Object redCpuPrefab;
	public Object greenCpuPrefab;
	public Object assembler;

	void Start() {
		Dispatcher.Instance.Register(this);
		SSGameSetup.ConnectToGame("akausejr", false);
	}
	
	[HandlesEvent]
	public void OnGameConnection(GameConnectionEvent connectionEvent) {
		Debug.Log("Game Connected, opponent is " + connectionEvent.opponentName);

		Random.seed = connectionEvent.randomSeed;

		GridManager.Init();
		GUIModelManager.Init();
		SelectionManager.Init();
		CombinationManager.Init();
		FogOfWarManager.Init();

		UserInputManager myInputManager;
		UserInputManager hisOrHerInputManager;

		GameObject playerObject = GameObject.Find("Player");
		playerObject.GetComponent<PlayerScript>().id = connectionEvent.ID;
		myInputManager = (UserInputManager) playerObject.GetComponent<UserInputManager>();
		myInputManager.playerID = connectionEvent.ID;
		FogOfWarManager.playerID = connectionEvent.ID;


		GameObject opponentObject = GameObject.Find("Opponent");
		hisOrHerInputManager = opponentObject.GetComponent<UserInputManager>();
		hisOrHerInputManager.playerID = connectionEvent.opponentID;

		GameObject redCpu = (GameObject) Instantiate(redCpuPrefab);
		GameObject greenCpu = (GameObject) Instantiate(greenCpuPrefab);

		GameObject assembler1 = (GameObject) Instantiate(assembler, assembler1Pos, Quaternion.identity);
		GameObject assembler2 = (GameObject)Instantiate (assembler, assembler2Pos, Quaternion.identity);
		int count = 0;
		if (connectionEvent.ID == 1) {
			var query = ParseObject.GetQuery("GameID");
			query.CountAsync().ContinueWith(t =>
			{
				count = t.Result;
				ParseObject gameID = new ParseObject("GameID");
				gameID["ID"] = count++;
				gameID.SaveAsync();
			});
			ParseManager.Init(connectionEvent.ID, count);
			greenCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			redCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			Camera.main.transform.position = cameraStartPosition1;

			assembler1.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
			assembler2.GetComponent<AssemblerScript>().playerID = connectionEvent.opponentID;

			SSGameManager.Register(myInputManager);
			SSGameManager.Register(hisOrHerInputManager);
		} else {
			var query = ParseObject.GetQuery("GameID");
			query.CountAsync().ContinueWith(t =>
			{
				count = t.Result;
			});
			ParseManager.Init(connectionEvent.ID, count + 1);
			greenCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			redCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			Camera.main.transform.position = cameraStartPosition2;

			assembler2.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
			assembler1.GetComponent<AssemblerScript>().playerID = connectionEvent.opponentID;

			SSGameManager.Register(hisOrHerInputManager);
			SSGameManager.Register(myInputManager);
		}

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent) {
		Debug.Log("Game is ready");
	}
}
