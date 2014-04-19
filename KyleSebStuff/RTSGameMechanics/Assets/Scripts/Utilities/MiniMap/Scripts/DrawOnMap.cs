using UnityEngine;
using System.Collections;

public class DrawOnMap : MonoBehaviour {
	GUIStyle	style;
	public		Texture		marker;
	public		Texture		timeMeter;
	// Use this for initialization
	void Start () {
		Time.fixedDeltaTime = 0.001f;
		style = new GUIStyle();
		style.fontSize = 25;
		style.normal.textColor = Color.white;
	}
	
	// Update is called once per frame
	void Update () {

	}
	void OnFixedUpdate() {
		
		//timerString = timeLeft.ToString () + "sec";
	}
	
	void OnGUI() {
		//		Debug.Log ("calling OnGUI()");

		GUI.Label(new Rect (Screen.width - 150,25,100,50), "hello", style);
		GUI.DrawTexture (new Rect (Screen.width - 150,25,10,10), marker, ScaleMode.ScaleToFit);
	}
	
	public void setCurrentHigh(float currHigh) {
//		highScore = currHigh;
	}
}
