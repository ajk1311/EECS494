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
	private GUIModelManager.GUIModel mUnitCreationModel;

	public Vector3 spawnOffset = new Vector3(0, 0, 5);

	protected override void Start() {
		base.Start();
		BuildUnitCreationModel();
	}

	protected override RTS.GUIModelManager.GUIModel GetGUIModel() {
		// TODO different ones based on different states
		return mUnitCreationModel;
	}

	void BuildUnitCreationModel() {
		mUnitCreationModel = new GUIModelManager.GUIModel();

		GUIModelManager.Button cubeButton = new GUIModelManager.Button();
		cubeButton.icon = unit1Icon;
		cubeButton.clicked += new GUIModelManager.OnClick(ProduceCube);

		GUIModelManager.Button sphereButton = new GUIModelManager.Button();
		sphereButton.icon = unit2Icon;
		sphereButton.clicked += new GUIModelManager.OnClick(ProduceSphere);

		mUnitCreationModel.AddButton(0, cubeButton);
		mUnitCreationModel.AddButton(0, sphereButton);
	}

	void ProduceCube() {
		GameObject cube = (GameObject) Instantiate(cubePrefab, transform.position + spawnOffset, Quaternion.identity);
		cube.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceSphere() {
		GameObject sphere = (GameObject) Instantiate(spherePrefab, transform.position + spawnOffset, Quaternion.identity);
		sphere.GetComponent<WorldObject>().playerID = PlayerID;
	}
}
