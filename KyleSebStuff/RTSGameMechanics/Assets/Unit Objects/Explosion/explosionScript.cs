using UnityEngine;
using System.Collections;

public class explosionScript : MonoBehaviour {

	void Start() {
		Invoke("removeSelf", 1.0f);
	}
	
	void removeSelf() {
		Destroy(gameObject);
	}
}
