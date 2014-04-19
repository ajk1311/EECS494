using UnityEngine;
using System.Collections.Generic;
using EventBus;
using SSGameEvents;
using RTS;
using Parse;

public class StartScript : MonoBehaviour {
	private Vector3 cameraStartPosition1 = new Vector3(75, 60, 150);
	private Vector3 cameraStartPosition2 = new Vector3(725, 60, 150);

	private Vector3 assembler1Pos = new Vector3 (50, 1, 190);
	private Vector3 assembler2Pos = new Vector3 (750, 1, 190);

    private Vector3 magentaDefensiveTower1 = new Vector3(610, 1, 272.5f);
    private Vector3 magentaDefensiveTower2 = new Vector3(710, 1, 200);
    private Vector3 magentaDefensiveTower3 = new Vector3(610, 1, 127.5f);
    private Vector3 orangeDefensiveTower1 = new Vector3(190, 1, 272.5f);
    private Vector3 orangeDefensiveTower2 = new Vector3(90, 1, 200);
    private Vector3 orangeDefensiveTower3 = new Vector3(190, 1, 127.5f);

	public Object magentaCpuPrefab;
	public Object orangeCpuPrefab;
    public Object orangeDefensiveTower;
    public Object magentaDefensiveTower;
	public Object assembler;
	public Object captureTower;

	public WorldObject[] objs;

	void Start() {
		Dispatcher.Instance.Register(this);
		SSGameSetup.ConnectToGame("akausejr", true);
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
		ParseManager.Init(connectionEvent.ID, connectionEvent.gameID);

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

		GameObject magentaCpu = (GameObject) Instantiate(magentaCpuPrefab);
		GameObject orangeCpu = (GameObject) Instantiate(orangeCpuPrefab);
        List<GameObject> orangeDefensiveTowers = new List<GameObject>();
        orangeDefensiveTowers.Add((GameObject)Instantiate(orangeDefensiveTower, orangeDefensiveTower1, Quaternion.identity));
        orangeDefensiveTowers.Add((GameObject)Instantiate(orangeDefensiveTower, orangeDefensiveTower2, Quaternion.identity));
        orangeDefensiveTowers.Add((GameObject)Instantiate(orangeDefensiveTower, orangeDefensiveTower3, Quaternion.identity));
        List<GameObject> magentaDefensiveTowers = new List<GameObject>();
        magentaDefensiveTowers.Add((GameObject)Instantiate(magentaDefensiveTower, magentaDefensiveTower1, Quaternion.identity));
        magentaDefensiveTowers.Add((GameObject)Instantiate(magentaDefensiveTower, magentaDefensiveTower2, Quaternion.identity));
        magentaDefensiveTowers.Add((GameObject)Instantiate(magentaDefensiveTower, magentaDefensiveTower3, Quaternion.identity));


		GameObject assembler1 = (GameObject) Instantiate(assembler, assembler1Pos, Quaternion.identity);
		assembler1.name = "assembler1";
		GameObject assembler2 = (GameObject)Instantiate (assembler, assembler2Pos, Quaternion.identity);
		assembler2.name = "assembler2";

		if (connectionEvent.ID == 1) {
			orangeCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			magentaCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
//            foreach(GameObject gameObject in orangeDefensiveTowers)
//                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.ID;
//            foreach(GameObject gameObject in magentaDefensiveTowers)
//                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			Camera.main.GetComponent<CameraControl>().StartPosition = cameraStartPosition1;

			assembler1.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
			assembler2.GetComponent<AssemblerScript>().playerID = connectionEvent.opponentID;

		} else {
			orangeCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			magentaCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
//            foreach(GameObject gameObject in orangeDefensiveTowers)
//                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
//            foreach(GameObject gameObject in magentaDefensiveTowers)
			//                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			Camera.main.GetComponent<CameraControl>().StartPosition = cameraStartPosition2;

			assembler2.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
			assembler1.GetComponent<AssemblerScript>().playerID = connectionEvent.opponentID;

		}

//		GameObject dogwaffle = (GameObject)Instantiate (captureTower);

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent) {
		Debug.Log("Game is ready");
	}
}
