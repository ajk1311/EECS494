using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	protected GameObject target;
	public float speed = 0;
	// Use this for initialization
	void Start () {
        target = this.transform.parent.GetComponent<Unit>().currentTarget;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed);
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject == target){
			Destroy(gameObject);
		}
	}
}
