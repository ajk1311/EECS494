using UnityEngine;
using System.Collections;

namespace RTS {
	public static class RTSGameMechanics {
		//change this to use world coordinates
		public static GameObject FindHitObject(Vector3 position) {
			Collider[] hitColliders = Physics.OverlapSphere (position, 0.25f);
			if(hitColliders.Length != 0) {
				return hitColliders[0].gameObject;
			}
			return null;
		}

		public static Vector3 FindHitPoint() {
			return FindHitPoint(Input.mousePosition);
		}

        public static Vector3 FindHitPoint(Vector3 position) {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                return hit.point;
			}
            return MechanicResources.InvalidPosition;
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