using UnityEngine;
using System.Collections;

public class explosionScript : MonoBehaviour {

	void Start() {
		Invoke("removeSelf", 3.5f);
	}
	
	void removeSelf() {
		Destroy(gameObject);
	}
}
