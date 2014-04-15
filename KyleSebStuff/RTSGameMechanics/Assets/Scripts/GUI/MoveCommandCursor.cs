using UnityEngine;
using System.Collections;

public class MoveCommandCursor : MonoBehaviour {

    public GUISkin moveCommandSkin;
    public Texture2D[] moveCommandCursors;
    public Texture2D activeCursor;
    private int currentCursorFrame;
    // Use this for initialization
    void Start() {
        Invoke("Destroy", 2f);
    }

    void OnGUI() {
        if(renderer.isVisible)
            DrawMoveCursor();
    }

    private void UpdateCursorAnimation() {
        activeCursor = moveCommandCursors [(int)(Time.time * 10) % moveCommandCursors.Length];
    }

    private void DrawMoveCursor() {
        GUI.skin = moveCommandSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
        UpdateCursorAnimation();
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        Rect cursorPosition = new Rect(screenPosition.x, screenPosition.y, activeCursor.width, activeCursor.height);
        GUI.Label(cursorPosition, activeCursor);
        GUI.EndGroup();
    }

    void Destroy() {
        Destroy(this.gameObject);
    }
}
