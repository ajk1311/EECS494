using UnityEngine;
using System.Collections.Generic;

namespace RTS {
    public static class GUIResources {
        //Playing Area Calculations
        private static int RESOURCE_BAR_HEIGHT = 0;
        private static int ORDERS_BAR_HEIGHT = 0;
        private static Rect PLAYING_AREA = new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width, Screen.height - RESOURCE_BAR_HEIGHT - ORDERS_BAR_HEIGHT);

        public static Rect GetPlayingArea() {
            return PLAYING_AREA;
        }

        //Select Box Skin
        private static GUISkin SELECT_BOX_SKIN;

        public static GUISkin getSelectBoxSkin { get { return SELECT_BOX_SKIN; } }

        public static void setSelectBoxSkin(GUISkin skin) {
            SELECT_BOX_SKIN = skin;
        }

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
            float selectBoxTop = PLAYING_AREA.height - (screenBounds.center.y + screenBounds.extents.y);
            float selectBoxLeft = screenBounds.center.x - screenBounds.extents.x;
            float selectBoxWidth = 2 * screenBounds.extents.x;
            float selectBoxHeight = 2 * screenBounds.extents.y;
            
            return new Rect(selectBoxLeft, selectBoxTop, selectBoxWidth, selectBoxHeight);
        }
    }
}