using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS {
	public static class GUIModelManager{

		private static GUIModel currentGUIModel;

		public static GUIModel CurrentModel {
			get {
				return currentGUIModel;
			}
			set {
				if(currentGUIModel != null) {
					currentGUIModel.cached = false;
				}
				currentGUIModel = value;
			}
		}

		public class GUIModel {
			public bool cached = false;
			public int leftPanelColumns = 3;
			public int centerPanelColumns = 3;
			public List<Button> leftPanelButtons;
			public List<Button> centerPanelButtons;

			public GUIModel() {
				cached = false;
				leftPanelButtons = new List<Button>();
				centerPanelButtons = new List<Button>();
			}

			public void AddButton(int panel, Button button) {
				switch (panel) {
				case 0:
					leftPanelButtons.Add(button);
					break;
				case 1:
					centerPanelButtons.Add(button);
					break;
				}
			}
		}

		public delegate void OnClick();

		public class Button {
			public Texture icon;
			public event OnClick clicked;
			public Rect rect = new Rect (0, 0, 0, 0);

			public void Click() {
				if (clicked != null) clicked();
			}
		}

		public static void ExecuteClick(Vector3 position) {
			switch((int)position.x) {
				case 0:
					currentGUIModel.leftPanelButtons[(int)position.y].Click();
					break;
				case 1:
					currentGUIModel.centerPanelButtons[(int)position.y].Click();
					break;
			}
		}

	}
}
