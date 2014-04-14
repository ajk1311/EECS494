using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Projectile : MonoBehaviour, SSGameManager.IUpdatable {

	public WorldObject target;
	public float speed = 0;
	public int damageInflicted;
	public int playerID;

	private Int3 intPosition;

	protected virtual void Start () {
		SSGameManager.Register(this);
		intPosition = new Int3(transform.position);
	}

	public virtual void GameUpdate(float deltaTime) {
		if(target == null) {
			Destroy(gameObject);
		} else {
			Int3 targetPosition = (Int3) RTSGameMechanics.FindTransform(target.transform, "Target").position;
			if (IntPhysics.IsCloseEnough(intPosition, targetPosition, 0.5f)) {
				target.TakeDamage(damageInflicted);
				Destroy(gameObject);
			} else {
				intPosition += IntPhysics.DisplacementTo(intPosition, targetPosition, 
				                                     IntPhysics.FloatSafeMultiply(speed, deltaTime));
				transform.position = (Vector3) intPosition;
			}
		}

		GameObject fogTile = FogOfWarManager.getMyFogTile (intPosition);
		if(FogOfWarManager.isVisible(fogTile, playerID)) {
			renderer.enabled = true;
		}
		else {
			renderer.enabled = false;
		}
	}

	protected virtual void OnDestroy() {
		SSGameManager.Unregister(this);
	}
}
