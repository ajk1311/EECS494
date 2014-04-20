using UnityEngine;
using System.Collections.Generic;

namespace RTS {
    public static class GUIResources {

		// This is the standard screen size we base all our GUI dimension calculations on.
		// Any would-be pixel values must first be scaled based on this size.
		public static readonly Vector2 StandardScreen = new Vector2(1024, 768);

		public static float GetScaledPixelSize(float px) {
			float scaleX = Screen.width / StandardScreen.x;
			float scaleY = Screen.height / StandardScreen.y;
			if (Mathf.Abs(scaleX - scaleY) > 0.1f) {
				throw new System.Exception("Something went horribly wrong with screen sizes!");
			}
			return px * scaleX;
		}

        // By specifying pixel sizes for GUI at the default camera height,
        // we can scale such pixels so they are consistent when we zoom in and out
        public static float GetZoomedPixelSize(float px) {
            float scale = Camera.main.transform.position.y / 70;
            return px * scale;
        }

        //Playing Area Calculations
        public static int ResourceBarHeight {
			get { return Screen.height / 20; }
		}
        public static int OrdersBarHeight {
			get { return Screen.height / 4; }
		}
        public static Rect PlayingArea {
			get { return new Rect(0, 0, Screen.width, Screen.height - OrdersBarHeight); }
		}

		public static float TextWidth = GetScaledPixelSize(128);
		public static float TextHeight = GetScaledPixelSize(32);
		public static float IconWidth = GetScaledPixelSize(32);
		public static float IconHeight = GetScaledPixelSize(32);

		public static bool MouseInPlayingArea() {
			//Screen coordinates start in the lower-left corner of the screen
			//not the top-left of the screen like the drawing coordinates do
			Vector3 mousePos = Input.mousePosition;
			bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width;
			bool insideHeight = mousePos.y >= OrdersBarHeight && mousePos.y <= Screen.height;
			return insideWidth && insideHeight;
		}

        //Select Box Skin
        public static GUISkin SelectBoxSkin;

        //Calculating the Selection Box Bounds
        public static Rect CalculateSelectionBox(Bounds selectionBounds) {
            //shorthand for the coordinates of the centre of the selection bounds
            float cx = selectionBounds.center.x;
            float cy = selectionBounds.center.y;
            float cz = selectionBounds.center.z;
            //shorthand for the coordinates of the extents of the selection bounds
            float ex = selectionBounds.extents.x;
            float ey = selectionBounds.extents.y;
            float ez = selectionBounds.extents.z;
            
            //Determine the screen coordinates for the corners of the selection bounds
            List<Vector3> corners = new List<Vector3>();
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)));
            corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz - ez)));
            
            //Determine the bounds on screen for the selection bounds
            Bounds screenBounds = new Bounds(corners[0], Vector3.zero);
            for (int i=1; i<corners.Count; i++) {
                screenBounds.Encapsulate(corners[i]);
            }
            
            //Screen coordinates start in the bottom right corner, rather than the top left corner
            //this correction is needed to make sure the selection box is drawn in the correct place
            float selectBoxTop = Screen.height - (screenBounds.center.y + screenBounds.extents.y);
            float selectBoxLeft = screenBounds.center.x - screenBounds.extents.x;
			float selectBoxWidth = 2 * screenBounds.extents.x;
			float selectBoxHeight = 2 * screenBounds.extents.y;
            
            return new Rect(selectBoxLeft, selectBoxTop, selectBoxWidth, selectBoxHeight);
        }
    }
}