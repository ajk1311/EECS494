using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

namespace MyMinimap {
	public class MapManager : MonoBehaviour {
		GUIStyle	style;
		public		Transform   MainCam;
		public		GUIManager 	guiManager;
		public		Texture		mapFrame;
		public		Texture		navArrow;
		public		Texture		highlighter;
		public		Transform	NavTrail;
		public		Texture     mapTexture;
		public		Texture     miniMapSquare;
		public		Rect        miniMapRect;
		public		float       highlightCount;
		
		public float minX;
		public float minY;
		public float maxX;
		public float maxY;
		
		public float xRange;
		public float yRange;
		
		public float xCenter;
		public float yCenter;
		
		public float x1;
		public float y1;
		public float x2;
		public float y2;
		public float x3;
		public float y3;
		public float x4;
		public float y4;
		
		private static MapSettings mapSettings;
		
		private static float zoomMax;
		private static float zoomMin;
		private static float zoomVal;
		
		private static	List<MapMarker>	inRange;
		private static	List<MapMarker>	outRange;
		private static 	List<MapMarker> toRemove;
		private List<Transform> trailList;
		private static	Rect mapCenter;
		private static	float mapLength = 0;

		private float rangeUpdate = 1.0f;
		private float timer = 0.0f;
		private	Vector3	target;
		private	bool	navigating;
		
		private Camera orthographicCam;
	
		void Awake() {
			inRange = new List<MapMarker> ();
			outRange = new List<MapMarker> ();
			toRemove = new List<MapMarker> ();
		}
		// Use this for initialization
		void Start () {
			guiManager = GameObject.Find("Player").GetComponent<GUIManager>();
			MainCam = GameObject.Find("Main Camera").transform;
			miniMapRect = new Rect(0,0,11,11);
			trailList = new List<Transform> ();
			zoomMin = mapLength / 8;
			zoomMax = mapLength;
			
			
			maxY = Screen.height;
			maxX = Screen.width;
			minY = Screen.height*0.75f;
			minX = 0.68f * Screen.width;
			xRange = Screen.width/3.5f;
			yRange = Screen.height/4.4f;
			
			xCenter = Screen.width*0.69f + Screen.width/6.6f;
			yCenter = Screen.height*0.76f + Screen.height/8;
			
			mapCenter = new Rect (xCenter, yCenter-10, 20, 20);
			
			Time.fixedDeltaTime = 0.001f;
			style = new GUIStyle();
			style.fontSize = 25;
			style.normal.textColor = Color.white;

			drawPath ();
		}
	
		// Each update will check the dimensions of the screen
		// The boundaries of the minimap display will be adjusted accordingly
		// Every fixed amount of time (rangeUpdate), the program checks
		// whether markers are within the display boundaries of the minimap
		void Update () {
			maxY = Screen.height * 0.3f;
			maxX = Screen.width - 5;
			minY = 0;
			minX = 0.85f * Screen.width;
			
			timer += Time.deltaTime;
			if (timer > rangeUpdate) {
				updateRange();
				timer = 0;
			}
		}

		void checkMouseToMap() {
			if(Input.mousePosition.x > Screen.width*0.68 && Input.mousePosition.x < Screen.width*0.99) {
				if(Input.mousePosition.y > Screen.height*0.01 && Input.mousePosition.y < Screen.height*0.25) {
					guiManager.DrawMouseCursor();
					convertCursorToWorldCoordinate();
				}
			}
		}

		void convertCursorToWorldCoordinate() {
			double worldX = ((Input.mousePosition.x - Screen.width*0.71)/ (Screen.width*0.29f)) * 800; 
			double worldY = ((Input.mousePosition.y - Screen.height*0.00)/ (Screen.height*0.25f)) * 400;
			float currHeight = MainCam.position.y;
			if(Input.GetMouseButton(0)){
				MainCam.position = new Vector3((float)worldX + 10, currHeight, (float)worldY - 30);
			}
		}

