using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

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


	public Vector3 spawnOffset = new Vector3(0, 0, 5);

	protected override void Start() {
		base.Start();
		BuildTierSelectionModel();
		BuildTier1UnitCreationModel();
		mCurrentModel = mTierSelectionModel;
	}

	protected override RTS.GUIModelManager.GUIModel GetGUIModel() {
		return mCurrentModel;
	}

	void BuildTierSelectionModel() {
		mTierSelectionModel = new GUIModelManager.GUIModel();

		GUIModelManager.Button tier1 = new GUIModelManager.Button();
		tier1.text = "Tier 1";
		tier1.clicked += new GUIModelManager.OnClick(Tier1Clicked);

		GUIModelManager.Button tier2 = new GUIModelManager.Button();
		tier2.text = "Tier 2";
		tier2.clicked += new GUIModelManager.OnClick(Tier2Clicked);

		GUIModelManager.Button tier3 = new GUIModelManager.Button();
		tier3.text = "Tier 3";
		tier3.clicked += new GUIModelManager.OnClick(Tier3Clicked);

		mTierSelectionModel.AddButton(tier1);
		mTierSelectionModel.AddButton(tier2);
		mTierSelectionModel.AddButton(tier3);
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
	}

	void ProduceCube() {
		// TODO check resources
		GameObject cube = (GameObject) Instantiate(cubePrefab, transform.position + spawnOffset, Quaternion.identity);
		cube.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceSphere() {
		// TODO check resources
		GameObject sphere = (GameObject) Instantiate(spherePrefab, transform.position + spawnOffset, Quaternion.identity);
		sphere.GetComponent<WorldObject>().playerID = PlayerID;
	}
}
