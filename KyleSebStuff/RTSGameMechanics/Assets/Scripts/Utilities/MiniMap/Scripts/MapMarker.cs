using UnityEditor;
using UnityEngine;
using System.Collections;

using System;

namespace MyMinimap
{
	public class MapMarker
	{
		public GameObject	gameObject;
		public Texture		texture;
		public int			priority;
		public Rect			rect;
		public Rect         highlightRect;   
		public static float	x;
		public static float	y;
		public bool			shouldHighLight = false;
		public int          highlightCount;
		public float        timeElapsed;
		public int          iterateCount;


		public MapMarker(GameObject _gameObject, Texture _texture) {
			gameObject = _gameObject;
			texture = _texture;
			rect = new Rect (x, y, 7, 7);
			highlightRect = new Rect(x, y, 10, 10);
			highlightCount = 0;
		}

		public void highLight(bool set){
			if(set){
				shouldHighLight = true;
			}
			else {
				shouldHighLight = false;
				iterateCount = 0;
			}
		}
	}
}