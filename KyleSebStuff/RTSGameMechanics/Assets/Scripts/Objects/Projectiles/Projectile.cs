using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Projectile : MonoBehaviour, SSGameManager.IUpdatable {

	protected GameObject target;
	public float speed = 0;
	public int damageInflicted;
	// Use this for initialization
	protected virtual void Start () {
		SSGameManager.Register(this);
        target = this.transform.parent.GetComponent<Unit>().currentTarget;
		damageInflicted = this.transform.parent.GetComponent<Ranged>().damageInflicted;
	}

	// Update is called once per frame
	public virtual void GameUpdate(float deltaTime) {
		if(target == null) {
			Destroy (gameObject);
		}
		else {
			Vector3 targetPosition = RTSGameMechanics.FindTransform(target.transform, "Target").position;
			Int3 direction = (Int3) (targetPosition - transform.position).normalized;
			direction *= speed * deltaTime;
			transform.Translate((Vector3) direction);
		}
	}

	protected virtual void OnDestroy() {
		SSGameManager.Unregister(this);
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject == target){
            target.GetComponent<WorldObject>().TakeDamage(damageInflicted);
			Destroy(gameObject);
		}
	}
}
