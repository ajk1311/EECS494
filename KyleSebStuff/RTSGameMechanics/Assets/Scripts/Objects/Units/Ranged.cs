using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Ranged : Unit {

    public GameObject Projectile;

    protected override void AttackHandler() {
        base.AttackHandler();
        //Stop moving in order to attack
        moving = false;
		Int3 projectilePosition = intPosition + new Int3(0, 1, 0);
		GameObject projectile = (GameObject) Instantiate(Projectile, (Vector3) projectilePosition, Quaternion.identity);
        Projectile script = projectile.GetComponent<Projectile>();
		script.target = currentTarget;
		script.damageInflicted = damageInflicted;
		script.playerID = playerID;
        reloading = true;
    }

    protected override void Reload() {
        base.Reload();
    }
}
