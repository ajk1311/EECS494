using UnityEngine;
using System.Collections.Generic;
using EventBus;
using SSGameEvents;
using RTS;
using Parse;

public class StartScript : MonoBehaviour {
	public bool testServer;
    public string username;
    public string serverIP;
    public string loadMessage;
    public Texture startLogo;

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
	public Object orangeRAMPrefab;
	public Object magentaRAMPrefab;

	public Object tower1;
	public Object tower2;
	public Object tower3;
	public Object tower4;
	public Object centerTower;

	public WorldObject[] objs;

	private GUIManager guiManager;
	private bool notConnected = true;

	private float panSpeed;
	private string winnerName;
	private bool gameOver = false;
	private GUIStyle gameOverStyle;
	private Vector3 gameOverPanPosition;

	private const int START = 0;
	private const int CONNECTING = 2;
	private const int READY = 3;
	private const int OVER = 4;
	private int gameState;

	public class GameOverEvent {
		public int loserPlayerID;
		public GameOverEvent(int loserPlayerID_) {
			loserPlayerID = loserPlayerID_;
		}
	}

	void Start() {
		gameState = START;
		testServer = false;
        username = "Enter name...";
        loadMessage = "Loading game...";
        serverIP = "Server IP...";
		panSpeed = 6;
		gameOverStyle = new GUIStyle();
		Dispatcher.Instance.Register(this);
		guiManager = GameObject.Find("Player").GetComponent<GUIManager>();
		Camera.main.GetComponent<CameraControl>().enabled = false;
	}

