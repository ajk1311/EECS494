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
    public GUISkin mouseCursorSkin;
    public Texture2D activeCursor;
    public Texture2D leftCursor;
    public Texture2D rightCursor;
    public Texture2D upCursor;
    public Texture2D downCursor;
    public Texture2D[] selectCursors;
    public Texture2D[] attackCursors;
    public Texture2D[] moveCursors;
    public Texture2D[] captureCursors;
    private CursorState activeCursorState;
    private int currentCursorFrame = 0;

    //Dragging GUI variables
    private static GUIModelManager.GUIModel mCurrentGuiModel;
    private GUIStyle mDragStyle = new GUIStyle();
    private Vector2 mDragLocationStart;
    private Vector2 mDragLocationEnd;
    private bool mCheckDeselect = false;
    private static bool sDragging = false;

    public static bool Dragging {
        get { return sDragging; }
    }

    //Initialization into GUIResources
    void Start() {
        player = gameObject.GetComponent<PlayerScript>();
        GUIResources.SelectBoxSkin = selectBoxSkin;

        //Initialize the Dragging Variables
        mDragStyle.normal.background = TextureGenerator.MakeTexture(0.8f, 0.8f, 0.8f, 0.3f);
        mDragStyle.border.bottom = 1;
        mDragStyle.border.top = 1;
        mDragStyle.border.left = 1;
        mDragStyle.border.right = 1;

        //Initialize CursorState
        SetCursorState(CursorState.Select);

        //Hide the Cursor
        Screen.showCursor = false;
    }

    void Update() {
        checkIfDragging();
        MouseHover();
    }
    
    void OnGUI() {
        DrawOrdersBar();
        DrawCurrentGUIModel();
        DrawResourceBar();
        DrawMouseCursor();
        if (sDragging) {
            float padding = GUIResources.GetScaledPixelSize(4);
            mDragLocationEnd = new Vector2(
                Mathf.Min(Mathf.Max(Input.mousePosition.x, padding), Screen.width - padding), 
                Mathf.Min(Mathf.Max(Input.mousePosition.y, GUIResources.OrdersBarHeight + padding), Screen.height - padding));
            DragBox(mDragLocationStart, mDragLocationEnd, mDragStyle);
        }
    }

    /*
     * Drawing the Mouse Cursors
     */
    public void SetCursorState(CursorState cursorState){
        activeCursorState = cursorState;
        switch(cursorState) {
        case CursorState.Select:
            currentCursorFrame = (int)(Time.time * 10) % selectCursors.Length;
            activeCursor = selectCursors[currentCursorFrame];
            break;
        case CursorState.Attack:
            currentCursorFrame = (int)(Time.time * 5) % attackCursors.Length;
            activeCursor = attackCursors[currentCursorFrame];
            break;
        case CursorState.Capture:
            currentCursorFrame = (int)Time.time % captureCursors.Length;
            activeCursor = captureCursors[currentCursorFrame];
            break;
        case CursorState.Move:
            currentCursorFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentCursorFrame];
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
        default: break;
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
            activeCursor = selectCursors[currentCursorFrame];
        }
        else if(activeCursorState == CursorState.Move) {
            currentCursorFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentCursorFrame];
        } else if(activeCursorState == CursorState.Attack) {
            currentCursorFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentCursorFrame];
        } else if(activeCursorState == CursorState.Capture) {
            currentCursorFrame = (int)Time.time % captureCursors.Length;
            activeCursor = captureCursors[currentCursorFrame];
        }
    }
    
    private Rect GetCursorDrawPosition() {
        //Set base position for custom cursor image
        float leftPos = Input.mousePosition.x;
        //Screen draw coordinates are inverted
        float topPos = Screen.height - Input.mousePosition.y; 
        //Adjust position base on the type of cursor being shown
        if(activeCursorState == CursorState.PanRight) 
            leftPos = Screen.width - activeCursor.width;
        else if(activeCursorState == CursorState.PanDown) 
            topPos = Screen.height - activeCursor.height;
        else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Capture) {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    private void MouseHover() {
        if (GUIResources.MouseInPlayingArea()) {
            GameObject hoverObject = RTSGameMechanics.FindHitObject();
            if (hoverObject) {
                if (player.SelectedObject)
                    player.SelectedObject.SetHoverState(hoverObject);
                else if (hoverObject.name != "Ground") {
                    Player owner = hoverObject.transform.root.GetComponent<Player>();
                    if (owner) {
                        Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
                        Building building = hoverObject.transform.parent.GetComponent<Building>();
                        if (owner.username == player.username && (unit || building)) 
                            player.hud.SetCursorState(CursorState.Select);
                    }
                }
            }
        }
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
		mCurrentGuiModel = GUIModelManager.GetCurrentModel(player.id);
		if (mCurrentGuiModel == null) {
			return;
		}

		if(!mCurrentGuiModel.cached) {
			float panelWidth = Screen.width / 3;

			float innerPadding = GUIResources.GetScaledPixelSize(2);
			float verticalPadding = GUIResources.GetScaledPixelSize(24);
			float horizontalPadding = GUIResources.GetScaledPixelSize(36);
			
			float buttonHeight = GUIResources.GetScaledPixelSize(48);
			float buttonMinWidth = GUIResources.GetScaledPixelSize(100);
			
			float leftButtonWidth = (panelWidth - 1.5f * horizontalPadding - innerPadding * (mCurrentGuiModel.leftPanelColumns - 1)) / mCurrentGuiModel.leftPanelColumns;
			float centerButtonWidth = (panelWidth - horizontalPadding - innerPadding * (mCurrentGuiModel.leftPanelColumns - 1)) / mCurrentGuiModel.centerPanelColumns;

			float initialX = horizontalPadding;
			float initialY = Screen.height - GUIResources.OrdersBarHeight + verticalPadding;

			for (int i = 0, len = mCurrentGuiModel.leftPanelButtons.Count; i < len; i++) {
				int row = i / mCurrentGuiModel.leftPanelColumns;
				int column = i % mCurrentGuiModel.leftPanelColumns;
				float buttonX = initialX + column * leftButtonWidth + (column  > 0 ? innerPadding : 0);
				float buttonY = initialY + row * buttonHeight + (row > 0 ? innerPadding : 0);
				mCurrentGuiModel.leftPanelButtons[i].rect = new Rect(buttonX, buttonY, leftButtonWidth, buttonHeight);
			}

			initialX = panelWidth + horizontalPadding / 2;
			for (int i = 0, len = mCurrentGuiModel.centerPanelButtons.Count; i < len; i++) {
				int row = i / mCurrentGuiModel.centerPanelColumns;
				int column = i % mCurrentGuiModel.centerPanelColumns;
				float buttonX = initialX + column * centerButtonWidth + (column > 0 ? innerPadding : 0);
				float buttonY = initialY + row * buttonHeight + (row > 0 ? innerPadding : 0);
				mCurrentGuiModel.centerPanelButtons[i].rect = new Rect(buttonX, buttonY, centerButtonWidth, buttonHeight);
				
			}

			mCurrentGuiModel.cached = true;
		}

		GUIModelManager.Button temp;

		//draw left panel icons
		for (int i = 0, len = mCurrentGuiModel.leftPanelButtons.Count; i < len; i++) {
			temp = mCurrentGuiModel.leftPanelButtons[i];
			if (temp.text != null) {
				GUI.Button(temp.rect, temp.text);
			} else {
				GUI.Button(temp.rect, temp.icon);
			}
		}
		//draw center panel icons
		for (int i = 0, len = mCurrentGuiModel.centerPanelButtons.Count; i < len; i++) {
			temp = mCurrentGuiModel.centerPanelButtons[i];
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
            mDragLocationStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mCheckDeselect = true;
        }
        
        if (Input.GetMouseButtonUp(0)) {
            mCheckDeselect = false;
            sDragging = false;
        }
        
        if (mCheckDeselect) {
            if (Mathf.Abs(Input.mousePosition.x - mDragLocationStart.x) > 2 && 
                Mathf.Abs(Input.mousePosition.y - mDragLocationStart.y) > 2) {
                mCheckDeselect = false;
                sDragging = true;
            }
        }
    }

    public void DragBox(Vector2 topLeft, Vector2 bottomRight, GUIStyle style) {
        float minX = Mathf.Max(topLeft.x, bottomRight.x);
        float maxX = Mathf.Min(topLeft.x, bottomRight.x);
        
        float minY = Mathf.Max(Screen.height - topLeft.y, Screen.height - bottomRight.y);
        float maxY = Mathf.Min(Screen.height - topLeft.y, Screen.height - bottomRight.y);
        
        Rect rect = new Rect(minX, minY, maxX - minX, maxY - minY);

        GUI.Box(rect, "", style);
    }

    /*
     * User Input on Buttons
     */
    public static int [] GetButtonID(Vector3 mousePos) {
        int[] button = new int[2];
        Vector2 mousePos2D = new Vector2(mousePos.x, Screen.height - mousePos.y);
        for (int i = 0; i < mCurrentGuiModel.leftPanelButtons.Count; i++) {
            if (mCurrentGuiModel.leftPanelButtons [i].rect.Contains(mousePos2D)) {
                button [0] = 0;
                button [1] = i;
                return button;
            }
        }

        for (int i = 0; i < mCurrentGuiModel.centerPanelButtons.Count; i++) {
            if (mCurrentGuiModel.centerPanelButtons [i].rect.Contains(mousePos2D)) {
                button [0] = 1;
                button [1] = i;
                return button;
            }
        }
        return null;
    }
}
