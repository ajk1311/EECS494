using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class GUIManager : MonoBehaviour {

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

    public static bool Dragging {
        get { return isDragging; }
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
    }

    void Update() {
        checkIfDragging();
    }
    
    void OnGUI() {
        DrawOrdersBar();
        DrawCurrentGUIModel();
        DrawResourceBar();
        DrawMouseCursor();
        if (isDragging) {
            float padding = GUIResources.GetScaledPixelSize(4);
            dragLocationEnd = new Vector2(
                Mathf.Min(Mathf.Max(Input.mousePosition.x, padding), Screen.width - padding), 
                Mathf.Min(Mathf.Max(Input.mousePosition.y, GUIResources.OrdersBarHeight + padding), Screen.height - padding));
            DragBox(dragLocationStart, dragLocationEnd, dragSelectSkin);
        }
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
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Capture) {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public void SetDestination(Vector3 destination){
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

        if (!currentGUIModel.cached) {
            float panelWidth = Screen.width / 3;

            float innerPadding = GUIResources.GetScaledPixelSize(2);
            float verticalPadding = GUIResources.GetScaledPixelSize(24);
            float horizontalPadding = GUIResources.GetScaledPixelSize(36);
            
            float buttonHeight = GUIResources.GetScaledPixelSize(48);
            float buttonMinWidth = GUIResources.GetScaledPixelSize(100);
            
            float leftButtonWidth = (panelWidth - 1.5f * horizontalPadding - innerPadding * (currentGUIModel.leftPanelColumns - 1)) / currentGUIModel.leftPanelColumns;
            float centerButtonWidth = (panelWidth - horizontalPadding - innerPadding * (currentGUIModel.leftPanelColumns - 1)) / currentGUIModel.centerPanelColumns;

            float initialX = horizontalPadding;
            float initialY = Screen.height - GUIResources.OrdersBarHeight + verticalPadding;

            for (int i = 0, len = currentGUIModel.leftPanelButtons.Count; i < len; i++) {
                int row = i / currentGUIModel.leftPanelColumns;
                int column = i % currentGUIModel.leftPanelColumns;
                float buttonX = initialX + column * leftButtonWidth + (column > 0 ? innerPadding : 0);
                float buttonY = initialY + row * buttonHeight + (row > 0 ? innerPadding : 0);
                currentGUIModel.leftPanelButtons [i].rect = new Rect(buttonX, buttonY, leftButtonWidth, buttonHeight);
            }

            initialX = panelWidth + horizontalPadding / 2;
            for (int i = 0, len = currentGUIModel.centerPanelButtons.Count; i < len; i++) {
                int row = i / currentGUIModel.centerPanelColumns;
                int column = i % currentGUIModel.centerPanelColumns;
                float buttonX = initialX + column * centerButtonWidth + (column > 0 ? innerPadding : 0);
                float buttonY = initialY + row * buttonHeight + (row > 0 ? innerPadding : 0);
                currentGUIModel.centerPanelButtons [i].rect = new Rect(buttonX, buttonY, centerButtonWidth, buttonHeight);
                
            }

            currentGUIModel.cached = true;
        }

        GUIModelManager.Button temp;

        //draw left panel icons
        for (int i = 0, len = currentGUIModel.leftPanelButtons.Count; i < len; i++) {
            temp = currentGUIModel.leftPanelButtons [i];
            if (temp.text != null) {
                GUI.Button(temp.rect, temp.text);
            } else {
                GUI.Button(temp.rect, temp.icon);
            }
        }
        //draw center panel icons
        for (int i = 0, len = currentGUIModel.centerPanelButtons.Count; i < len; i++) {
            temp = currentGUIModel.centerPanelButtons [i];
            if (temp.text != null) {
                GUI.Button(temp.rect, temp.text);
            } else {
                GUI.Button(temp.rect, temp.icon);
            }
        }
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

    /*
     * Drag Select GUI
     */
    private void checkIfDragging() {
        if (Input.GetMouseButtonDown(0)) {
            dragLocationStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            checkSelect = true;
        }
        
        if (Input.GetMouseButtonUp(0)) {
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
    public static int [] GetButtonID(Vector3 mousePos) {
        int[] button = new int[2];
        Vector2 mousePos2D = new Vector2(mousePos.x, Screen.height - mousePos.y);
        for (int i = 0; i < currentGUIModel.leftPanelButtons.Count; i++) {
            if (currentGUIModel.leftPanelButtons [i].rect.Contains(mousePos2D)) {
                button [0] = 0;
                button [1] = i;
                return button;
            }
        }

        for (int i = 0; i < currentGUIModel.centerPanelButtons.Count; i++) {
            if (currentGUIModel.centerPanelButtons [i].rect.Contains(mousePos2D)) {
                button [0] = 1;
                button [1] = i;
                return button;
            }
        }
        return null;
    }
}
