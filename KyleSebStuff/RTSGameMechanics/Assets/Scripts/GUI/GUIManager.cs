using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class GUIManager : MonoBehaviour {

    public PlayerScript player;

    //Public GUI Skins to Initalize
    public GUISkin SELECT_BOX_SKIN;
    public GUISkin ORDERS_SKIN;
    public GUISkin RESOURCE_SKIN;

    //Resources
    public Texture2D POWER_ICON;
    public Texture2D MEMORY_ICON;

    //Initialization into GUIResources
    void Start() {
        player = this.gameObject.GetComponent<PlayerScript>();
        GUIResources.SELECT_BOX_SKIN = SELECT_BOX_SKIN;
    }
    
    void OnGUI() {
        DrawOrdersBar();
        DrawResourceBar();
    }
    
    /*** Private Worker Methods ***/
    private void DrawOrdersBar() {
        GUI.skin = ORDERS_SKIN;
        GUI.BeginGroup(new Rect(0, Screen.height - GUIResources.ORDERS_BAR_HEIGHT, Screen.width, GUIResources.ORDERS_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, Screen.width, GUIResources.ORDERS_BAR_HEIGHT), "");
        GUI.EndGroup();
    }
    
    private void DrawResourceBar() {
        GUI.skin = RESOURCE_SKIN;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, GUIResources.RESOURCE_BAR_HEIGHT));
        
        int topPos = 4, iconLeft = 4, textLeft = 2 * GUIResources.ICON_WIDTH;
        DrawResourceIcon(POWER_ICON, iconLeft, textLeft, topPos);
        iconLeft += GUIResources.TEXT_WIDTH;
        textLeft += GUIResources.TEXT_WIDTH;
        DrawResourceIcon(MEMORY_ICON, iconLeft, textLeft, topPos);
        
        GUI.EndGroup();
    }
    
    private void DrawResourceIcon(Texture2D resourceIcon, int iconLeft, int textLeft, int topPos) {
        string text;
        if (resourceIcon.Equals(POWER_ICON))
            text = player.getPower().ToString();
        else
            text = player.getMemory().ToString() + "/" + player.getMaxMemory().ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, GUIResources.ICON_WIDTH, GUIResources.ICON_HEIGHT), resourceIcon);
        GUI.Label(new Rect(textLeft, topPos, GUIResources.TEXT_WIDTH, GUIResources.TEXT_HEIGHT), text);
    }
}
