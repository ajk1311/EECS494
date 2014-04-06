using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class GUIManager : MonoBehaviour {

    public PlayerScript player;

    //Public GUI Skins to Initalize
    public GUISkin SELECT_BOX_SKIN;
    public GUISkin ORDERS_SKIN;
    public GUISkin RESOURCE_SKIN;

    //Resources
    public Texture2D POWER_ICON;
    public Texture2D MEMORY_ICON;

	//Dragging GUI variables
	private GUIStyle m_DragStyle = new GUIStyle();
	private Vector2 m_DragLocationStart;
	private Vector2 m_DragLocationEnd;
	private bool m_CheckDeselect = false;
	private static bool s_Dragging = false;

	public static bool Dragging {
		get { return s_Dragging; }
	}

    //Initialization into GUIResources
    void Start() {
        player = gameObject.GetComponent<PlayerScript>();
        GUIResources.SELECT_BOX_SKIN = SELECT_BOX_SKIN;

		//Initialize the Dragging Variables
		m_DragStyle.normal.background = TextureGenerator.MakeTexture(0.8f, 0.8f, 0.8f, 0.3f);
		m_DragStyle.border.bottom = 1;
		m_DragStyle.border.top = 1;
		m_DragStyle.border.left = 1;
		m_DragStyle.border.right = 1;
    }
    
    void OnGUI() {
        DrawOrdersBar();
        DrawResourceBar();

		if (s_Dragging) {
			m_DragLocationEnd = new Vector2(
				Mathf.Min(Mathf.Max(Input.mousePosition.x, 4), Screen.width - 4), 
				Mathf.Min(Mathf.Max(Input.mousePosition.y, GUIResources.ORDERS_BAR_HEIGHT + 4), Screen.height - 4));
			DragBox(m_DragLocationStart, m_DragLocationEnd, m_DragStyle);
		}
    }
    
    /*** Private Worker Methods ***/
    private void DrawOrdersBar() {
        GUI.skin = ORDERS_SKIN;
        GUI.BeginGroup(new Rect(0, Screen.height - GUIResources.ORDERS_BAR_HEIGHT, Screen.width, GUIResources.ORDERS_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, Screen.width, GUIResources.ORDERS_BAR_HEIGHT), "");
        GUI.EndGroup();
    }
    
    private void DrawResourceBar() {
        GUI.skin = RESOURCE_SKIN;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, GUIResources.RESOURCE_BAR_HEIGHT));
        
        int topPos = 4, iconLeft = 4, textLeft = 2 * GUIResources.ICON_WIDTH;
        DrawResourceIcon(POWER_ICON, iconLeft, textLeft, topPos);
        iconLeft += GUIResources.TEXT_WIDTH;
        textLeft += GUIResources.TEXT_WIDTH;
        DrawResourceIcon(MEMORY_ICON, iconLeft, textLeft, topPos);
        
        GUI.EndGroup();
    }
    
    private void DrawResourceIcon(Texture2D resourceIcon, int iconLeft, int textLeft, int topPos) {
        string text;
        if (resourceIcon.Equals(POWER_ICON))
            text = player.getPower().ToString();
        else
            text = player.getMemory().ToString() + "/" + player.getMaxMemory().ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, GUIResources.ICON_WIDTH, GUIResources.ICON_HEIGHT), resourceIcon);
        GUI.Label(new Rect(textLeft, topPos, GUIResources.TEXT_WIDTH, GUIResources.TEXT_HEIGHT), text);
    }

	void Update() {
		checkIfDragging ();
	}

	private void checkIfDragging() {
		if (Input.GetMouseButtonDown (0))
		{
			m_DragLocationStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			m_CheckDeselect = true;
		}
		
		if (Input.GetMouseButtonUp (0))
		{
			m_CheckDeselect = false;
			s_Dragging = false;
		}
		
		if (m_CheckDeselect)
		{
			if (Mathf.Abs (Input.mousePosition.x - m_DragLocationStart.x) > 2 && Mathf.Abs (Input.mousePosition.y-m_DragLocationStart.y) > 2)
			{
				m_CheckDeselect = false;
				s_Dragging = true;
			}
		}
	}

	public void DragBox(Vector2 topLeft, Vector2 bottomRight, GUIStyle style) {
		float minX = Mathf.Max (topLeft.x, bottomRight.x);
		float maxX = Mathf.Min (topLeft.x, bottomRight.x);
		
		float minY = Mathf.Max (Screen.height-topLeft.y, Screen.height-bottomRight.y);
		float maxY = Mathf.Min (Screen.height-topLeft.y, Screen.height-bottomRight.y);
		
		Rect rect = new Rect(minX, minY, maxX-minX, maxY-minY);
		
		//Don't let the dragged area interfere with the gui
		//		if (rect.xMin > Screen.width-m_GuiWidth)
		//		{
		//			rect.xMin = Screen.width-m_GuiWidth;
		//		}
		//		
		//		if (rect.xMax > Screen.width-m_GuiWidth)
		//		{
		//			rect.xMax = Screen.width-m_GuiWidth;
		//		}

		GUI.Box (rect, "", style);
	}
}