		void OnGUI() {
			if (guiManager.gameLoading) {
				return;
			}
			// Iterate through the list of all markers that are within the display boundaries
			// of the minimap and draw them onto the map.
			DrawMap();
			checkMouseToMap();
			var miniMapSquarepos = convertToMinimapCoords(MainCam.transform.position);
			miniMapRect.x = miniMapSquarepos.x;
			miniMapRect.y = (miniMapSquarepos.y - 25) + ((100 - MainCam.transform.position.y)/100)*20;
			miniMapRect.height = (MainCam.transform.position.y/100) * 20;
			miniMapRect.width = (MainCam.transform.position.y/100) * 20;
			GUI.DrawTexture(miniMapRect, miniMapSquare, ScaleMode.StretchToFill);
			foreach (MapMarker marker in inRange) {
				WorldObject wo = marker.gameObject.GetComponent<WorldObject>();
				if (wo != null && wo.objectRenderer.enabled) {
					var pos = convertToMinimapCoords(marker.gameObject.transform.position);
					marker.rect.x = pos.x;
					marker.rect.y = pos.y;
					GUI.DrawTexture(marker.rect, marker.texture, ScaleMode.ScaleToFit);
					if (marker.shouldHighLight) {
						marker.timeElapsed += Time.deltaTime;
						// Debug.Log("iterateCount: " + marker.iterateCount);
						// Debug.Log("timeElapsed: " + marker.timeElapsed);
						if(marker.timeElapsed >= 0.25 && marker.iterateCount <= 4) {
							marker.timeElapsed = 0;
							marker.iterateCount++;
							marker.highlightRect.height = 100 - 22*marker.iterateCount;
							marker.highlightRect.width = 100 - 22*marker.iterateCount;
						}
						marker.highlightRect.x = pos.x - ((100 - 22*marker.iterateCount) / 2) + 3;
						marker.highlightRect.y = pos.y - ((100 - 22*marker.iterateCount) / 2) + 4;
						// Debug.Log("Iterate is: " + marker.iterateCount + " highlightRect size: " + marker.highlightRect.height);
						GUI.DrawTexture(marker.highlightRect, highlighter, ScaleMode.StretchToFill);
					}
				}
			}
			
			//Draw the zoom in and zoom out buttons
//			if(GUI.Button (new Rect (minX - 30, 20, 30, 30), "+"))
//				zoom (true);
//			if(GUI.Button (new Rect (minX - 30, 50, 30, 30), "-"))
//				zoom (false);
			
			GUI.DrawTexture (new Rect (minX,minY,10,yRange), mapFrame, ScaleMode.StretchToFill);
			GUI.DrawTexture (new Rect (minX,maxY,xRange,10), mapFrame, ScaleMode.StretchToFill);
			GUI.DrawTexture (new Rect (maxX,minY,10,10), mapFrame, ScaleMode.ScaleToFit);
			GUI.DrawTexture (new Rect (maxX,maxY,10,10), mapFrame, ScaleMode.ScaleToFit);

			// Draw the cursor at the center of the screen
			// The cursor points in the direction the player is facing
//			GUI.BeginGroup (mapCenter);
//			GUIUtility.RotateAroundPivot( currAngle(), new Vector2(10, 10));
//			GUI.DrawTexture (new Rect (0, 0, 20, 20), navArrow, ScaleMode.ScaleToFit);
//			GUI.EndGroup ();
		}

		private void DrawMap() {
        	GUI.DrawTexture(new Rect(Screen.width*0.7f, Screen.height*0.76f, Screen.width/3.3f, Screen.height/4.0f), mapTexture, ScaleMode.StretchToFill, true, 0.0f);
   		}
	
		// Allows the user to register transforms to be displayed on the minimap
		public void register(MapMarker marker) {
			inRange.Add (marker);
		}

		public void unregister(MapMarker marker) {
			inRange.Remove(marker);
			outRange.Remove(marker);
		}
	
		// Converts the coordinates of transforms from the game coordinates to the minimap coordinates
		public Vector2 convertToMinimapCoords(Vector3 pos) {
			// var xOffset = ((pos.x - transform.position.x) / (orthographicCam.orthographicSize*2)) * xRange;
			// var yOffset = ((pos.z - transform.position.z) / (orthographicCam.orthographicSize*2)) * yRange;
			var xOffset = ((pos.x - transform.position.x) / (800)) * xRange;
			var yOffset = ((pos.z - transform.position.z) / (400)) * yRange;
			return new Vector2(xCenter + xOffset,yCenter - yOffset);
		}

		//checks whether a given game coordinates is within the display boundaries of the minimap
		public bool isOnScreen(Vector3 pos) {
			// if (Mathf.Abs ((pos.x - transform.position.x) / (orthographicCam.orthographicSize*2)) > 0.5)
			// 	return false;
			// else if(Mathf.Abs ((pos.z - transform.position.z) / (orthographicCam.orthographicSize*2)) > 0.5)
			// 	return false;
			return true;
		}
	
		// checks all markers that are registered
		// markers within the display bounds of the minimap are moved into 'inRange'
		// markers beyond the display bounds of the minimap are moved into 'outRange'
		public void updateRange() {
			toRemove.Clear ();
			foreach(MapMarker marker in inRange) {
				if(!isOnScreen(marker.gameObject.transform.position)) {
					outRange.Add(marker);
					toRemove.Add (marker);
//					inRange.Remove(marker);
				}
			}
			foreach(MapMarker marker in toRemove) {
				inRange.Remove(marker);
			}
			toRemove.Clear ();
			foreach(MapMarker marker in outRange) {
				if(isOnScreen(marker.gameObject.transform.position)) {
					inRange.Add(marker);
					toRemove.Add(marker);
//					outRange.Remove(marker);
				}
			}
			foreach(MapMarker marker in toRemove) {
				outRange.Remove(marker);
			}
			toRemove.Clear ();
		}
	
		// get the current rotation of the player
		public float currAngle() {
			return transform.eulerAngles.y;
		}
	
