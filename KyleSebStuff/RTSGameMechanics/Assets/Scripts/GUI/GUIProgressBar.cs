using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class GUIProgressBar : MonoBehaviour {

    public int progress;
    public int progressFull = 1000;
    public float sizeX = 100;
    public float sizeY = 25;
    public int heightOffset = 6;
    private bool show = false;

    public Texture2D progressBarForeground;
    public Texture2D progressBarBackground;

    void OnGUI() {
        if (gameObject.GetComponentInChildren<Renderer>().isVisible && show) {
            Vector3 offsetPosition = new Vector3(transform.position.x, transform.position.y + heightOffset, transform.position.z);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(offsetPosition);
            // draw the background:
            GUI.BeginGroup (new Rect (screenPosition.x, Screen.height - screenPosition.y,
                                      GUIResources.GetZoomedPixelSize(sizeX), GUIResources.GetZoomedPixelSize(sizeY)));
            GUI.Label(new Rect (0, 0, GUIResources.GetZoomedPixelSize(sizeX),
                           GUIResources.GetZoomedPixelSize(sizeY)), (Texture) progressBarBackground);

            // draw the filled-in part:
            GUI.BeginGroup (new Rect (0, 0, GUIResources.GetZoomedPixelSize(sizeX * progress / progressFull),
                                      GUIResources.GetZoomedPixelSize(sizeY)));
            GUI.Label(new Rect (0, 0, GUIResources.GetZoomedPixelSize(sizeX * progress / progressFull),
                           GUIResources.GetZoomedPixelSize(sizeY)), (Texture) progressBarForeground);
            GUI.EndGroup ();

            GUI.EndGroup ();
        }

    }

    public void startProgressBar(int initialProgress) {
        progressBarForeground = (Texture2D) Resources.Load("GUISkins/ProgressBar/ProgressMovingBar", typeof(Texture2D));
        progressBarBackground = (Texture2D) Resources.Load("GUISkins/ProgressBar/ProgressBackgroundBar", typeof(Texture2D));
        show = true;
        progress = initialProgress;
    }

    public void finishProgressBar() {
        show = false;
        progress = 0;
    }
}
