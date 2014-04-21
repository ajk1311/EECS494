using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Pathfinding;

public class DefensiveTower : DestructableBuilding {

    // State variables
    protected bool attacking = false;
    protected bool idle = true;
    public bool reloading = false;
    private int mCooldownTime;
    public int damageInflicted = 5;
    public float reloadSpeed = 1f;

    // Targeting state
    public WorldObject currentTarget = null;
    public int attackRadius = 25;
    public GameObject Projectile;

    protected override void Start() {
        base.Start();
    }
    
    protected override GUIModelManager.GUIModel GetGUIModel() {
        return null;
    }

    public override void GameUpdate(float deltaTime) {
        base.GameUpdate(deltaTime);

        if (attacking) {
            if (currentTarget) {
                if (WithinAttackRange()) {
                    if (!reloading) 
                        AttackHandler();
                } else {
					idle = true;
					attacking = false;
                    currentTarget = null;
                }
            } else {
                FinishAttacking();
            }
        }
        if (idle) {
            ScanForEnemies();
        }
        if (reloading) {
            mCooldownTime += (int) System.Math.Round(deltaTime * Int3.FloatPrecision);
            if (mCooldownTime >= (int) System.Math.Round(reloadSpeed * Int3.FloatPrecision)) {
                reloading = false;
                mCooldownTime = 0;
            }
        }
    }

    public virtual void ScanForEnemies() {
        int lowestID = int.MaxValue;
        WorldObject finalTarget = null;
        
        List<WorldObject> potentialEnemies = 
            GridManager.GetObjectsInRadius(this, attackRadius);
        
        WorldObject potentialEnemy;
        for (int i = 0, sz = potentialEnemies.Count; i < sz; i++) {
            potentialEnemy = potentialEnemies[i];
            if (potentialEnemy.gameObject.layer != gameObject.layer && 
                potentialEnemy.gameObject.tag != "CaptureTower" && 
                potentialEnemy.ID < lowestID) {
                lowestID = potentialEnemy.ID;
                finalTarget = potentialEnemy;
            }
        }
        if (finalTarget != null) {
            idle = false;
            currentTarget = finalTarget;
			attacking = true;
        }
    }
    
    protected virtual bool WithinAttackRange() {
        return GridManager.IsWithinRange(this, currentTarget, attackRadius);
    }

    public void FinishAttacking() {
        idle = true;
        attacking = false;
    }

    private void AttackHandler(){
        Int3 projectilePosition = intPosition + new Int3(0, 8, 0);
        GameObject projectile = (GameObject) Instantiate(Projectile, (Vector3) projectilePosition, Quaternion.identity);
        projectile.GetComponent<Projectile>().target = currentTarget;
        projectile.GetComponent<Projectile>().damageInflicted = damageInflicted;
        projectile.GetComponent<Projectile> ().playerID = playerID;
        reloading = true;
    }
}
