using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;
using System.Collections.Generic;

public class Unit : WorldObject {

	// State variables
    protected bool pathComplete = false;
    protected bool moving = false;
    protected bool attacking = false;
	protected bool idle = true;
	public bool reloading = false;

	// Pathfinding variables
    protected Seeker seeker;
    protected Path path;
	public int currentWaypoint = 0;
	public float nextWaypointDistance = 0.5f;
	protected Int3 destination;

	// Targeting state
    public WorldObject currentTarget = null;
	public Int3 lastTargetDestination;

    // Attributes
    public float speed;
    public int attackRadius;
    public float reloadSpeed;
    public float attentionRange;

    protected override void Start() {
        base.Start();
		destination = intPosition;
		seeker = GetComponent<Seeker>();
		lastTargetDestination = MechanicResources.InvalidIntPosition;
    }

    public bool isMoving() {
        return moving;
    }

    public void setMoving(bool moving) {
        this.moving = moving;
    }

    public bool isAttacking() {
        return attacking;
    }

    public void setAttacking(bool attacking) {
        this.attacking = attacking;
    }

    public void IssueAttackCommand(WorldObject target) {
        currentTarget = target;
        attacking = true;
    }

    public void IssueMoveCommand(Vector3 destination) {
        attacking = false;
        currentTarget = null;
		this.destination = (Int3) destination;
        StartMovement(destination);
    }

    private void StartMovement(Vector3 destination) {
		seeker.StartPath(transform.position, destination, OnPathComplete);
        moving = true;
        idle = false;
    }

    protected virtual void Pursuit(float deltaTime) {
        if (WithinAttackRange()) {
			moving = false;
            if (!reloading) {
                AttackHandler();
			}
        } else {
			Unit unit = currentTarget.GetComponent<Unit>();
			if (unit == null) {
				// Target is a building, so just go to it
				StartMovement((Vector3) currentTarget.intPosition);
			} else if (lastTargetDestination != unit.destination){
				// Target is a unit and has changed destinations
				StartMovement((Vector3) unit.destination);
				lastTargetDestination = unit.destination;
			}
		}
    }

    protected virtual void Reload() {
    }

    protected virtual void AttackHandler() {
    }

    protected virtual bool WithinAttackRange() {
		return GridManager.IsWithinRange(this, currentTarget, attackRadius);
    }

    protected void OnPathComplete(Path p) {
        if (!p.error) {
            path = p;
			path.nnConstraint = NNConstraint.None;
            currentWaypoint = 0;
            pathComplete = true;
        } else {
            pathComplete = false;
		}
    }

    public void ReachedDestination() {
        pathComplete = false;
        moving = false;
        idle = true;
    }

    public void FinishAttacking() {
        attacking = false;
        moving = false;
		idle = true;
		lastTargetDestination = 
			MechanicResources.InvalidIntPosition;
    }

    public override void GameUpdate(float deltaTime) {
        base.GameUpdate(deltaTime);

		// TODO check for floating point calculations
		if (RTSGameMechanics.IsWithin(gameObject, SelectionManager.GetSelectedSpace(playerID))) {
			currentlySelected = true;
		}

        if (attacking) {
            if (currentTarget) {
                Pursuit(deltaTime);
            } else {
                FinishAttacking();
            }
        }

		if (idle) {
			ScanForEnemies();
		}

		if (moving && pathComplete) {
			CalculateBounds();

			if (currentWaypoint >= path.vectorPath.Count) {
				ReachedDestination();
				return;
			}
			Int3 nextWayPoint = (Int3) path.vectorPath[currentWaypoint];
			int intSpeed = (int) System.Math.Round(speed * Int3.FloatPrecision);
			int intTime = (int) System.Math.Round (deltaTime * Int3.FloatPrecision);
			intPosition = IntPhysics.MoveTowards(intPosition, nextWayPoint, 
			                                     IntPhysics.FloatSafeMultiply(speed, deltaTime));
			transform.position = (Vector3) intPosition;
			
			if (IntPhysics.IsCloseEnough(intPosition, nextWayPoint, nextWaypointDistance)) {
				currentWaypoint++;
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
			    potentialEnemy.ID < lowestID) {
				lowestID = potentialEnemy.ID;
				finalTarget = potentialEnemy;
			}
		}

		if(finalTarget != null) {
			idle = false;
			attacking = true;
			currentTarget = finalTarget;
		}
	}
}
