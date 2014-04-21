using UnityEngine;
using System.Collections;

public class explosionScript : MonoBehaviour {

	void Start() {
		Invoke("removeSelf", 5.0f);
	}
	
	void removeSelf() {
		Destroy(gameObject);
	}
}
