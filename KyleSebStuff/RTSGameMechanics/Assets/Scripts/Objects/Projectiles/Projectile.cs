using UnityEngine;
using System.Collections;
using RTS;

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

	public int PlayerID {
		get { return 0; }
	}

	// Update is called once per frame
	public virtual void GameUpdate(float deltaTime) {
		if(target == null) {
			Destroy (gameObject);
		}
		else {
			// TODO npe
			transform.position = Vector3.MoveTowards(transform.position, RTSGameMechanics.FindTransform(target.transform, "Target").position, speed);
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
