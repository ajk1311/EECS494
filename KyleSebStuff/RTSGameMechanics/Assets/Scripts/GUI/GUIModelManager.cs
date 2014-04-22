using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RTS {
	public static class GUIModelManager {

		public class GUIModel {
			public static readonly int MaxColumns = 3;

			public int leftPanelColumns = MaxColumns;
			public List<Button> leftPanelButtons;
			public string leftPanelTitle;

			public int centerPanelColumns = MaxColumns;
			public List<Button> centerPanelButtons;
			public string centerPanelTitle;

			public GUIModel() {
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

			public void ClearButtons(int panel) {
				switch(panel) {
					case 0:
						leftPanelButtons.Clear();
						break;

					case 1:
						centerPanelButtons.Clear();
						break;
				}
			}
		}

		public delegate void OnClick();

		public class Button {
			public string text;
			public string hint;
			public Texture icon;
			public bool enabled = true;
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
