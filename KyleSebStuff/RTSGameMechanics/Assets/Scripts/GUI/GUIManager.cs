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
        DrawResourceBar();

		if (sDragging) {
			float padding = GUIResources.GetScaledPixelSize(4);
			mDragLocationEnd = new Vector2(
				Mathf.Min(Mathf.Max(Input.mousePosition.x, padding), Screen.width - padding), 
				Mathf.Min(Mathf.Max(Input.mousePosition.y, GUIResources.OrderesBarHeight + padding), Screen.height - padding));
			DragBox(mDragLocationStart, mDragLocationEnd, mDragStyle);
		}
    }
    
    /*** Private Worker Methods ***/
    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(0, Screen.height - GUIResources.OrderesBarHeight, Screen.width, GUIResources.OrderesBarHeight));
        GUI.Box(new Rect(0, 0, Screen.width, GUIResources.OrderesBarHeight), "");
        GUI.EndGroup();
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
}
