using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class GUIProgressBar : MonoBehaviour {

    // Variables
    public int progress;
    public int progressFull = 100;
    public float sizeX;
    public float sizeY;
    public int heightOffset = 2;
    public bool show;

    // Textures and GUIStyles
    public Texture2D progressBarForegroundFriendly;
    public Texture2D progressBarBackground;
    public Texture2D progressBarForegroundEnemy;
    public GUIStyle progressBackground;
    public GUIStyle progressForeground;

    // Constants
    private const int PROGRESS_BAR_X = 80;
    private const int PROGRESS_BAR_Y = 20;
    private const int HEALTH_BAR_X = 20;
    private const int HEALTH_BAR_Y = 5;

    void Awake() {
        progressBackground = new GUIStyle();
        progressForeground = new GUIStyle();
        show = false;
    }

    void OnGUI() {
        if (show) {
            Vector3 offsetPosition = new Vector3(transform.position.x, transform.position.y + heightOffset, transform.position.z);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(offsetPosition);
            Debug.Log(screenPosition);
            // draw the background:
            GUI.BeginGroup(new Rect (screenPosition.x - sizeX / 2, Screen.height - screenPosition.y, sizeX, sizeY));
            GUI.Label(new Rect (0, 0, sizeX, sizeY), "", progressBackground);

            // draw the filled-in part:
            GUI.BeginGroup(new Rect (0, 0, sizeX * progress / progressFull, sizeY));
            GUI.Label(new Rect (0, 0, sizeX * progress / progressFull, sizeY), "", progressForeground);
            GUI.EndGroup();

            GUI.EndGroup();
        }
    }

    public void initProgressBar(int initialProgress, string mode, bool friendly) {
        progressBarForegroundFriendly = (Texture2D) Resources.Load("GUISkins/ProgressBar/ProgressBarFriendly", typeof(Texture2D));
        progressBarBackground = (Texture2D) Resources.Load("GUISkins/ProgressBar/ProgressBackgroundBar", typeof(Texture2D));
        progressBarForegroundEnemy = (Texture2D) Resources.Load("GUISkins/ProgressBar/ProgressBarEnemy", typeof(Texture2D));
        progressBackground.normal.background = progressBarBackground;
        if (friendly)
            progressForeground.normal.background = progressBarForegroundFriendly;
        else
            progressForeground.normal.background = progressBarForegroundEnemy;

        if (mode == "Health") {
            sizeX = HEALTH_BAR_X;
            sizeY = HEALTH_BAR_Y;
        } else if (mode == "Progress") {
            sizeX = PROGRESS_BAR_X;
            sizeY = PROGRESS_BAR_Y;
        }
        progress = initialProgress;
    }
}
