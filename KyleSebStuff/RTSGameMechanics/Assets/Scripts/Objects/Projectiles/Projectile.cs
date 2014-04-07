using UnityEngine;
using System.Collections;
using RTS;

public class Projectile : MonoBehaviour {

	protected GameObject target;
	public float speed = 0;
	public int damageInflicted;
	// Use this for initialization
	void Start () {
        target = this.transform.parent.GetComponent<Unit>().currentTarget;
		damageInflicted = this.transform.parent.GetComponent<Ranged> ().damageInflicted;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position = Vector3.MoveTowards(transform.position, RTSGameMechanics.FindTransform(target.transform, "Target").position, speed);
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject == target){
            target.GetComponent<WorldObject>().TakeDamage(damageInflicted);
			Destroy(gameObject);
		}
	}
}
