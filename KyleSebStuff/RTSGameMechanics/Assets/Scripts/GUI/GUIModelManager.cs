using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS {
	public static class GUIModelManager {

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
		
		private static List<GUIModel> sModels;

		public static void Init() {
			sModels = new List<GUIModel>();
			sModels.Add(null);
			sModels.Add(null);
		}

		public static GUIModel GetCurrentModel(int playerID) {
			return sModels != null ? sModels[playerID - 1] : null;
		}

		public static void SetCurrentModel(int playerID, GUIModel model) {
			sModels[playerID - 1] = model;
		}

		public static void ExecuteClick(int playerID, Vector3 position) {
			GUIModel currentModel = sModels[playerID - 1];
			if (currentModel == null) {
				return;
			}
			switch((int)position.x) {
			case 0:
				currentModel.leftPanelButtons[(int)position.y].Click();
				break;
			case 1:
				currentModel.centerPanelButtons[(int)position.y].Click();
				break;
			}
		}

	}
}
