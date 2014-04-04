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

		public static Vector3 FindHitPointOnMap() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;
            if (Physics.RaycastAll(ray, out hits)) 
				foreach(RaycastHit hit in hits) {
					if(hit.transform.tag == "Map") {
					return hit.point;
					}
				}
            return MechanicResources.InvalidPosition;
        }

		public static bool IsWithin(GameObject gameObject, Rect rect) {
			Vector3 modifiedObjectPos = new Vector3 (gameObject.transform.position.x, 0, gameObject.transform.position.z);
			
			if (rect.Contains (modifiedObjectPos, true)) {
				return true;
			}
			
			return false;
		}
	}
}