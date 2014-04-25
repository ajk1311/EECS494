using UnityEngine;
using System.Collections;

using System;

namespace MyMinimap
{
	public class MapHandler
	{
		#region Constants and Fields

		private readonly IMapLoader mapLoader;
		private readonly AssetBundle bundle;
		private readonly MapSettings mapSettings;

		private readonly MapSegment[,] loadedSegments = new MapSegment[2,2];

		private readonly int mapLayer;

		Vector3 mapOffset;
		Rect mapBounds;

		#endregion

		#region Properties

		MapSegment bottomLeft {
			get { return this.loadedSegments [0, 0]; }
		}

		MapSegment bottomRight {
			get { return this.loadedSegments [1, 0]; }
		}

		MapSegment topLeft {
			get { return this.loadedSegments [0, 1]; }
		}

		MapSegment topRight {
			get { return this.loadedSegments [1, 1]; }
		}

		#endregion

		#region Constructors and Destructors
		public MapHandler(IMapLoader mapLoader, AssetBundle mapAsset, MapSettings mapSettings, int mapLayer) {
			this.mapLoader = mapLoader;
			this.bundle = mapAsset;
			this.mapSettings = mapSettings;
			this.mapLayer = mapLayer;
			
			this.mapOffset = new Vector3 (mapSettings.length / 2, 200, mapSettings.width / 2);
			this.mapBounds = new Rect ();
		}
		#endregion

		#region Public Methods

		public void Start(Vector3 position) {
			this.PrepareMapAt (position);
		}

		public void UpdateMap(Vector3 position) {
			this.updateMapAt (position);
		}

		public void Unload() {
			this.bundle.Unload (true);
		}

		#endregion

		#region Local Methods

		void PrepareMapAt(Vector3 pos) {
			this.mapBounds = this.GetMapBoundForPos(pos);
			this.loadedSegments [0, 0] = new MapSegment () { gameObject = LoadAndCreateSegmentAt(mapBounds.xMin, mapBounds.yMin), state = SegmentState.Active };
			this.loadedSegments [1, 0] = new MapSegment () { gameObject = LoadAndCreateSegmentAt(mapBounds.xMax, mapBounds.yMin), state = SegmentState.Active };
			this.loadedSegments [0, 1] = new MapSegment () { gameObject = LoadAndCreateSegmentAt(mapBounds.xMin, mapBounds.yMax), state = SegmentState.Active };
			this.loadedSegments [1, 1] = new MapSegment () { gameObject = LoadAndCreateSegmentAt(mapBounds.xMax, mapBounds.yMax), state = SegmentState.Active };

			for (int i = mapSettings.xMin; i < mapSettings.xMax; i += mapSettings.length) {
				for(int j = mapSettings.zMin; j < mapSettings.zMax; j += mapSettings.width) {
					LoadAndCreateSegmentAt(i, j);
				}
			}
		}

		void updateMapAt(Vector3 pos) {
			var newMapBounds = this.GetMapBoundForPos (pos);

			bool changed = false;

			if (newMapBounds.xMin < mapBounds.xMin) {
				this.topRight.Replace(topLeft);
				this.bottomRight.Replace(bottomLeft);
				this.topLeft.Reset();
				this.bottomLeft.Reset();

				changed = true;
			}
			else if(newMapBounds.xMax > mapBounds.xMax) {
				this.topLeft.Replace(topRight);
				this.bottomLeft.Replace(bottomRight);
				this.topRight.Reset();
				this.bottomRight.Reset();

				changed = true;
			}

			if (newMapBounds.yMin < mapBounds.yMin) {
				this.topLeft.Replace(bottomLeft);
				this.topRight.Replace(bottomRight);
				this.bottomLeft.Reset();
				this.bottomRight.Reset();
			}
			else if(newMapBounds.yMax > mapBounds.yMax) {
				this.bottomLeft.Replace(topLeft);
				this.bottomRight.Replace(topRight);
				this.topLeft.Reset();
				this.topRight.Reset();
				
				changed = true;
			}

			if(changed) {
				this.mapBounds = newMapBounds;
				
				this.HandleSegmentAt(bottomLeft, mapBounds.xMin, mapBounds.yMin);
				this.HandleSegmentAt(bottomRight, mapBounds.xMax, mapBounds.yMin);
				this.HandleSegmentAt(topLeft, mapBounds.xMin, mapBounds.yMax);
				this.HandleSegmentAt(topRight, mapBounds.xMax, mapBounds.yMax);
			}
		}

