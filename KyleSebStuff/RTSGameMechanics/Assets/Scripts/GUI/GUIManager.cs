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
	private static GUIModelManager.GUIModel curr_model;
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
    
    /*** Private Worker Methods ***/
    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(0, Screen.height - GUIResources.OrdersBarHeight, Screen.width, GUIResources.OrdersBarHeight));
        GUI.Box(new Rect(0, 0, Screen.width, GUIResources.OrdersBarHeight), "");
        GUI.EndGroup();
    }

	private void DrawCurrentGUIModel() {
		curr_model = GUIModelManager.GetCurrentModel(player.id);
		if (curr_model == null) {
			return;
		}
		float panel_height = Screen.height / 4.0f;
		float icon_height = panel_height / 3;

		float left_panel_width = Screen.width / 3.0f;
		float center_panel_width = Screen.width / 3.0f;
		
		float left_icon_width = left_panel_width / curr_model.leftPanelColumns;
		float center_icon_width = center_panel_width / curr_model.centerPanelColumns;
		
		if(!curr_model.cached) {
			for (int i = 0, len = curr_model.leftPanelButtons.Count; i < len; i++) {
				float button_x = (i % curr_model.leftPanelColumns) * left_icon_width;
				float button_y = (3.0f/4.0f) * Screen.height + (i / 3) * icon_height;
				curr_model.leftPanelButtons[i].rect = new Rect(button_x, button_y, left_icon_width, icon_height);
			}
			for (int i = 0, len = curr_model.centerPanelButtons.Count; i < len; i++) {
				float button_x = left_panel_width + (i % curr_model.centerPanelColumns) * center_icon_width;
				float button_y = (3.0f/4.0f) * Screen.height + (i / 3) * icon_height;
				curr_model.centerPanelButtons[i].rect = new Rect(button_x, button_y, center_icon_width, icon_height);
				
			}
			curr_model.cached = true;
		}

//		GUI.BeginGroup(new Rect(0, (3.0f/4.0f) * Screen.height, Screen.width, panel_height));
		
		//draw left panel icons
		for (int i = 0, len = curr_model.leftPanelButtons.Count; i < len; i++) {
//			float button_x = (i % curr_model.leftPanelColumns) * left_icon_width;
//			float button_y = (i / 3) * icon_height;
			GUI.Button (curr_model.leftPanelButtons[i].rect, curr_model.leftPanelButtons[i].icon);
		}
		//draw center panel icons
		for (int i = 0, len = curr_model.centerPanelButtons.Count; i < len; i++) {
//			float button_x = left_panel_width + (i % curr_model.centerPanelColumns) * center_icon_width;
//			float button_y = (i / 3) * icon_height;
			GUI.Button (curr_model.centerPanelButtons[i].rect, curr_model.centerPanelButtons[i].icon);
		}
//		GUI.EndGroup();
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

	public static int [] GetButtonID(Vector3 mousePos) {
		int[] button = new int[2];
		Vector2 mousePos2D = new Vector2 (mousePos.x, Screen.height - mousePos.y);
		for(int i = 0; i < curr_model.leftPanelButtons.Count; i++) {
			if(curr_model.leftPanelButtons[i].rect.Contains(mousePos2D)) {
				button[0] = 0;
				button[1] = i;
				return button;
			}
		}

		for(int i = 0; i < curr_model.centerPanelButtons.Count; i++) {
			if(curr_model.centerPanelButtons[i].rect.Contains(mousePos2D)) {
				button[0] = 1;
				button[1] = i;
				return button;
			}
		}
		return null;
	}
}
