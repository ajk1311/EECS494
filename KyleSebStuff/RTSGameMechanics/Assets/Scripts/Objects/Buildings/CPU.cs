using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class CPU : DestructableBuilding {

	public Texture unit1_icon;
	public Texture unit2_icon;
	public Texture unit3_icon;

	private GUIModelManager.GUIModel CPUGuiModel;
	// Use this for initialization
	protected override void Start ()
	{
		base.Start ();
	}

	protected override RTS.GUIModelManager.GUIModel GetGUIModel ()
	{
		CPUGuiModel = new GUIModelManager.GUIModel ();
		CPUGuiModel.leftPanelButtons = new List<GUIModelManager.Button> ();
		GUIModelManager.Button button1 = new GUIModelManager.Button ();
		button1.icon = unit1_icon;
		button1.clicked += () => {Debug.Log ("Creating unit 1 from CPU");};
		CPUGuiModel.leftPanelButtons.Add (button1);

		GUIModelManager.Button button2 = new GUIModelManager.Button ();
		button2.icon = unit2_icon;
		button2.clicked += () => {Debug.Log ("Creating unit 2 from CPU");};
		CPUGuiModel.leftPanelButtons.Add (button2);

		GUIModelManager.Button button3 = new GUIModelManager.Button ();
		button3.icon = unit1_icon;
		button3.clicked += () => {Debug.Log ("Creating unit 3 from CPU");};
		CPUGuiModel.leftPanelButtons.Add (button3);

		CPUGuiModel.cached = false;
		return CPUGuiModel;
	}
}
