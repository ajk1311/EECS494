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
    }
    
    void OnGUI() {
        DrawOrdersBar();
		DrawCurrentGUIModel();
        DrawResourceBar();

		if (sDragging) {
			float padding = GUIResources.GetScaledPixelSize(4);
			mDragLocationEnd = new Vector2(
				Mathf.Min(Mathf.Max(Input.mousePosition.x, padding), Screen.width - padding), 
				Mathf.Min(Mathf.Max(Input.mousePosition.y, GUIResources.OrdersBarHeight + padding), Screen.height - padding));
			DragBox(mDragLocationStart, mDragLocationEnd, mDragStyle);
		}
    }
    
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
//        GUI.DrawTexture(new Rect(iconLeft, topPos, GUIResources.IconWidth, GUIResources.IconHeight), resourceIcon);
        GUI.Label(new Rect(textLeft, topPos, GUIResources.TextWidth, GUIResources.TextHeight), text);
    }

	void Update() {
		checkIfDragging();
	}

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

	public static int[] GetButtonID(Vector3 mousePos) {
		int[] button = new int[2];
		Vector2 mousePos2D = new Vector2 (mousePos.x, Screen.height - mousePos.y);
		for(int i = 0; i < mCurrentGuiModel.leftPanelButtons.Count; i++) {
			if(mCurrentGuiModel.leftPanelButtons[i].rect.Contains(mousePos2D)) {
				button[0] = 0;
				button[1] = i;
				return button;
			}
		}
		for(int i = 0; i < mCurrentGuiModel.centerPanelButtons.Count; i++) {
			if(mCurrentGuiModel.centerPanelButtons[i].rect.Contains(mousePos2D)) {
				button[0] = 1;
				button[1] = i;
				return button;
			}
		}
		return null;
	}
}
