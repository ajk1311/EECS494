using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Ranged : Unit {

    public GameObject Projectile;
	public int damageInflicted;

	private int mCooldownTime;

    protected override void Start() {
        base.Start();
		damageInflicted = 0;
        attackRadius = 10;
        speed = 15f;
        reloadSpeed = 0.75f;
        hitPoints = 5;
        maxHitPoints = 5;
		mCooldownTime = 0;
    }

    public override void GameUpdate(float deltaTime) {
        base.GameUpdate(deltaTime);
		if (reloading) {
			mCooldownTime += (int) System.Math.Round(deltaTime * Int3.FloatPrecision);
			if (mCooldownTime >= (int) System.Math.Round(reloadSpeed * Int3.FloatPrecision)) {
				reloading = false;
				mCooldownTime = 0;
			}
		}
    }

    protected override void AttackHandler() {
        base.AttackHandler();
        //Stop moving in order to attack
        moving = false;

		Vector3 projectilePosition = transform.position + new Vector3(0, 1, 0);

		GameObject projectile = (GameObject) Instantiate(Projectile, projectilePosition, Quaternion.identity);
        projectile.transform.parent = this.transform;
        reloading = true;
//        Invoke("Reload", reloadSpeed);
    }

    protected override void Reload() {
        reloading = false;
    }

    public override void TakeDamage(int damage) {
        base.TakeDamage(damage);
    }
}
