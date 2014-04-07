using UnityEngine;
using System.Collections;

namespace RTS {
    public static class RTSGameMechanics {
        //change this to use world coordinates
        public static GameObject FindHitObject(Vector3 position) {
            Collider[] hitColliders = Physics.OverlapSphere(position, 0.25f);
            if (hitColliders.Length != 0) {
                return hitColliders [0].gameObject;
            }
            return null;
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

            float x0 = rect [0].x;
            float x1 = rect [1].x;
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

            float z0 = rect [0].z;
            float z1 = rect [1].z;
            bool containsZ = false;
            float targetZ = gameObject.transform.position.z;

            if (z0 < z1) {
                containsZ = targetZ <= z1 && targetZ >= z0;
            } else {
                containsZ = targetZ <= z0 && targetZ >= z1;
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