		//zooms in and out of the minimap
		public void zoom(bool zoomIn) {
			float camZoom = orthographicCam.orthographicSize;

			if(zoomIn && orthographicCam.orthographicSize > zoomMin){
				orthographicCam.orthographicSize = Mathf.Lerp(camZoom, camZoom - mapLength/8, Time.time);
				foreach(Transform trailPiece in trailList){
					trailPiece.localScale = new Vector3(trailPiece.localScale.x, 10*(orthographicCam.orthographicSize/mapLength), 1);
				}
			} else if(!zoomIn && orthographicCam.orthographicSize < zoomMax) {
				orthographicCam.orthographicSize = Mathf.Lerp(camZoom, camZoom + mapLength/8, Time.time);
				foreach(Transform trailPiece in trailList){
					trailPiece.localScale = new Vector3(trailPiece.localScale.x, 10*(orthographicCam.orthographicSize/mapLength), 1);
				}
			}
		}

		// the user calls this function to set a GPS destination,
		// this function uses the built-in path-finding algorithm
		public void setDestination(Vector3 destination) {
			navigating = true;
			target = destination;
			drawPath ();
		}

		// the user calls this function to set a GPS destination,
		// the user provides an array of coordinates for the path
		public void setDestination(Vector3[] path) {
			navigating = true;
			target = path [path.Length];
			drawPath (path);
		}

		// 
		public void drawPath() {
			clearDestination ();
			NavMeshPath path = new NavMeshPath ();
			NavMesh.CalculatePath (transform.position, target, -1, path);
			Vector3 secondToLast;
			if (path.corners.Length == 0) {
				Debug.Log("NAV ERROR: CANNOT REACH DESTINATION");
			}
			for(int i = 0; i < path.corners.Length; i++) {
				if(i < path.corners.Length - 1) {
					var currNode = path.corners[i];
					var nextNode = path.corners[i+1];
					var newTrailPiece = Instantiate(NavTrail) as Transform;
					newTrailPiece.position = new Vector3((currNode.x + nextNode.x)/2, 201, (currNode.z + nextNode.z)/2);
					newTrailPiece.eulerAngles = new Vector3(90, getRotation(currNode, nextNode), 0);
					newTrailPiece.localScale = new Vector3(Vector2.Distance(new Vector2(currNode.x, currNode.z), new Vector2(nextNode.x, nextNode.z)), 10*(orthographicCam.orthographicSize/mapLength), 1);
					secondToLast = nextNode;
					trailList.Add (newTrailPiece);
				} else {
					var lastTrailPiece = Instantiate(NavTrail) as Transform;
					lastTrailPiece.position = new Vector3((path.corners[i].x + target.x)/2, 201, (path.corners[i].z + target.z)/2);
					lastTrailPiece.eulerAngles = new Vector3(90, getRotation(path.corners[i], target), 0);
					lastTrailPiece.localScale = new Vector3(Vector2.Distance(new Vector2(path.corners[i].x, path.corners[i].z), new Vector2(target.x, target.z)), 10*(orthographicCam.orthographicSize/mapLength), 1);
					trailList.Add(lastTrailPiece);
				}
			}
		}

		// this function is called if the user used a custom nav algorithm
		// and provided their own array of coordinates for the path to be drawn
		public void drawPath(Vector3[] path){
			for(int i = 0; i < path.Length; i++) {
					var currNode = path[i];
					var nextNode = path[i+1];
					var newTrailPiece = Instantiate(NavTrail) as Transform;
					newTrailPiece.position = new Vector3((currNode.x + nextNode.x)/2, 201, (currNode.z + nextNode.z)/2);
					newTrailPiece.eulerAngles = new Vector3(90, getRotation(currNode, nextNode), 0);
					newTrailPiece.localScale = new Vector3(Vector2.Distance(new Vector2(currNode.x, currNode.z), new Vector2(nextNode.x, nextNode.z)), 10*(orthographicCam.orthographicSize/mapLength), 1);
					trailList.Add (newTrailPiece);
				}
		}

		// cancels a destination the user has specified
		public void clearDestination(){
			foreach (Transform trailPiece in trailList) {
				Destroy (trailPiece.gameObject);
			}
			trailList.Clear ();
		}

		// gets the angle by which the trail prefab component shold be
		// rotated by in order to connected between two points along the path
		public float getRotation(Vector3 currNode, Vector3 nextNode) {
			if (nextNode.x == currNode.x)
				return 90f;
			else {
				return -(Mathf.Atan((nextNode.z - currNode.z) / (nextNode.x - currNode.x)))*(180/Mathf.PI);
			}
		}

		// returns a list of all markers that have been registered to the minimap
		public List<MapMarker> getRegisteredMarkers() {
			List<MapMarker> allMarkers = new List<MapMarker> ();
			foreach(MapMarker marker in outRange) {
				allMarkers.Add(marker);
			}
			foreach(MapMarker marker in inRange) {
				allMarkers.Add(marker);
			}

			return allMarkers;
		}


		// retrieves the length of each map segment for scaling purposes
		public void getMapLength() {
			// GameObject OHC = GameObject.Find ("OHC");
			// mapLength = OHC.GetComponent<MapLoader> ().getMapLength()/2;
		}
	}
}
