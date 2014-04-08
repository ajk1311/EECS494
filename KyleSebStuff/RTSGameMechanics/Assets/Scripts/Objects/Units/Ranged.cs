using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Ranged : Unit {

    public GameObject PROJECTILE;
	public int damageInflicted;

    // Use this for initialization
    protected override void Start() {
        base.Start();
		damageInflicted = 1;
        attackRange = 45f;
        speed = 15f;
        reloadSpeed = .75f;
        hitPoints = 5;
        maxHitPoints = 5;
    }

    public override void GameUpdate(float deltaTime) {
        base.GameUpdate(deltaTime);
    }

    protected override void AttackHandler() {
        base.AttackHandler();
        //Stop moving in order to attack
        moving = false;
        Vector3 projectilePosition = Vector3.MoveTowards(transform.position, currentTarget.transform.position, 1f);
        projectilePosition.y += 1;
        GameObject projectile = (GameObject)Instantiate(PROJECTILE, projectilePosition, Quaternion.identity);
        projectile.transform.parent = this.transform;
        reloading = true;
        Invoke("Reload", reloadSpeed);
    }

    protected override void Reload() {
        reloading = false;
    }

    public override void TakeDamage(int damage) {
        base.TakeDamage(damage);
    }
}
