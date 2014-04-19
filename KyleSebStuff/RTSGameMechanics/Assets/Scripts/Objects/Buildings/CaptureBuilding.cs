using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

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
		
		timeToCapture = 6;
		
		playerID = 0;
		
		Camera.main.ScreenPointToRay (Vector2.zero);
	}
	
	protected override RTS.GUIModelManager.GUIModel GetGUIModel() {
		return null;
	}
	
	public override void GameUpdate (float deltaTime)
	{
		base.GameUpdate (deltaTime);
		objectRenderer.enabled = true;
		
		getCurrentUnitCounts ();
		
		//neither Player1 or Player2 owns the tower so reset all variables except ID
		if(player1UnitCount == 0 && player2UnitCount == 0) {
			player1UnitCount = 0;
			player2UnitCount = 0;
			
			player1Holding = false;
			player1AlreadyHolding = false;
			player2Holding = false;
			player2AlreadyHolding = false;
			player1OwnsTower = false;
			player2OwnsTower = false;
		}
		
		//Player1 Is in Control
		if(player1UnitCount > 0 && player2UnitCount == 0) {
			//Player1 is about to start claiming
			if(!player1AlreadyHolding) {
				Debug.Log ("------------Player1 has started to claim tower-----------");
				player1AlreadyHolding = true;
				player1Holding = true;
				player2AlreadyHolding = false;
				player2Holding = false;
				player2OwnsTower = false;
				currentTime = 0;
			}
			//Player1 already has claimed
			else if(player1Holding){
				Debug.Log ("------------Player1 has Already claimed tower-----------");
				currentTime += (int) System.Math.Round(deltaTime * Int3.FloatPrecision);
				if(currentTime >= (int) System.Math.Round(timeToCapture * Int3.FloatPrecision)) {
					player1OwnsTower = true;
				}
			}
		}
		
		//Player2 Is in Control
		if(player2UnitCount > 0 && player1UnitCount == 0) {
			//Player2 is about to start claiming
			if(!player2AlreadyHolding) {
				Debug.Log ("------------Player2 has started to claim tower-----------");
				player2AlreadyHolding = true;
				player2Holding = true;
				player1AlreadyHolding = false;
				player1Holding = false;
				player1OwnsTower = false;
				currentTime = 0;
			}
			//Player2 has already claimed
			else if(player2Holding){
				Debug.Log ("-----------Player2 has already claimed-----------");
				currentTime += (int) System.Math.Round(deltaTime * Int3.FloatPrecision);;
				if(currentTime >= (int) System.Math.Round(timeToCapture * Int3.FloatPrecision)) {
					player2OwnsTower = true;
				}
			}
		}
		
		//Player1 Has Control of the Tower
		if(player1OwnsTower) {
			Debug.Log("---------Player 1 Owns the Tower-----------");
			
			//player 2 last owned tower
			if(playerID == 2) {
				FogOfWarManager.updateFogTileUnitCount (currentFogTile, null, 2);
				removeBuffForPlayer(2);
			}
			
			//switch to new ID, reset Fog, add buff to new player, siwtch color
			setBuffForPlayer(1);
			playerID = 1;
			FogOfWarManager.updateFogTileUnitCount (null, currentFogTile, 1);
			objectRenderer.material.SetColor("_Color", new Color(255f, 140f, 0f, 144f));
		}
		
		//Player1 Has Control of the Tower
		if(player2OwnsTower) {
			Debug.Log("---------Player 2 Owns the Tower-----------");
			
			//player1 last owned tower
			if(playerID == 1) {
				FogOfWarManager.updateFogTileUnitCount (currentFogTile, null, 1);
				removeBuffForPlayer(1);
			}
			
			//switch to new ID, reset Fog to New iD, add buff to new player, switch color
			playerID = 2;
			setBuffForPlayer(2);
			FogOfWarManager.updateFogTileUnitCount (null, currentFogTile, 2);
			objectRenderer.material.SetColor("_Color", new Color(226f, 94f, 255f, 255f));
		}
	}
	
	private void getCurrentUnitCounts() {
		player1UnitCount = 0;
		player2UnitCount = 0;
		
		List<WorldObject> surroundingUnits = new List<WorldObject> ();
		surroundingUnits = GridManager.GetObjectsInRadius (this, 23);
		
		foreach(WorldObject obj in surroundingUnits) {
			int currentID = obj.playerID;
			if(currentID == 1) {
				player1UnitCount++;
			}
			else if(currentID == 2) {
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
	
	private void removeBuffForPlayer(int playerID) {
		if(buff == 0) {
			
		}
		else if(buff == 1) {
			
		}
		else if(buff == 2) {
			
		}
	}
}
