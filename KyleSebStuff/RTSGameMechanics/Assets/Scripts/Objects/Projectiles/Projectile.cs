using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Projectile : MonoBehaviour, SSGameManager.IUpdatable {

	protected WorldObject target;
	public float speed = 0;
	public int damageInflicted;

	protected virtual void Start () {
		SSGameManager.Register(this);
        target = this.transform.parent.GetComponent<Unit>().currentTarget;
		damageInflicted = this.transform.parent.GetComponent<Ranged>().damageInflicted;
	}

	public virtual void GameUpdate(float deltaTime) {
		if(target == null) {
			Destroy(gameObject);
		} else {
			Vector3 targetPosition = RTSGameMechanics.FindTransform(target.transform, "Target").position;
			float distance = ((Int3) transform.position - (Int3) targetPosition).magnitude / Int3.FloatPrecision;
			if (distance < 0.1f) {
				target.GetComponent<WorldObject>().TakeDamage(damageInflicted);
				Destroy(gameObject);
			} else {
				Int3 direction = (Int3) ((Vector3) ((Int3) targetPosition - (Int3) transform.position)).normalized;
				direction *= speed * deltaTime;
				transform.Translate((Vector3) direction);
			}
		}
	}

	protected virtual void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
