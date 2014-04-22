using UnityEngine;
using System.Collections;
using Pathfinding;

public class Melee : Unit {

	public float attackSpeed;
	private bool attackStarted;
	private Int3 attackPosition;
	private Int3 startAttackPosition;

	//Sound
	public AudioClip punchSound;

	// Use this for initialization
	protected override void Start() {
		base.Start();
        attackSpeed = 28f;
	}

	public override void GameUpdate(float deltaTime) {
		base.GameUpdate(deltaTime);
		if (!attacking || currentTarget == null) {
			attackStarted = false;
			return;
		}

		if (reloading) {
			intPosition += IntPhysics.DisplacementTo(intPosition, 
				startAttackPosition, IntPhysics.FloatSafeMultiply(attackSpeed, deltaTime));
			transform.position = (Vector3) intPosition;
		} else if (attackStarted) {
			if (IntPhysics.IsCloseEnough(intPosition, attackPosition, 0.2f)) {
				currentTarget.TakeDamage(damageInflicted);
				AudioSource.PlayClipAtPoint(punchSound, transform.position);
        		reloading = true;
	        } else {
	        	intPosition += IntPhysics.DisplacementTo(intPosition, 
	        		attackPosition, IntPhysics.FloatSafeMultiply(attackSpeed, deltaTime));
	        	transform.position = (Vector3) intPosition;
	        }
		}
	}

	protected override void AttackHandler() {
        base.AttackHandler();
        // Stop moving in order to attack
        if (moving) {
        	moving = false;
        }

        // Record position before attack
        if (!attackStarted) {
        	attackStarted = true;
        	startAttackPosition = intPosition;
        	transform.rotation = Quaternion.LookRotation(
        		(currentTarget.transform.position - transform.position).normalized);
        	attackPosition = intPosition + (Int3) (transform.forward * attackRadius);
        }
    }

    protected override void Reload() {
        base.Reload();
        intPosition = startAttackPosition;
		transform.position = (Vector3) intPosition;
    }
}