	void Update() {
		if (gameState == OVER) {
			if (Vector3.Distance(Camera.main.transform.position, gameOverPanPosition) > 0.2f) {
				Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, gameOverPanPosition, panSpeed * Time.deltaTime);
			} else {
				Invoke("EndGame", 10);
			}
		}
	}

	void EndGame() {
		Application.LoadLevel(Application.loadedLevelName);
	}

	void OnGUI() {
		switch (gameState) {
			case START:
				showStartScreen();
				break;

			case CONNECTING:
				showLoadingScreen();
				break;

			case READY:
				showOpponent();
				break;

			case OVER:
				showGameOver();
				break;
		}
	}

	private void showStartScreen() {
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), startLogo);
        GUI.backgroundColor = Color.green;
        // GUI.Label (new Rect (Screen.width/2 - 100, Screen.height/2 - 100, 200, 200), "Enter player name:");
        username = GUI.TextField(new Rect(Screen.width/2 - 100, Screen.height/2 + 200, 200, 50), username, 25);
        serverIP = GUI.TextField(new Rect(Screen.width/2 - 100, Screen.height/2 + 250, 200, 50), serverIP, 25);
        testServer = GUI.Toggle(new Rect(Screen.width/2 - 100, Screen.height/2 + 300, 200, 25), testServer, "Use Test Server");
        if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 325, 100, 50), "Enter")) {
			if (serverIP == "Server IP...") {
				serverIP = "";
			}
			SSGameSetup.ConnectToGame(username, testServer, serverIP);
			gameState = CONNECTING;
        }
	}

	private void showLoadingScreen() {
        GUI.Label(new Rect (Screen.width/2 - 50, Screen.height/2 - 50, 200, 200), loadMessage);
    }

    private bool hideInvoked = false;

    private void showOpponent() {
    	showLoadingScreen();
    	if (!hideInvoked) {
    		Invoke("hideOpponent", 3);
    		hideInvoked = true;
    	}
    }

    private void hideOpponent() {
    	loadMessage = null;
    }

    private void showGameOver() {
    	gameOverStyle.alignment = TextAnchor.MiddleCenter;
		gameOverStyle.fontSize = 200;
		gameOverStyle.normal.textColor = new Color(0, 1, 0.09f);
		float w = Screen.width / 3;
		float h = Screen.height / 3;
		GUI.Label(new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h), winnerName + " won!");
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
		playerObject.GetComponent<PlayerScript>().playerName = connectionEvent.name;
		myInputManager = playerObject.GetComponent<UserInputManager>();
		myInputManager.playerID = connectionEvent.ID;
		FogOfWarManager.playerID = connectionEvent.ID;

		GameObject opponentObject = GameObject.Find("Opponent");
		opponentObject.GetComponent<PlayerScript>().id = connectionEvent.opponentID;
		opponentObject.GetComponent<PlayerScript>().playerName = connectionEvent.opponentName;
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

        GameObject magentaRAM = (GameObject) Instantiate(magentaRAMPrefab);
        GameObject orangeRAM = (GameObject) Instantiate(orangeRAMPrefab);

        orangeCpu.GetComponent<CPU>().Ram = orangeRAM.GetComponent<RAM>();
        magentaCpu.GetComponent<CPU>().Ram = magentaRAM.GetComponent<RAM>();

		GameObject assembler1 = (GameObject) Instantiate(assembler, assembler1Pos, Quaternion.identity);
		assembler1.name = "assembler1";
		GameObject assembler2 = (GameObject)Instantiate (assembler, assembler2Pos, Quaternion.identity);
		assembler2.name = "assembler2";

		if (connectionEvent.ID == 1) {
			orangeCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			magentaCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
            foreach(GameObject gameObject in orangeDefensiveTowers)
                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.ID;
            foreach(GameObject gameObject in magentaDefensiveTowers)
                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			Camera.main.GetComponent<CameraControl>().StartPosition = cameraStartPosition1;

			assembler1.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
			assembler2.GetComponent<AssemblerScript>().playerID = connectionEvent.opponentID;

		} else {
			orangeCpu.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
			magentaCpu.GetComponent<WorldObject>().playerID = connectionEvent.ID;
            foreach(GameObject gameObject in orangeDefensiveTowers)
                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.opponentID;
            foreach(GameObject gameObject in magentaDefensiveTowers)
			                gameObject.GetComponent<WorldObject>().playerID = connectionEvent.ID;
			Camera.main.GetComponent<CameraControl>().StartPosition = cameraStartPosition2;

			assembler2.GetComponent<AssemblerScript>().playerID = connectionEvent.ID;
			assembler1.GetComponent<AssemblerScript>().playerID = connectionEvent.opponentID;

		}

		GameObject tower1Object = (GameObject)Instantiate (tower1);
		GameObject tower2Object = (GameObject)Instantiate (tower2);
		GameObject tower3Object = (GameObject)Instantiate (tower3);
		GameObject tower4Object = (GameObject)Instantiate (tower4);

		tower1Object.GetComponent<CaptureBuilding> ().detectionRadius = 18;
		tower2Object.GetComponent<CaptureBuilding> ().detectionRadius = 18;
		tower3Object.GetComponent<CaptureBuilding> ().detectionRadius = 18;
		tower4Object.GetComponent<CaptureBuilding> ().detectionRadius = 18;

		GameObject centerTowerObject = (GameObject)Instantiate (centerTower);

		SSGameSetup.Ready(connectionEvent.ID);
	}
	
	[HandlesEvent]
	public void OnGameReady(GameReadyEvent readyEvent) {
		loadMessage = "Connected to " + readyEvent.opponentInfo.name;
		guiManager.gameLoading = false;
		Camera.main.GetComponent<CameraControl>().enabled = true;
		Debug.Log("Game is ready");
		gameState = READY;
	}

	[HandlesEvent]
	public void OnGameOver(GameOverEvent gameOverEvent) {
		gameState = OVER;
		Camera.main.GetComponent<CameraControl>().enabled = false;

		PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
		PlayerScript opponent = GameObject.Find("Opponent").GetComponent<PlayerScript>();

		if (gameOverEvent.loserPlayerID == player.id) {
			winnerName = opponent.playerName;
			gameOverPanPosition = cameraStartPosition1;
		} else {
			winnerName = player.playerName;
			gameOverPanPosition = cameraStartPosition2;
		}
		SSGameSetup.DisconnectFromGame();
	}
}
