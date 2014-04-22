using UnityEngine;
using System.Collections;

public class explosionScript : MonoBehaviour {

	void Start() {
		Invoke("removeSelf", 2.0f);
	}
	
	void removeSelf() {
		Destroy(gameObject);
	}
}