		void HandleSegmentAt(MapSegment segment, float x, float z) {
			if(segment.state == SegmentState.Destroyed) {
				this.mapLoader.StartAsyncMethod(WaitUntilSegmentLoadAt(segment, x , z));
			}
		}

		IEnumerator WaitUntilSegmentLoadAt(MapSegment segment, float x, float z) {
			segment.state = SegmentState.Loading;

			var segCoordPos = GetSegmentCoordForPos (x, z);
			var asyncRequest = this.LoadSegmentAsyncAt ((int)segCoordPos.x, (int)segCoordPos.y);

			yield return asyncRequest;

			segment.gameObject = CreateSegmentAt (segCoordPos, asyncRequest.asset as GameObject);
			segment.state = SegmentState.Active;
		}

		AssetBundleRequest LoadSegmentAsyncAt(int x, int z) {
			return this.bundle.LoadAsync (string.Format ("{0}-{1}.{2}", x, z, mapSettings.segmentName), typeof(GameObject));
		}

		Rect GetMapBoundForPos(Vector3 pos) {
			return this.GetMapBoundForPos(pos.x, pos.z);
		}

		Rect GetMapBoundForPos(float x, float z) {
			var currSegCoord = this.GetSegmentCoordForPos(x, z);

			Rect bounds = new Rect(currSegCoord.x, currSegCoord.y, mapSettings.length, mapSettings.width);

			if(bounds.xMax > mapSettings.xMax) {
				var val = bounds.xMax - mapSettings.xMax;
				
				bounds.xMin = this.GetSegmentCoordForPos(bounds.xMin - val, z).x;
				bounds.xMax = this.GetSegmentCoordForPos(bounds.xMax - val, z).x;
			}

			else if( x < currSegCoord.x) {
				bounds.xMin -= mapSettings.length;
				bounds.xMax -= mapSettings.length;
			}

			if(bounds.yMax > mapSettings.zMax) {
				var val = bounds.yMax - mapSettings.zMax;
				
				bounds.yMin = this.GetSegmentCoordForPos(x, bounds.yMin - val).y;
				bounds.yMax = this.GetSegmentCoordForPos(x, bounds.yMax - val).y;
			}
			
			else if( x < currSegCoord.x) {
				bounds.yMin -= mapSettings.width;
				bounds.yMax -= mapSettings.width;
			}

			return bounds;
		}
		
		Vector2 GetSegmentCoordForPos(Vector3 pos) {
			return this.GetSegmentCoordForPos(pos.x, pos.z);
		}
		Vector2 GetSegmentCoordForPos(float x, float z) {
//			Debug.Log ("input x: " + x + " input y: " + z);
			var nX = Mathf.Clamp (x + mapOffset.x, mapSettings.xMin, mapSettings.xMax);
			var nZ = Mathf.Clamp (z + mapOffset.z, mapSettings.zMin, mapSettings.zMax);
//			var nX = x + mapOffset.x;
//			var nZ = z + mapOffset.z;
			
			var pX = (int)(nX / mapSettings.length);
			var pZ = (int)(nZ / mapSettings.width);

			var aX = pX * mapSettings.length;
			var aZ = pZ * mapSettings.width;

//			Debug.Log ("output x: " + aX + " output y: " + aZ);

			return new Vector2(aX, aZ);
		}

		GameObject CreateSegmentAt(Vector2 coord, GameObject segment) {
			var go = MonoBehaviour.Instantiate(segment) as GameObject;
			go.transform.position = new Vector3(coord.x - mapOffset.x, mapOffset.y, coord.y - mapOffset.z);
			go.layer = mapLayer;

			return go;
		}

		GameObject LoadAndCreateSegmentAt(float x, float z){
			var segCoord = this.GetSegmentCoordForPos(x, z);
			var segment = this.LoadSegmentAt ((int)segCoord.x, (int) segCoord.y);

			var go = GameObject.Instantiate (segment) as GameObject;
			var go2 = GameObject.Instantiate (bundle.Load (string.Format ("{0}-{1}.{2}", 0, 400, mapSettings.segmentName), typeof(GameObject))) as GameObject;
			go2.transform.position = new Vector3(0 - mapOffset.x, mapOffset.y, 400 - mapOffset.z);
			go.transform.position = new Vector3(x - mapOffset.x, mapOffset.y, z - mapOffset.z);
			go2.layer = mapLayer;
			go.layer = mapLayer;

			return go;
		}

		GameObject LoadSegmentAt(int x, int z) {
			Debug.Log ("segment name: " + mapSettings.segmentName);
			return bundle.Load (string.Format ("{0}-{1}.{2}", x, z, mapSettings.segmentName), typeof(GameObject)) as GameObject;
		}
		#endregion


	}
}

