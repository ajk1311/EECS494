using UnityEngine;
using System.Collections.Generic;

namespace RTS {
    public static class GUIResources {
        //Playing Area Calculations
        public static int RESOURCE_BAR_HEIGHT = Screen.height / 20;
        public static int ORDERS_BAR_HEIGHT = Screen.height / 4;
        public static Rect PLAYING_AREA = new Rect(0, 0, Screen.width, Screen.height - ORDERS_BAR_HEIGHT);

		public static int TEXT_WIDTH = 128;
		public static int TEXT_HEIGHT = 32;
		public static int ICON_WIDTH = 32;
		public static int ICON_HEIGHT = 32;

		public static bool MouseInPlayingArea() {
			//Screen coordinates start in the lower-left corner of the screen
			//not the top-right of the screen like the drawing coordinates do
			Vector3 mousePos = Input.mousePosition;
			bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width;
			bool insideHeight = mousePos.y >= ORDERS_BAR_HEIGHT && mousePos.y <= Screen.height;
			return insideWidth && insideHeight;
		}

        //Select Box Skin
        public static GUISkin SELECT_BOX_SKIN;

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
            Bounds screenBounds = new Bounds(corners [0], Vector3.zero);
            for (int i=1; i<corners.Count; i++) {
                screenBounds.Encapsulate(corners [i]);
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