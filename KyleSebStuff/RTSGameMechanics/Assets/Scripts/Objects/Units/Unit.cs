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
    public GameObject currentTarget = null;
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
        seeker = GetComponent<Seeker>();
		lastPosition = new Int3(transform.position);
		GridManager.UpdatePosition(lastPosition, this);
    }

	protected override void OnDestroy() {
		base.OnDestroy();
		GridManager.RemoveFromGrid(this);
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

    public void IssueAttackCommand(GameObject target) {
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
            if (oldEnemyPosition != (Int3) currentTarget.transform.position) {
				Int3 direction =  (Int3) ((Vector3) ((Int3) currentTarget.transform.position - (Int3) transform.position)).normalized;
				direction *= speed * deltaTime;
				transform.Translate((Vector3) direction);
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
		float distance = ((Int3) currentTarget.transform.position - (Int3) transform.position).magnitude / Int3.FloatPrecision;
		return distance <= attackRange;
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

		Int3 currentPosition = new Int3(transform.position);
		if(currentPosition != lastPosition) {
			lastPosition = currentPosition;
			GridManager.UpdatePosition(lastPosition, this);
		}

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
        //TODO: Make Attention Work
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

				Int3 difference = (Int3) path.vectorPath[currentWaypoint] - (Int3) transform.position;
				Int3 direction = (Int3) ((Vector3) difference).normalized;
				direction *= speed * deltaTime;

				transform.Translate((Vector3) direction);

                //Check if we are close enough to the next waypoint
                //If we are, proceed to follow the next waypoint
				float distance = ((Int3) transform.position - (Int3) path.vectorPath[currentWaypoint]).magnitude / Int3.FloatPrecision;
                if (distance < nextWaypointDistance) {
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
		GameObject finalTarget = null;

		List<WorldObject> potentialEnemies = 
			GridManager.GetObjectsInRadius(this, attackRange);

		if(potentialEnemies.Count > 0) {
			foreach(WorldObject obj in potentialEnemies) {
				if(obj.transform.tag == "Kill" && obj.gameObject.layer != this.gameObject.layer) {
					if(obj.ID < currentID) {
						currentID = obj.ID;
						finalTarget = obj.gameObject;
						foundEnemy = true;
					}
				}
			}
		}

		if(foundEnemy) {
			idle = false;
			attacking = true;
			currentTarget = finalTarget;
		}
	}
}
