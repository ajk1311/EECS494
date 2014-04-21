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
	public Object capsulePrefab;
	public Object tier2BinaryTree;
	public Object tier2Static;
	public Object tier2Heap;

	// GUI models
	private GUIModelManager.GUIModel mTierSelectionModel;
	private GUIModelManager.GUIModel mTier1UnitCreationModel;
	private GUIModelManager.GUIModel mTier2UnitCreationModel;
	private GUIModelManager.GUIModel mTier3UnitCreationModel;

	public int spawnOffsetX;
	public Int3 spawnOffset;

	protected override void Start() {
		base.Start();
		BuildTierSelectionModel();
		BuildTier1UnitCreationModel();
		BuildTier2UnitCreationModel();
		spawnOffset = new Int3(spawnOffsetX * Int3.Precision, 0, 0);
	}
	
	protected override GUIModelManager.GUIModel GetGUIModel() { return null; }

	public override void OnSelectionChanged(bool selected) {
		GUIModelManager.SetCurrentModel(playerID, selected ? mTierSelectionModel : null);
	}

	void BuildTierSelectionModel() {
		PlayerScript po = GameObject.Find("Player").GetComponent<PlayerScript>();
		PlayerScript oo = GameObject.Find("Opponent").GetComponent<PlayerScript>();
		PlayerScript player = po.id == playerID ? po : oo;

		mTierSelectionModel = new GUIModelManager.GUIModel();
		mTierSelectionModel.leftPanelColumns = 1;

		GUIModelManager.Button tier1 = new GUIModelManager.Button();
		tier1.text = "Tier 1";
		tier1.clicked += new GUIModelManager.OnClick(Tier1Clicked);

		GUIModelManager.Button tier2 = new GUIModelManager.Button();
		tier2.text = "Tier 2";
		tier2.enabled = player.CurrentTier >= 2;
		tier2.clicked += new GUIModelManager.OnClick(Tier2Clicked);

		GUIModelManager.Button tier3 = new GUIModelManager.Button();
		tier3.text = "Tier 3";
		tier3.enabled = player.CurrentTier == 3;
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
		Debug.Log("Tier 2 clicked");
		GUIModelManager.SetCurrentModel(playerID, mTier2UnitCreationModel);
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

		AddBackButton(mTier1UnitCreationModel);
		AddDefaultButtons(mTier1UnitCreationModel);
	}

	void ProduceCube() {
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Cube", "CPU");
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

	void BuildTier2UnitCreationModel() {
		mTier2UnitCreationModel = new GUIModelManager.GUIModel();

		GUIModelManager.Button cube2Button = new GUIModelManager.Button();
		// TODO icon
		cube2Button.clicked += new GUIModelManager.OnClick(ProduceTier2Cube);

		GUIModelManager.Button sphere2Button = new GUIModelManager.Button();
		// TODO icon
		sphere2Button.clicked += new GUIModelManager.OnClick(ProduceTier2Sphere);

		GUIModelManager.Button capsule2Button = new GUIModelManager.Button();
		// TODO icon
		capsule2Button.clicked += new GUIModelManager.OnClick(ProduceTier2Capsule);

		mTier2UnitCreationModel.AddButton(0, cube2Button);
		mTier2UnitCreationModel.AddButton(0, sphere2Button);
		mTier2UnitCreationModel.AddButton(0, capsule2Button);

		AddBackButton(mTier2UnitCreationModel);
		AddDefaultButtons(mTier2UnitCreationModel);
	}

	void ProduceTier2Cube() {
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Tier2Cube", "CPU");
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
		GameObject static_ = (GameObject) Instantiate(tier2Static, (Vector3) spawnPosition, Quaternion.identity);
		static_.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceTier2Sphere() {
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Tier2Sphere", "CPU");
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
		GameObject heap = (GameObject) Instantiate(tier2Heap, (Vector3) spawnPosition, Quaternion.identity);
		heap.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void ProduceTier2Capsule() {
		// TODO check resources
		ParseManager.LogEvent (ParseManager.ParseEvent.UnitCreation, playerID, "Tier2Capsule", "CPU");
		Int3 spawnPosition = GridManager.FindNextAvailPos(intPosition + spawnOffset, 8, playerID);
		GameObject tree = (GameObject) Instantiate(tier2BinaryTree, (Vector3) spawnPosition, Quaternion.identity);
		tree.GetComponent<WorldObject>().playerID = PlayerID;
	}

	void BuildTier3UnitCreationModel() {
		mTier3UnitCreationModel = new GUIModelManager.GUIModel();

		GUIModelManager.Button cube3Button = new GUIModelManager.Button();
		// TODO icon
		cube3Button.clicked += new GUIModelManager.OnClick(ProduceTier3Cube);

		GUIModelManager.Button sphere3Button = new GUIModelManager.Button();
		// TODO icon
		sphere3Button.clicked += new GUIModelManager.OnClick(ProduceTier3Sphere);

		GUIModelManager.Button capsule3Button = new GUIModelManager.Button();
		// TODO icon
		capsule3Button.clicked += new GUIModelManager.OnClick(ProduceTier3Capsule);

		mTier3UnitCreationModel.AddButton(0, cube3Button);
		mTier3UnitCreationModel.AddButton(0, sphere3Button);
		mTier3UnitCreationModel.AddButton(0, capsule3Button);

		AddBackButton(mTier3UnitCreationModel);
		AddDefaultButtons(mTier3UnitCreationModel);
	}

	void ProduceTier3Cube() {
		// TODO check resources

	}

	void ProduceTier3Sphere() {
		// TODO check resources

	}

	void ProduceTier3Capsule() {
		// TODO check resources

	}

	private void AddBackButton(GUIModelManager.GUIModel model) {
		GUIModelManager.Button back = new GUIModelManager.Button();
		back.text = "Back";
		back.clicked += () => GUIModelManager.SetCurrentModel(playerID, mTierSelectionModel);

		model.AddButton(0, back);
	}

	private void AddDefaultButtons(GUIModelManager.GUIModel model) {
		PlayerScript po = GameObject.Find("Player").GetComponent<PlayerScript>();
		PlayerScript oo = GameObject.Find("Opponent").GetComponent<PlayerScript>();
		PlayerScript player = po.id == playerID ? po : oo;

		model.centerPanelColumns = 1;

		GUIModelManager.Button random = new GUIModelManager.Button();
		random.text = "Randomize";
		random.clicked += new GUIModelManager.OnClick(ProduceRandomUnit);
		model.AddButton(1, random);

		GUIModelManager.Button upgrade = new GUIModelManager.Button();
		upgrade.text = "Upgrade";
		upgrade.enabled = player.CurrentTier < 3;
		upgrade.clicked += new GUIModelManager.OnClick(UpgradeToNextTier);
		model.AddButton(1, upgrade);
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
		PlayerScript po = GameObject.Find("Player").GetComponent<PlayerScript>();
		PlayerScript oo = GameObject.Find("Opponent").GetComponent<PlayerScript>();
		PlayerScript me = playerID == po.id ? po : oo;
		if (me.upgradeTier()) {
			BuildTierSelectionModel();
			GUIModelManager.SetCurrentModel(playerID, mTierSelectionModel);
		}
	}
}