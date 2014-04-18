using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MyMinimap {
	public class MapManager : MonoBehaviour {
		GUIStyle	style;
		public		Texture		mapFrame;
		public		Texture		navArrow;
		public		Texture		highlighter;
		public		Transform	NavTrail;
		
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
		private static	float mapLength;

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
			getMapLength ();
			trailList = new List<Transform> ();
			zoomMin = mapLength / 8;
			zoomMax = mapLength;
			orthographicCam = GameObject.Find ("OHC").camera;
			orthographicCam.aspect = 1;
			
			maxY = Screen.height;
			maxX = Screen.width;
			minY = Screen.height*0.77f;
			minX = 0.71f * Screen.width;
			xRange = maxX - minX;
			yRange = maxY - minY;
			
			xCenter = minX + (xRange / 2);
			yCenter = minY + (yRange / 2);
			
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
			maxY = Screen.height*0.3f;
			maxX = Screen.width - 5;
			minY = 0;
			minX = 0.85f * Screen.width;
			
			timer += Time.deltaTime;
			if (timer > rangeUpdate) {
				updateRange();
				timer = 0;
			}
		}
	

		void OnGUI() {
			// Iterate through the list of all markers that are within the display boundaries
			// of the minimap and draw them onto the map.
			foreach(MapMarker marker in inRange) {
				var pos = convertToMinimapCoords(marker.gameObject.transform.position);
				marker.rect.x = pos.x;
				marker.rect.y = pos.y;
				GUI.DrawTexture(marker.rect, marker.texture, ScaleMode.ScaleToFit);
				if(marker.shouldHighLight){
					GUI.DrawTexture(marker.rect, highlighter, ScaleMode.StretchToFill);
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
	
		// Allows the user to register transforms to be displayed on the minimap
		public void register(MapMarker marker) {
			inRange.Add (marker);
		}
	
		// Converts the coordinates of transforms from the game coordinates to the minimap coordinates
		public Vector2 convertToMinimapCoords(Vector3 pos) {
			var xOffset = ((pos.x - transform.position.x) / (orthographicCam.orthographicSize*2)) * xRange;
			var yOffset = ((pos.z - transform.position.z) / (orthographicCam.orthographicSize*2)) * yRange;
			return new Vector2(xCenter + xOffset,yCenter - yOffset);
		}

		//checks whether a given game coordinates is within the display boundaries of the minimap
		public bool isOnScreen(Vector3 pos) {
			if (Mathf.Abs ((pos.x - transform.position.x) / (orthographicCam.orthographicSize*2)) > 0.5)
				return false;
			else if(Mathf.Abs ((pos.z - transform.position.z) / (orthographicCam.orthographicSize*2)) > 0.5)
				return false;
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
			}

			else if(!zoomIn && orthographicCam.orthographicSize < zoomMax) {
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
				}
				else {
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
			GameObject OHC = GameObject.Find ("OHC");
			mapLength = OHC.GetComponent<MapLoader> ().getMapLength()/2;
			Debug.Log ("map length is: " + mapLength);
		}
	}
}
