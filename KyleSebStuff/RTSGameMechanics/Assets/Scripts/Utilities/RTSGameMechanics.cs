using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

namespace RTS {
    public static class RTSGameMechanics {

		public static Vector3 GetMapSizes() {
			GameObject origin = GameObject.Find("__Origin__");
			GameObject upperBound = GameObject.Find("__UpperMapBound__");
			
			float mapWidth = upperBound.transform.position.x - origin.transform.position.x;
			float mapHeight = upperBound.transform.position.z - origin.transform.position.z; 
			return new Vector3 (mapWidth, 0, mapHeight);
		}

		public static GameObject FindHitObject(Vector3 position) {
            // List<WorldObject> occupants = GridManager.GetGridOccupants((Int3) position);
            // GameObject returnObj = null;
            // int currentID = int.MaxValue;
            // if (occupants.Count > 0) {
            //     foreach (WorldObject occupant in occupants) {
            //         if (occupant != null && occupant.ID < currentID) {
            //             currentID = occupant.ID;
            //             returnObj = occupant.transform.root.gameObject;
            //         }
            //     }
            // } else {
            //     returnObj = GameObject.Find("Final Map");
            // }
            // return returnObj;

			Collider[] hitColliders = Physics.OverlapSphere(position, 0.2f);
			GameObject returnObj = null;
			int currentID = int.MaxValue;
			if (hitColliders.Length != 0) {
				if(hitColliders.Length == 1) {
					return hitColliders[0].gameObject.transform.root.gameObject;
                }
				foreach (Collider obj in hitColliders) {
					WorldObject script = obj.gameObject.transform.root.GetComponent<WorldObject>();
					if (script != null && script.ID < currentID) {
                        currentID = script.ID;
						returnObj = obj.transform.root.gameObject;
					}
				}
			}
			return returnObj;
		}

        public static Vector3 FindHitPoint() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                return hit.point;
            }
            return MechanicResources.InvalidPosition;
        }

        public static Vector3 FindHitPointOnMap(Vector3 position) {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit[] hits;
            hits = Physics.RaycastAll(ray);
            if (hits.Length > 0) { 
                foreach (RaycastHit hit in hits) {
                    if (hit.transform.tag == "Map") {
                        return hit.point;
                    }
                }
            }
            return MechanicResources.InvalidPosition;
        }

        public static bool IsWithin(GameObject gameObject, Vector3[] rect) {
			if (rect == null) {
                return false;
            }
			float x0 = rect[0].x;
            float x1 = rect[1].x;
            bool containsX = false;
            float targetX = gameObject.transform.position.x;

            if (x0 < x1) {
                containsX = targetX <= x1 && targetX >= x0;
            } else {
                containsX = targetX >= x1 && targetX <= x0;
            }

            if (!containsX) {
                return false;
            }

            float z0 = rect[0].z;
            float z1 = rect[1].z;
            bool containsZ = false;
            float targetZ = gameObject.transform.position.z;

            if (z0 < z1) {
                containsZ = targetZ <= z1 && targetZ >= z0;
            } else {
                containsZ = targetZ >= z1 && targetZ <= z0;
            }

            return containsZ;
        }

        public static int GetAttentionPhysicsLayer(int friendlyLayer) {
            if (friendlyLayer == LayerMask.NameToLayer("P1"))
                return LayerMask.NameToLayer("P1 Attention");
            return LayerMask.NameToLayer("P2 Attention");
        }

        public static Transform FindTransform(Transform parent, string name) {      
            if (parent.name.Equals(name))
                return parent;
            foreach (Transform child in parent) {              
                Transform result = FindTransform(child, name);                
                if (result != null)
                    return result;                
            }
            return null;            
        }
    }
}