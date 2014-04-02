﻿using UnityEngine;
using System.Collections;
using GameEvents;
using EventBus;
using System.Threading;

public class ConnectionTest : MonoBehaviour {

	public Object spawnPrefab;

	void Start () 
	{
		Dispatcher.Instance.Register(this);
		GameSetup.ConnectToGame("jjhsiung");
	}

	[HandlesEvent]
	public void OnGameConnection(GameConnectionEvent connectionEvent)
	{
		Debug.Log("Game Connected, opponent is " + connectionEvent.opponentName);

		GameObject playerSpawnPoint = (GameObject) Instantiate(spawnPrefab);
		playerSpawnPoint.name = connectionEvent.name + "_spawnPoint";
		playerSpawnPoint.GetComponent<SpawnBall>().SetPlayerID(connectionEvent.ID);

		GameObject opponentSpawnPoint = (GameObject) Instantiate(spawnPrefab);
		opponentSpawnPoint.name = connectionEvent.opponentName + "_spawnPoint";
		opponentSpawnPoint.GetComponent<SpawnBall>().SetPlayerID(connectionEvent.opponentID);

		GameSetup.Ready(connectionEvent.ID);
	}

	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
