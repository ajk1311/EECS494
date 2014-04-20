﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Pathfinding;

public class CPU : DestructableBuilding {

	public Texture unit1Icon;
	public Texture unit2Icon;
	public Texture unit3Icon;

	// TODO change for production
	public Object cubePrefab;
	public Object spherePrefab;
	public Object capsulePrefab;
	public Object tier2BinaryTree;
	public Object tier2Heap;
	public Object tier2Static;

	// GUI models
	private GUIModelManager.GUIModel mTierSelectionModel;
	private GUIModelManager.GUIModel mTier1UnitCreationModel;

	//Progress Bar
	private GUIProgressBar progressBar;

	public int spawnOffsetX;
	public Int3 spawnOffset;

	protected override void Start() {
		base.Start();
		BuildTierSelectionModel();
		BuildTier1UnitCreationModel();
		spawnOffset = new Int3(spawnOffsetX * Int3.Precision, 0, 0);
		progressBar = (GUIProgressBar) gameObject.AddComponent("GUIProgressBar");
	}

	protected override GUIModelManager.GUIModel GetGUIModel(){ return null; }

	public override void OnSelectionChanged(bool selected) {
		GUIModelManager.SetCurrentModel(playerID, selected ? mTierSelectionModel : null);
	}

	void BuildTierSelectionModel() {
		mTierSelectionModel = new GUIModelManager.GUIModel();
		mTierSelectionModel.leftPanelColumns = 1;

		GUIModelManager.Button tier1 = new GUIModelManager.Button();
		tier1.text = "Tier 1";
		tier1.clicked += new GUIModelManager.OnClick(Tier1Clicked);

		GUIModelManager.Button tier2 = new GUIModelManager.Button();
		tier2.text = "Tier 2";
		tier2.clicked += new GUIModelManager.OnClick(Tier2Clicked);

		GUIModelManager.Button tier3 = new GUIModelManager.Button();
		tier3.text = "Tier 3";
		tier3.clicked += new GUIModelManager.OnClick(Tier3Clicked);

		mTierSelectionModel.AddButton(0, tier1);
		mTierSelectionModel.AddButton(0, tier2);
		mTierSelectionModel.AddButton(0, tier3);

		AddDefaultButtons(mTierSelectionModel);
	}

	void Tier1Clicked() {
		// TODO check if unlocked
        Debug.Log("Tier 1 clicked");
		GUIModelManager.SetCurrentModel(playerID, mTier1UnitCreationModel);
	}

	void Tier2Clicked() {
		// TODO check if unlocked
		
	}

	void Tier3Clicked() {
		// TODO check if unlocked
		
	}

	void BuildTier1UnitCreationModel() {
		mTier1UnitCreationModel = new GUIModelManager.GUIModel();

		GUIModelManager.Button cubeButton = new GUIModelManager.Button();
		cubeButton.icon = unit1Icon;
		cubeButton.clicked += new GUIModelManager.OnClick(ProduceCube);

		GUIModelManager.Button sphereButton = new GUIModelManager.Button();
		sphereButton.icon = unit2Icon;
		sphereButton.clicked += new GUIModelManager.OnClick(ProduceSphere);

		GUIModelManager.Button capsuleButton = new GUIModelManager.Button();
		// TODO icon
		capsuleButton.clicked += new GUIModelManager.OnClick(ProduceCapsule);

		mTier1UnitCreationModel.AddButton(0, cubeButton);
		mTier1UnitCreationModel.AddButton(0, sphereButton);
		mTier1UnitCreationModel.AddButton(0, capsuleButton);

		AddDefaultButtons(mTier1UnitCreationModel);
	}

	private void AddDefaultButtons(GUIModelManager.GUIModel model) {
		model.centerPanelColumns = 1;
		
		GUIModelManager.Button random = new GUIModelManager.Button();
		random.text = "Randomize";
		random.clicked += new GUIModelManager.OnClick(ProduceRandomUnit);
		model.AddButton(1, random);

		GUIModelManager.Button upgrade = new GUIModelManager.Button();
		upgrade.text = "Upgrade";
		upgrade.clicked += new GUIModelManager.OnClick(UpgradeToNextTier);
		model.AddButton(1, upgrade);
	}

	void Update() {
		progressBar.progress++;
		if(progressBar.progress >= progressBar.progressFull)
			progressBar.finishProgressBar();
	}

	void ProduceCube() {
		// Progress Bar just for show right now
		progressBar.startProgressBar(0);
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID+1, "Cube", "CPU");
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
		GameObject cube = (GameObject) Instantiate(cubePrefab, (Vector3) spawnPosition, Quaternion.identity);
		cube.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceSphere() {
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Sphere", "CPU");
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
		GameObject sphere = (GameObject) Instantiate(spherePrefab, (Vector3) spawnPosition, Quaternion.identity);
		sphere.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceCapsule() {
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Capsule", "CPU");
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
		GameObject capsule = (GameObject) Instantiate(capsulePrefab, (Vector3) spawnPosition, Quaternion.identity);
		capsule.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceRandomUnit() {
		int unit = Bellagio.gambleUnit(playerID);
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
				
		switch(unit) {
			case 0: 
				ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Sphere", "CPU");
				GameObject sphere = (GameObject) Instantiate(spherePrefab, (Vector3) spawnPosition, Quaternion.identity);
				sphere.GetComponent<WorldObject>().playerID = PlayerID;
				break;
			case 1:
				ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Cube", "CPU");
				GameObject cube = (GameObject) Instantiate(cubePrefab, (Vector3) spawnPosition, Quaternion.identity);
				cube.GetComponent<WorldObject>().playerID = PlayerID;
				break;
			case 2:
				ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Capsule", "CPU");
				GameObject capsule = (GameObject) Instantiate(capsulePrefab, (Vector3) spawnPosition, Quaternion.identity);
				capsule.GetComponent<WorldObject>().playerID = PlayerID;
				break;
			case 3:
				ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "tier2BinaryTree", "CPU");
				GameObject binaryTree = (GameObject) Instantiate(tier2BinaryTree, (Vector3) spawnPosition, Quaternion.identity);
				binaryTree.GetComponent<WorldObject>().playerID = PlayerID;
				break;
			case 4:
				ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "tier2Heap", "CPU");
				GameObject heap = (GameObject) Instantiate(tier2Heap, (Vector3) spawnPosition, Quaternion.identity);
				heap.GetComponent<WorldObject>().playerID = PlayerID;
				break;
			case 5:
				ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "tier2Static", "CPU");
				GameObject staticPrefab = (GameObject) Instantiate(tier2Static, (Vector3) spawnPosition, Quaternion.identity);
				staticPrefab.GetComponent<WorldObject>().playerID = PlayerID;
				break;
			default:
				Debug.Log("got a tier 3!");
				break;
		}
	}

	void UpgradeToNextTier() {
		var script1 = GameObject.Find("Player").GetComponent<PlayerScript>();
		var script2 = GameObject.Find("Opponent").GetComponent<PlayerScript>();
		if(playerID == script1.id) {
			script1.upgradeTier();
		}
		else {
			script2.upgradeTier();
		}
	}
}
