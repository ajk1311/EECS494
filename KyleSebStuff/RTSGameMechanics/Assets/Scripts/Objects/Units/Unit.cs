using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;
using System.Collections.Generic;

public class Unit : WorldObject {

    protected Vector3 destination;
    protected bool pathComplete = false;
    protected bool moving = false;
    protected bool attacking = false;
    protected bool idle = true;
    protected Seeker seeker;
    protected Path path;
    protected Int3 oldEnemyPosition;
    public WorldObject currentTarget = null;
    public int currentWaypoint = 0;
    public float nextWaypointDistance = 0.5f;
    public float speed;
    public int attackRange;
    public float reloadSpeed;
    public bool reloading = false;
    public float attentionRange;
	public Int3 lastPosition;	

    protected override void Awake() {
        base.Awake();
    }
    // Use this for initialization
    protected override void Start() {
        base.Start();
		lastPosition = intPosition;
        seeker = GetComponent<Seeker>();
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
        StartMovement(destination);
    }

    private void StartMovement(Vector3 destination) {
        seeker.StartPath(transform.position, destination, OnPathComplete);
        moving = true;
        idle = false;
    }

    protected virtual void Pursuit(float deltaTime) {
        if (WithinAttackRange()) {
            if (!reloading)
                AttackHandler();
        } else {
            if (oldEnemyPosition != currentTarget.intPosition) {
				intPosition = IntPhysics.MoveTowards(intPosition, currentTarget.intPosition, speed * deltaTime);
				transform.position = (Vector3) intPosition;
            } else {
                StartMovement(currentTarget.transform.position);
            }
            oldEnemyPosition = (Int3) currentTarget.transform.position;
        }
    }

    protected virtual void Reload() {
    }

    protected virtual void AttackHandler() {
    }

    protected virtual bool WithinAttackRange() {
		return GridManager.IsWithinRange(this, currentTarget, attackRange);
    }

    protected void OnPathComplete(Path p) {
        if (!p.error) {
            path = p;
			path.nnConstraint = NNConstraint.None;
            currentWaypoint = 0;
            pathComplete = true;
        } else
            pathComplete = false;
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
    }

    public override void GameUpdate(float deltaTime) {
        base.GameUpdate(deltaTime);

		if(intPosition != lastPosition) {
			lastPosition = intPosition;
			GridManager.UpdatePosition(intPosition, this);
		}

		// TODO check for floating point calculations
		if (RTSGameMechanics.IsWithin(gameObject, SelectionManager.GetSelectedSpace(playerID))) {
			currentlySelected = true;
		}
				
        if (moving && pathComplete) {
            CalculateBounds();
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

        if (!moving) {
            return;
        } else {
            if (pathComplete) {
                if (currentWaypoint >= path.vectorPath.Count) {
                    ReachedDestination();
                    return;
                }
				Int3 nextWayPoint = (Int3) path.vectorPath[currentWaypoint];

				intPosition = IntPhysics.MoveTowards(intPosition, nextWayPoint, speed * deltaTime);
				transform.position = (Vector3) intPosition;

				if (IntPhysics.IsCloseEnough(intPosition, nextWayPoint, nextWaypointDistance)) {
                    currentWaypoint++;
                    return;
                }
            }
        }
    }

    public override void TakeDamage(int damage) {
        base.TakeDamage(damage);
    }

    public virtual void ScanForEnemies() {
		bool foundEnemy = false;
		int currentID = int.MaxValue;
		WorldObject finalTarget = null;

		List<WorldObject> potentialEnemies = 
			GridManager.GetObjectsInRadius(this, attackRange);

		if(potentialEnemies.Count > 0) {
			foreach(WorldObject obj in potentialEnemies) {
				if(obj.gameObject.layer != gameObject.layer && obj.ID < currentID) {
					currentID = obj.ID;
					finalTarget = obj;
					foundEnemy = true;
				}
			}
		}

		if(foundEnemy) {
//			idle = false;
//			attacking = true;
//			currentTarget = finalTarget;
//			Debug.Log("=========== Found enemy in range ============");
//			Unit unit = finalTarget.GetComponent<Unit>();
//			if (unit != null) {
//				Debug.Log("\tID=" + unit.ID);
//				Debug.Log("\tposition=" + unit.intPosition);
//				Debug.Log("\tgridIndex=[" + unit.gridIndex[0] + "," + unit.gridIndex[1] + "]");
//			}
		}
	}
}
