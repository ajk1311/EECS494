using UnityEngine;
using System.IO;
using System.Collections;

public class AutoScreenshot : MonoBehaviour {

	public float SnapDelay = 1.0f;
	public string SegmentName = "segment";
	public float Aspect = 1.0f;
	public int SuperSize = 2;
	// Use this for initialization
	IEnumerator Start () {
		if(SnapDelay < 0.1f){
			this.SnapDelay = 0.1f;
			Debug.LogWarning("Snap delay cannot be less than 0.1 seconds, seeting it back to 0.1 seconds");
		}

		camera.enabled = true;

		var cameras = Camera.allCameras;
		var activeCameras = 0;
		for(int i = 0; i < cameras.Length; i++) {
			if(cameras[i].enabled) activeCameras++;
			if(activeCameras > 1){
				Debug.LogError("Disable all other cameras before running AutoSnapshot");
				yield break;
			}
		}

		camera.aspect = Aspect;

		var xHalfUnit = camera.orthographicSize * Aspect;
		var zHalfUnit = camera.orthographicSize;

		this.moveCam (xHalfUnit, zHalfUnit);

		var xInc = xHalfUnit * 2;
		var zInc = zHalfUnit * 2;

		var xTerrainMax = Terrain.activeTerrain.terrainData.size.x;
		var zTerrainMax = Terrain.activeTerrain.terrainData.size.z;
		Debug.Log ("zterrainMax value: " + zTerrainMax);

		Helper.CreateAssetFolderIfNotExists ("Minimap/Textures");
		for(float x = 0; x < xTerrainMax + xHalfUnit; x += xInc)
		{
			for(float z = 0; z < zTerrainMax + zHalfUnit; z += zInc)
			{
				this.moveCam(x,z);
				Application.CaptureScreenshot(string.Format("Assets/Minimap/Textures/{0}-{1}.{2}.png", x, z, SegmentName));
				yield return new WaitForSeconds(SnapDelay);
			}
		}

		using(var writer = new StreamWriter("Assets/Minimap/settings.txt")) {
			writer.WriteLine(string.Format("name=\"{0}\"", SegmentName));
			writer.WriteLine(string.Format("length=\"{0}\"", xInc));
			writer.WriteLine(string.Format("width=\"{0}\"", zInc));
			writer.WriteLine(string.Format("xMin=\"{0}\"", 0));
			writer.WriteLine(string.Format("xMax=\"{0}\"", xTerrainMax));
      	    writer.WriteLine(string.Format("zMin=\"{0}\"", 0));
        	writer.WriteLine(string.Format("zMax=\"{0}\"", zTerrainMax));

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void moveCam(float x, float z) {
		this.transform.position = new Vector3 (x, this.transform.position.y, z);

	}
}
