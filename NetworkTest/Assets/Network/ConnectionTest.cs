using UnityEngine;
using System.Collections;
using GameEvents;
using EventBus;
using System.Threading;

public class ConnectionTest : MonoBehaviour {

	void Start () 
	{
		Dispatcher.Instance.Register(this);
		GameSetup.ConnectToGame("player1");
	}

	void OnDestroy()
	{
		Dispatcher.Instance.Unregister(this);
	}

	[HandlesEvent]
	public void OnGameConnection(GameConnectionEvent connectionEvent)
	{
		Debug.Log("Game Connected, opponent is " + connectionEvent.opponentName);
		UnityThreading.Thread.InBackground(() =>
		{
			Thread.Sleep(5000);
			UnityThreading.Thread.InForeground(() =>
			{
				GameSetup.Ready(connectionEvent.ID);
			});
		});
	}

	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent)
	{
		Debug.Log("Game is ready");
	}
}
