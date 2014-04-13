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
	protected Int3 intDirection;

	public Int3 IntDirection {
		get {
			if (moving && pathComplete) {
				Int3 nextWayPoint = (Int3) path.vectorPath[currentWaypoint];
				Int3 difference = nextWayPoint - intPosition;
				return new Int3(System.Math.Sign(difference.x), 0, System.Math.Sign(difference.z));
			}
			return Int3.zero;
		}
	}

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
		intDirection = Int3.zero;
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
		Path newPath = seeker.GetNewPath((Vector3) intPosition, destination);
		seeker.StartPath(newPath, OnPathComplete);
		AstarPath.WaitForPath(newPath);
        moving = true;
        idle = false;
    }

    protected virtual void Pursuit(float deltaTime) {
        if (WithinAttackRange()) {
			moving = false;
			lastTargetDestination = MechanicResources.InvalidIntPosition;
            if (!reloading) {
                AttackHandler();
			}
        } else {
			Unit unit = currentTarget.GetComponent<Unit>();
			if (unit == null) {
				// Target is a building, so just go to it
				StartMovement((Vector3) currentTarget.intPosition);
			} else if (lastTargetDestination != unit.destination) {
				if (TargetApproaching(unit)) {
					StartMovement((Vector3) currentTarget.intPosition);
				} else {
					StartMovement((Vector3) unit.destination);
				}
				lastTargetDestination = unit.destination;
			}
		}
    }

	private bool TargetApproaching(Unit target) {
		Debug.Log("Is target approaching?");
		int relativeX = currentTarget.intPosition.x - intPosition.x;
		Debug.Log("Relative x = " + System.Math.Sign(relativeX));
		int relativeZ = currentTarget.intPosition.z - intPosition.z;
		Debug.Log("Relative z = " + System.Math.Sign(relativeZ));
		Debug.Log("Target's direction = " + target.IntDirection);
		return System.Math.Sign(relativeX) != target.IntDirection.x && 
			System.Math.Sign(relativeZ) != System.Math.Sign(target.IntDirection.z);
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
			Int3 delta = IntPhysics.DisplacementTo(intPosition, nextWayPoint, 
			                                     IntPhysics.FloatSafeMultiply(speed, deltaTime));
			intDirection = new Int3(System.Math.Sign(delta.x), 0, System.Math.Sign(delta.z));
			intPosition += delta;
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
			lastTargetDestination = MechanicResources.InvalidIntPosition;
		}
	}
}
