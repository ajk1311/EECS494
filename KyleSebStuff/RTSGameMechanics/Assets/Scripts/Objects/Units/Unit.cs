using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;
using System.Collections.Generic;

public class Unit : WorldObject {

	// State variables
    protected bool pathComplete = false;
    protected bool moving = false;
	protected bool pursuing = false;
    protected bool attacking = false;
    protected bool attackMove = false;
    protected bool isTargetBuilding = false;
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
			if (moving && pathComplete && path.vectorPath.Count > 0 && currentWaypoint != path.vectorPath.Count) {
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
	public int pursuitRadius;
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
        idle = false;
        pursuing = true;
        currentTarget = target;
        lastTargetDestination = MechanicResources.InvalidIntPosition;
    }

    public void IssueMoveCommand(Vector3 destination, bool attackMove_ = false) {
        if (attacking || pursuing) {
            FinishAttacking();
        }
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
		if (currentTarget == null) {
			FinishAttacking();
		}
		if (WithinAttackRange()) {
			attacking = true;
			moving = pursuing = false;
			lastTargetDestination = MechanicResources.InvalidIntPosition;
        } else {
			Unit unit = currentTarget.GetComponent<Unit>();
			if (unit == null) {
                if (!isTargetBuilding) {
                    // Target is a building, so just go to it
                    isTargetBuilding = true;
                    StartMovement((Vector3)currentTarget.intPosition);
                }
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
		int relativeX = currentTarget.intPosition.x - intPosition.x;
		int relativeZ = currentTarget.intPosition.z - intPosition.z;
		Int3 direction = target.IntDirection;
		return System.Math.Sign(relativeX) != direction.x && 
			System.Math.Sign(relativeZ) != direction.z;
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
        moving = pathComplete = attackMove = false;
        idle = true;
    }

	public void FinishAttacking() {
		idle = true;
		moving = pursuing = attacking = attackMove = isTargetBuilding = false;
		lastTargetDestination = 
			MechanicResources.InvalidIntPosition;
    }

    public override void GameUpdate(float deltaTime) {
        base.GameUpdate(deltaTime);

		// TODO check for floating point calculations
		if (RTSGameMechanics.IsWithin(gameObject, SelectionManager.GetSelectedSpace(playerID))) {
			currentlySelected = true;
		}

		if (pursuing) {
			Pursuit(deltaTime);
		}

        if (attacking) {
            if (currentTarget) {
				if (WithinAttackRange()) {
					if (!reloading) AttackHandler();
				} else {
					pursuing = true;
					idle = attacking = false;
					lastTargetDestination =
						MechanicResources.InvalidIntPosition;
					Pursuit(deltaTime);
				}
            } else {
                FinishAttacking();
            }
        }

		if (idle || (attackMove && !pursuing && !attacking)) {
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
			GridManager.GetObjectsInRadius(this, pursuitRadius);

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
			pursuing = true;
			currentTarget = finalTarget;
			lastTargetDestination = MechanicResources.InvalidIntPosition;
		}
	}
}
