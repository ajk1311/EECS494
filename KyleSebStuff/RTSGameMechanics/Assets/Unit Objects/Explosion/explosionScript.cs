using UnityEngine;
using System.Collections;

public class explosionScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Invoke ("removeSelf", 5.0f);
	}
	

	void removeSelf() {
		Destroy (gameObject);
	}
}
