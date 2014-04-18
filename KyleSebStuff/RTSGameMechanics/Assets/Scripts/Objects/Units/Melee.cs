using UnityEngine;
using System.Collections;
using Pathfinding;

public class Melee : Unit {

	private bool attackStarted;
	private Int3 startAttackPosition;

	// Use this for initialization
	protected override void Start () {
		base.Start();
		damageInflicted = 1;
        attackRadius = 6;
		pursuitRadius = 12;
        speed = 15f;
        reloadSpeed = 0.75f;
        hitPoints = 5;
        maxHitPoints = 5;
	}

	public override void GameUpdate(float deltaTime) {
		base.GameUpdate(deltaTime);
		if (reloading) {
			// TODO while reloading, move back toward start attack position
			
		}
	}

	protected override void AttackHandler() {
        base.AttackHandler();
        //Stop moving in order to attack
        if (moving) {
        	moving = false;
        }

        if (!attackStarted) {
        	attackStarted = true;
        	startAttackPosition = intPosition;
        }

        // TODO move toward target rapidly, then set reloading to true

        reloading = true;
    }

    protected override void Reload() {
        base.Reload();
    }
}
