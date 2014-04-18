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
		public static float	x;
		public static float	y;
		public bool			shouldHighLight = false;


		public MapMarker(GameObject _gameObject, Texture _texture) {
			gameObject = _gameObject;
			texture = _texture;
			rect = new Rect (x, y, 10, 10);
		}

		public void highLight(bool set){
			if(set){
				shouldHighLight = true;
			}
			else
				shouldHighLight = false;
		}
	}
}