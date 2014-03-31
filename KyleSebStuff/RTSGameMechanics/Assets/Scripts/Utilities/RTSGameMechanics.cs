using UnityEngine;
using System.Collections;

namespace RTS {
	public static class RTSGameMechanics {

		public static GameObject FindHitObject() {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) 
				return hit.collider.gameObject;
			return null;
		}

		public static bool IsWithin(GameObject gameObject, Rect rect) {
			Vector3 screenPos = Camera.main.WorldToScreenPoint (gameObject.transform.position);
			Vector3 realScreenPos = new Vector3(screenPos.x, Screen.height-screenPos.y, screenPos.z);
			
			if (rect.Contains (realScreenPos, true)) {
				return true;
			}
			
			return false;
		}
	}
}