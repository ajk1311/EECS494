using UnityEngine;
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

	// GUI models
	private GUIModelManager.GUIModel mCurrentModel;
	private GUIModelManager.GUIModel mTierSelectionModel;
	private GUIModelManager.GUIModel mTier1UnitCreationModel;

	public int spawnOffsetZ;
	public Int3 spawnOffset = new Int3(0, 0, 10 * Int3.Precision);

	protected override void Start() {
		base.Start();
		BuildTierSelectionModel();
		BuildTier1UnitCreationModel();
		mCurrentModel = mTierSelectionModel;
	}

	protected override void OnUnselected() {
		mCurrentModel = mTierSelectionModel;
	}

	protected override GUIModelManager.GUIModel GetGUIModel() {
		return mCurrentModel;
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

		AddRandomButton(mTierSelectionModel);
	}

	void Tier1Clicked() {
		// TODO check if unlocked
		mCurrentModel = mTier1UnitCreationModel;
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

		mTier1UnitCreationModel.AddButton(0, cubeButton);
		mTier1UnitCreationModel.AddButton(0, sphereButton);

		AddRandomButton(mTier1UnitCreationModel);
	}

	private void AddRandomButton(GUIModelManager.GUIModel model) {
		model.centerPanelColumns = 1;
		
		GUIModelManager.Button random = new GUIModelManager.Button();
		random.text = "Randomize";
		random.clicked += new GUIModelManager.OnClick(ProduceRandomUnit);
		
		model.AddButton(1, random);
	}

	void ProduceCube() {
		// TODO check resources
		Int3 spawnPosition = intPosition + spawnOffset;
		GameObject cube = (GameObject) Instantiate(cubePrefab, (Vector3) spawnPosition, Quaternion.identity);
		cube.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceSphere() {
		// TODO check resources
		Int3 spawnPosition = intPosition + spawnOffset;
		GameObject sphere = (GameObject) Instantiate(spherePrefab, (Vector3) spawnPosition, Quaternion.identity);
		sphere.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceRandomUnit() {
		// TODO random unit
	}
}
