using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Ranged : Unit {

    public GameObject Projectile;
	public int damageInflicted;

    // Use this for initialization
    protected override void Start() {
        base.Start();
		damageInflicted = 1;
        attackRange = 10;
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
		Debug.Log ("---------Entered AttackHandler-------");
        moving = false;

		Int3 direction = (Int3) ((Vector3) ((Int3) currentTarget.transform.position - (Int3) transform.position)).normalized;
        direction.y += 1;

		GameObject projectile = (GameObject) Instantiate(Projectile, (Vector3) direction, Quaternion.identity);
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
