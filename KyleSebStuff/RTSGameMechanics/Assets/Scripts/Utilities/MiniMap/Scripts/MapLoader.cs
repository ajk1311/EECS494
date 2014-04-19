using UnityEngine;
using System;
using System.Collections;

using MyMinimap;

public class MapLoader : MonoBehaviour, IMapLoader
{

	public Transform player;

	MapHandler mapHandler;
	float mapLength;
	float mapCheck = 5f;
	float timer = 0;

	void Awake() {
		var bundle = AssetBundle.CreateFromFile (string.Format ("{0}/{1}", System.IO.Directory.GetCurrentDirectory (), "mapData.dat"));
		
		var settingsData = bundle.mainAsset as TextAsset;
		if(settingsData == null) {
			Debug.LogError("settings file cannot be found inside the bundle");
			return;
		}
		
		var mapSettings = new MapSettings (settingsData.text);
		mapLength = mapSettings.length;
		this.mapHandler = new MapHandler (this, bundle, mapSettings, LayerMask.NameToLayer("Map"));
	}

	void Start() {
		this.moveCam (player.position);
		this.mapHandler.Start (player.position);

	}

	void moveCam(Vector3 position) {
		this.transform.position = new Vector3 (position.x, transform.position.y, position.z);
	}

	public float getMapLength() {
		return mapLength;
	}

	public void Unload() {
		this.mapHandler.Unload ();
	}

	void Update() {
		this.moveCam (player.position);
		this.timer += Time.deltaTime;

		if(timer > mapCheck) {
			//this.mapHandler.UpdateMap(player.position);
			this.timer = 0;
		}
	}
		
	public void StartAsyncMethod(IEnumerator method) {
		this.StartCoroutine(method);
	}
}


