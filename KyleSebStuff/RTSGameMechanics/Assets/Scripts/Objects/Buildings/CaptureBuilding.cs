using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CaptureBuilding : Building {

	public int timeToCapture;
	public int currentTime;

	public int player1UnitCount;
	public int player2UnitCount;

	public bool player1Holding;
	public bool player1AlreadyHolding;
	public bool player2Holding;
	public bool player2AlreadyHolding;
	public bool player1OwnsTower;
	public bool player2OwnsTower;

	public int buff;

	protected override void Start() {
		base.Start();
		player1UnitCount = 0;
		player2UnitCount = 0;

		player1Holding = false;
		player1AlreadyHolding = false;
		player2Holding = false;
		player2AlreadyHolding = false;
		player1OwnsTower = false;
		player2OwnsTower = false;

		playerID = 0;
	}

	protected override RTS.GUIModelManager.GUIModel GetGUIModel() {
		return null;
	}

	public override void GameUpdate (float deltaTime)
	{
		base.GameUpdate (deltaTime);

		//Player1 Is in Control
		if(player1UnitCount > 0 && player2UnitCount == 0) {
			//Player1 is about to start claiming
			if(!player1AlreadyHolding) {
				player1AlreadyHolding = true;
				player1Holding = true;
				player2AlreadyHolding = false;
				player2Holding = false;
				player2OwnsTower = false;
				currentTime = 0;
			}
			//Player1 already has claimed
			else if(player1Holding){
				currentTime += (int) Mathf.Floor(deltaTime);
				if(currentTime >= timeToCapture) {
					player1OwnsTower = true;
				}
			}
		}

		//Player2 Is in Control
		if(player2UnitCount > 0 && player1UnitCount == 0) {
			//Player2 is about to start claiming
			if(!player2AlreadyHolding) {
				player2AlreadyHolding = true;
				player2Holding = true;
				player1AlreadyHolding = false;
				player1Holding = false;
				player1OwnsTower = false;
				currentTime = 0;
			}
			//Player2 has already claimed
			else if(player1Holding){
				currentTime += (int) Mathf.Floor(deltaTime);
				if(currentTime >= timeToCapture) {
					player2OwnsTower = true;
				}
			}
		}

		//Player1 Has Control of the Tower
		if(player1OwnsTower) {
			Debug.Log("---------Player 1 Owns the Tower-----------");
			setBuffForPlayer(1);
		}

		//Player1 Has Control of the Tower
		if(player2OwnsTower) {
			Debug.Log("---------Player 2 Owns the Tower-----------");
			setBuffForPlayer(2);
		}
	}

	private void getCurrentUnitCounts() {
		player1UnitCount = 0;
		player2UnitCount = 0;

		List<WorldObject> surroundingUnits = new List<WorldObject> ();
		surroundingUnits = GridManager.GetObjectsInRadius (this, 7);

		foreach(WorldObject obj in surroundingUnits) {
			int currentID = obj.playerID;
			if(playerID == 1) {
				player1UnitCount++;
			}
			else if(playerID == 2) {
				player2UnitCount++;
			}
		}
	}

	private void setBuffForPlayer(int playerID) {
		if(buff == 0) {
			
		}
		else if(buff == 1) {
			
		}
		else if(buff == 2) {
			
		}
	}
}
