using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class GUIManager : MonoBehaviour {

    public bool gameLoading;
    public bool usernameEntered;
    public bool countdown = false;
    public string username;
    public string loadMessage;
    public float messageCounter;

    public PlayerScript player;

    //Public GUI Skins to Initalize
    public GUISkin selectBoxSkin;
    public GUISkin ordersSkin;
    public GUISkin resourceSkin;

    //Resources
    public Texture2D powerIcon;
    public Texture2D memoryIcon;

    //Cursors
    public GameObject moveCommandCursor;
    public GUISkin mouseCursorSkin;
    public Texture2D activeCursor;
    public Texture2D leftCursor;
    public Texture2D rightCursor;
    public Texture2D upCursor;
    public Texture2D downCursor;
    public Texture2D[] selectCursors;
    public Texture2D[] attackCursors;
    public Texture2D[] hoverEnemyCursors;
    public Texture2D[] captureCursors;
    private CursorState activeCursorState;
    private int currentCursorFrame = 0;
    private bool attackingCommandCursor;
    private Vector3 destination;


    //Dragging GUI variables
    public GUISkin dragSelectSkin;
    private static GUIModelManager.GUIModel currentGUIModel;
    private Vector2 dragLocationStart;
    private Vector2 dragLocationEnd;
    private bool checkSelect = false;
    private static bool isDragging = false;

    private static Vector2 scrollPositionLeft = Vector2.zero;
    private static Vector2 scrollPositionCenter = Vector2.zero;

    public static bool Dragging {
        get {
            return isDragging;
        }
    }

    //Initialization into GUIResources
    void Start() {
        player = gameObject.GetComponent<PlayerScript>();
        GUIResources.SelectBoxSkin = selectBoxSkin;

        //Initialize the Dragging Variables
        //        dragStyle.normal.background = TextureGenerator.MakeTexture(0.8f, 0.8f, 0.8f, 0.3f);
        //        dragStyle.border.bottom = 1;
        //        dragStyle.border.top = 1;
        //        dragStyle.border.left = 1;
        //        dragStyle.border.right = 1;

        //Initialize CursorState
        SetCursorState(CursorState.Select);

        //Hide the Cursor
        Screen.showCursor = false;
        Screen.showCursor = false;
        gameLoading = true;
        username = "Enter name...";
        loadMessage = "Loading game...";
        messageCounter = 5;
    }

    void Update() {
        MouseHover();
        checkIfDragging();
        if (!isDragging && Input.GetMouseButtonUp(1)) {
            DoRightClick();
        }
    }

    private void MouseHover() {
        if (!GUIResources.MouseInPlayingArea()) {
            return;
        }
        Vector3 hitPoint = RTSGameMechanics.FindHitPoint();
        GameObject hoverObject = RTSGameMechanics.FindHitObject(hitPoint);
        if (hoverObject != null && hoverObject.tag != "Map") {
            WorldObject obj = hoverObject.GetComponent<WorldObject>();
            if (obj != null && obj.playerID != player.id) {
                SetCursorState(CursorState.HoverEnemy);
            }
        }
    }

    /*
     * Drag Select GUI
     */
    private void checkIfDragging() {
        if (Input.GetMouseButtonDown(0) && GUIResources.MouseInPlayingArea()) {
            dragLocationStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            checkSelect = true;
        }

        if (Input.GetMouseButtonUp(0)) {
            if (!isDragging && GUIResources.MouseInPlayingArea()) {
                DoLeftClick();
            }
            checkSelect = false;
            isDragging = false;
        }

        if (checkSelect) {
            if (Mathf.Abs(Input.mousePosition.x - dragLocationStart.x) > 2 &&
                    Mathf.Abs(Input.mousePosition.y - dragLocationStart.y) > 2) {
                checkSelect = false;
                isDragging = true;
            }
        }
    }

    private void DoLeftClick() {
        if (Input.GetKey(KeyCode.A) && SelectionManager.getSelectedUnits(player.id).Count > 0) {
            SetDestination(RTSGameMechanics.FindHitPoint());
            SetCursorState(CursorState.Move);
        }
    }

    private void DoRightClick() {
        Vector3 hitPoint = RTSGameMechanics.FindHitPoint();
        GameObject target = RTSGameMechanics.FindHitObject(hitPoint);
        if (target != null && target.tag != "Map") {
            FogScript fog = target.GetComponent<WorldObject>().currentFogTile.GetComponent<FogScript>();
            if (fog.friendlyUnitCount > 0) {
                if (target.GetComponent<WorldObject>().playerID != player.id) {
                    SetCursorState(CursorState.Attack);
                }
            }
        } else if (Input.mousePosition != MechanicResources.InvalidPosition) {
            SetDestination(RTSGameMechanics.FindHitPoint());
            SetCursorState(CursorState.Move);
        }
    }

    void OnGUI() {
        if(gameLoading) {
            showLoadingScreen();
            DrawMouseCursor();
        }
        else {
            DrawOrdersBar();
            DrawCurrentGUIModel();
            DrawResourceBar();
            if (isDragging) {
                float padding = GUIResources.GetScaledPixelSize(4);
                dragLocationEnd = new Vector2(
                    Mathf.Min(Mathf.Max(Input.mousePosition.x, padding), Screen.width - padding), 
                    Mathf.Min(Mathf.Max(Input.mousePosition.y, GUIResources.OrdersBarHeight + padding), Screen.height - padding));
                DragBox(dragLocationStart, dragLocationEnd, dragSelectSkin);
            }
            DrawMouseCursor();
        }
    }

    private void showLoadingScreen() {
        if(!usernameEntered) {
            GUI.backgroundColor = Color.green;
            // GUI.Label (new Rect (Screen.width/2 - 100, Screen.height/2 - 100, 200, 200), "Enter player name:");
            username = GUI.TextField(new Rect(Screen.width/2 - 100, Screen.height/2 - 25, 200, 50), username, 25);
            if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height/2 + 25, 100, 50), "Enter"))
                usernameEntered = true;
        }
        else {
            GUI.Label (new Rect (Screen.width/2 - 50, Screen.height/2 - 50, 200, 200), loadMessage);
            if(countdown && messageCounter <= 0) {
                gameLoading = false;
            }
            else if(countdown) {
                messageCounter -= Time.deltaTime;
            }
        }
    }

    public void connected(string opponentName) {
        loadMessage = "Connected to opponent: " + opponentName;
        countdown = true;
    }

    /*
     * Drawing the Mouse Cursors
     */
    public void SetCursorState(CursorState cursorState) {
        if (!attackingCommandCursor) {
            activeCursorState = cursorState;
            switch (cursorState) {
            case CursorState.Select:
                currentCursorFrame = (int)(Time.time * 10) % selectCursors.Length;
                activeCursor = selectCursors [currentCursorFrame];
                break;
            case CursorState.Attack:
                attackingCommandCursor = true;
                currentCursorFrame = (int)(Time.time * 5) % attackCursors.Length;
                activeCursor = attackCursors [currentCursorFrame];
                Invoke("FinishAttackCursor", 1.5f);
                break;
            case CursorState.HoverEnemy:
                currentCursorFrame = (int)(Time.time * 5) % hoverEnemyCursors.Length;
                activeCursor = hoverEnemyCursors [currentCursorFrame];
                break;
            case CursorState.Capture:
                currentCursorFrame = (int)Time.time % captureCursors.Length;
                activeCursor = captureCursors [currentCursorFrame];
                break;
            case CursorState.Move:
                CreateMoveCursor();
                break;
            case CursorState.PanLeft:
                activeCursor = leftCursor;
                break;
            case CursorState.PanRight:
                activeCursor = rightCursor;
                break;
            case CursorState.PanUp:
                activeCursor = upCursor;
                break;
            case CursorState.PanDown:
                activeCursor = downCursor;
                break;
            default:
                break;
            }
        }
    }

    private void DrawMouseCursor() {
        GUI.skin = mouseCursorSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
        UpdateCursorAnimation();
        Rect cursorPosition = GetCursorDrawPosition();
        GUI.Label(cursorPosition, activeCursor);
        GUI.EndGroup();
    }

    private void UpdateCursorAnimation() {
        //Sequence animation for cursor (based on more than one image for the cursor)
        //Change once per second, loops through array of images
        if (activeCursorState == CursorState.Select) {
            currentCursorFrame = (int)(Time.time * 10) % selectCursors.Length;
            activeCursor = selectCursors [currentCursorFrame];
        } else if (activeCursorState == CursorState.Attack) {
            currentCursorFrame = (int)(Time.time * 5) % attackCursors.Length;
            activeCursor = attackCursors [currentCursorFrame];
        } else if (activeCursorState == CursorState.HoverEnemy) {
            currentCursorFrame = (int)(Time.time * 10) % hoverEnemyCursors.Length;
            activeCursor = hoverEnemyCursors [currentCursorFrame];
        }
    }

    private Rect GetCursorDrawPosition() {
        //Set base position for custom cursor image
        float leftPos = Input.mousePosition.x;
        //Screen draw coordinates are inverted
        float topPos = Screen.height - Input.mousePosition.y;
        //Adjust position base on the type of cursor being shown
        if (activeCursorState == CursorState.PanRight)
            leftPos = Screen.width - activeCursor.width;
        else if (activeCursorState == CursorState.PanDown)
            topPos = Screen.height - activeCursor.height;
        //        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Capture) {
        //            topPos -= activeCursor.height / 2;
        //            leftPos -= activeCursor.width / 2;
        //        }
        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public void SetDestination(Vector3 destination) {
        this.destination = destination;
    }

    void FinishAttackCursor() {
        attackingCommandCursor = false;
    }

    private void CreateMoveCursor() {
        Instantiate(moveCommandCursor, destination, Quaternion.identity);
    }

    /*
     * Drawing the Order and Resource GUIS
     */
    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(0, Screen.height - GUIResources.OrdersBarHeight, Screen.width, GUIResources.OrdersBarHeight));
        GUI.Box(new Rect(0, 0, Screen.width, GUIResources.OrdersBarHeight), "");
        GUI.EndGroup();
    }

    private void DrawCurrentGUIModel() {
        currentGUIModel = GUIModelManager.GetCurrentModel(player.id);
        if (currentGUIModel == null) {
            return;
        }

        float panelWidth = Screen.width / 3;

        float outerPadding = GUIResources.GetScaledPixelSize(8);

        float titleHeight = GUIResources.GetScaledPixelSize(30);
        float tooltipHeight = GUIResources.GetScaledPixelSize(30);
        float buttonHeight = (GUIResources.OrdersBarHeight - 2 * outerPadding - titleHeight - tooltipHeight) / 3;

		float leftButtonWidth = (panelWidth - 2 * outerPadding) / currentGUIModel.leftPanelColumns;
		float centerButtonWidth = (panelWidth - 2 * outerPadding) / currentGUIModel.centerPanelColumns;

        float initX = outerPadding;
        float initY = Screen.height - GUIResources.OrdersBarHeight + outerPadding;

        GUIModelManager.Button temp;

        // Draw left panel title
        GUI.Label(new Rect(initX + outerPadding, initY, panelWidth - 2 * outerPadding, titleHeight), currentGUIModel.leftPanelTitle);
        initY += (titleHeight + tooltipHeight);

		// Draw left panel icons
		scrollPositionLeft = GUI.BeginScrollView(new Rect(initX, initY, panelWidth, GUIResources.OrdersBarHeight - (titleHeight + tooltipHeight)),
		                                         scrollPositionLeft, 
		                                         new Rect(0, 0, panelWidth, Mathf.CeilToInt((float) currentGUIModel.leftPanelButtons.Count / currentGUIModel.leftPanelColumns) * buttonHeight));
		for (int i = 0, len = currentGUIModel.leftPanelButtons.Count; i < len; i++) {
            temp = currentGUIModel.leftPanelButtons[i];
            float buttonX = i % currentGUIModel.leftPanelColumns * leftButtonWidth;
            float buttonY = i / currentGUIModel.leftPanelColumns * buttonHeight;
            Rect drawRect = new Rect(buttonX, buttonY, leftButtonWidth, buttonHeight);

            GUI.enabled = temp.enabled;
			if (temp.text != null) {
				GUI.Button(drawRect, new GUIContent(temp.text, temp.hint));
			} else {
				GUI.Button(drawRect, new GUIContent(temp.icon, temp.hint));
			}
            GUI.enabled = true;

			temp.rect = new Rect(initX + buttonX, initY + buttonY, leftButtonWidth, buttonHeight);
		}
		GUI.EndScrollView();

        // Draw center panel title
        initY -= (titleHeight + tooltipHeight);
        initX = panelWidth + outerPadding;
        GUI.Label(new Rect(initX + outerPadding, initY, panelWidth - 2 * outerPadding, titleHeight), currentGUIModel.centerPanelTitle);
        initY += (titleHeight + tooltipHeight);
		
		// Draw center panel icons
		scrollPositionCenter = GUI.BeginScrollView(new Rect(initX, initY, panelWidth, GUIResources.OrdersBarHeight - (titleHeight + tooltipHeight)), 
		                                           scrollPositionCenter, 
		                                           new Rect(0, 0, panelWidth, Mathf.CeilToInt((float) currentGUIModel.centerPanelButtons.Count / currentGUIModel.centerPanelColumns) * buttonHeight));
		for (int i = 0, len = currentGUIModel.centerPanelButtons.Count; i < len; i++) {
            temp = currentGUIModel.centerPanelButtons[i];
            float buttonX = i % currentGUIModel.centerPanelColumns * centerButtonWidth;
            float buttonY = i / currentGUIModel.centerPanelColumns * buttonHeight;
            Rect drawRect = new Rect(buttonX, buttonY, centerButtonWidth, buttonHeight);

            GUI.enabled = temp.enabled;
			if (temp.text != null) {
				GUI.Button(drawRect, new GUIContent(temp.text, temp.hint));
			} else {
				GUI.Button(drawRect, new GUIContent(temp.icon, temp.hint));
			}
            GUI.enabled = true;

			temp.rect = new Rect(initX + buttonX, initY + buttonY, centerButtonWidth, buttonHeight);
		}
		GUI.EndScrollView ();

        // Draw the tooltip
        initY -= tooltipHeight;
        if (Input.mousePosition.x < panelWidth) {
            initX -= (panelWidth + outerPadding);
        }
        GUI.Label(new Rect(initX + outerPadding, initY, panelWidth - 2 * outerPadding, tooltipHeight), GUI.tooltip);
    }

    private void DrawResourceBar() {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, GUIResources.ResourceBarHeight));

        float topPos = GUIResources.GetScaledPixelSize(4);
        float iconLeft = GUIResources.GetScaledPixelSize(4);
        float textLeft = GUIResources.GetScaledPixelSize(2 * GUIResources.IconWidth);
        DrawResourceIcon(powerIcon, iconLeft, textLeft, topPos);
        iconLeft += GUIResources.TextWidth;
        textLeft += GUIResources.TextWidth;
        DrawResourceIcon(memoryIcon, iconLeft, textLeft, topPos);

        GUI.EndGroup();
    }

    private void DrawResourceIcon(Texture2D resourceIcon, float iconLeft, float textLeft, float topPos) {
        string text;
        if (resourceIcon.Equals(powerIcon)) {
            text = player.getPower().ToString();
        } else {
            text = player.getMemory().ToString() + "/" + player.getMaxMemory().ToString();
        }
        GUI.DrawTexture(new Rect(iconLeft, topPos, GUIResources.IconWidth, GUIResources.IconHeight), resourceIcon);
        GUI.Label(new Rect(textLeft, topPos, GUIResources.TextWidth, GUIResources.TextHeight), text);
    }

    public void DragBox(Vector2 topLeft, Vector2 bottomRight, GUISkin dragSkin) {
        float minX = Mathf.Max(topLeft.x, bottomRight.x);
        float maxX = Mathf.Min(topLeft.x, bottomRight.x);

        float minY = Mathf.Max(Screen.height - topLeft.y, Screen.height - bottomRight.y);
        float maxY = Mathf.Min(Screen.height - topLeft.y, Screen.height - bottomRight.y);

        Rect rect = new Rect(minX, minY, maxX - minX, maxY - minY);

        GUI.Box(rect, "", dragSkin.box);
    }

    /*
     * User Input on Buttons
     */
    public static int [] GetButtonID(Vector3 mousePos, out GUIModelManager.Button pressed) {
        if (currentGUIModel == null) {
            pressed = null;
            return null;
        }

        int[] button = new int[2];
        Vector2 mousePos2D = new Vector2(mousePos.x, Screen.height - mousePos.y);

        Vector2 mousePosLeft = mousePos2D + scrollPositionLeft;
        for (int i = 0; i < currentGUIModel.leftPanelButtons.Count; i++) {
            if (currentGUIModel.leftPanelButtons[i].rect.Contains(mousePosLeft)) {
                pressed = currentGUIModel.leftPanelButtons[i];
                button [0] = 0;
                button [1] = i;
                return button;
            }
        }

        Vector2 mousePosCenter = mousePos2D + scrollPositionCenter;
        for (int i = 0; i < currentGUIModel.centerPanelButtons.Count; i++) {
            if (currentGUIModel.centerPanelButtons[i].rect.Contains(mousePosCenter)) {
                pressed = currentGUIModel.centerPanelButtons[i];
                button [0] = 1;
                button [1] = i;
                return button;
            }
        }
        pressed = null;
        return null;
    }
}
