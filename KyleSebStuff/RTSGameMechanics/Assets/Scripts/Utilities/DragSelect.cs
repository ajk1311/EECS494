using UnityEngine;
using System.Collections;

public class DragSelect : MonoBehaviour {
	
	private GUIStyle m_DragStyle = new GUIStyle();
	
	private Vector2 m_DragLocationStart;
	private Vector2 m_DragLocationEnd;
	
	private bool m_CheckDeselect = false;
	private bool m_Dragging = false;
	int PlayerID = 1;
		
//	private float m_GuiWidth = 0;

	// Use this for initialization
	void Start () 
	{
		m_DragStyle.normal.background = TextureGenerator.MakeTexture (0.8f, 0.8f, 0.8f, 0.3f);
		m_DragStyle.border.bottom = 1;
		m_DragStyle.border.top = 1;
		m_DragStyle.border.left = 1;
		m_DragStyle.border.right = 1;
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (Input.GetMouseButtonDown (0))
		{
			m_DragLocationStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			m_CheckDeselect = true;
		}

		if (Input.GetMouseButtonUp (0))
		{
			m_CheckDeselect = false;
			m_Dragging = false;

			SelectionManager.selectedSpace = new Rect(0,0,0,0);
		}

		if (m_CheckDeselect)
		{
			if (Mathf.Abs (Input.mousePosition.x - m_DragLocationStart.x) > 2 && Mathf.Abs (Input.mousePosition.y-m_DragLocationStart.y) > 2)
			{
				m_CheckDeselect = false;
				m_Dragging = true;
				SelectionManager.deselectAllGameObjects(PlayerID);
			}
		}

	}
	
	void OnGUI()
	{
		if (m_Dragging)
		{
			m_DragLocationEnd = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			DragBox (m_DragLocationStart, m_DragLocationEnd, m_DragStyle);
		}
	}

	
	public void DragBox(Vector2 topLeft, Vector2 bottomRight, GUIStyle style)
	{
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

		SelectionManager.selectedSpace = rect;

		GUI.Box (rect, "", style);
	}

	public void CreateBoxOnGround(Rect rect) {

	}
}