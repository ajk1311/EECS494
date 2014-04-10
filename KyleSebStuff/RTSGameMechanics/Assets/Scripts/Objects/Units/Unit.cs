using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Unit : WorldObject {

    protected Vector3 destination;
    protected bool pathComplete = false;
    protected bool moving = false;
    protected bool attacking = false;
    protected bool idle = true;
    protected Seeker seeker;
    protected Path path;
    protected Vector3 oldEnemyPosition;
    public GameObject currentTarget = null;
    public int currentWaypoint = 0;
    public float nextWaypointDistance = 0.1f;
    public float speed;
    public float attackRange;
    public float reloadSpeed;
    public bool reloading = false;
    public float attentionRange;

    protected override void Awake() {
        base.Awake();
    }
    // Use this for initialization
    protected override void Start() {
        base.Start();
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

    protected virtual void Pursuit() {
        if (WithinAttackRange()) {
            if (!reloading)
                AttackHandler();
        } else {
            if (oldEnemyPosition != currentTarget.transform.position) {
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position, speed * Time.deltaTime);
            } else {
                StartMovement(currentTarget.transform.position);
            }
            oldEnemyPosition = currentTarget.transform.position;
        }
    }

    protected virtual void Reload() {
    }

    protected virtual void AttackHandler() {

    }

    protected virtual bool WithinAttackRange() {
        if (Vector3.Distance(currentTarget.transform.position, transform.position) <= attackRange) {
            return true;
        }
        return false;
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

		if (RTSGameMechanics.IsWithin(gameObject, SelectionManager.GetSelectedSpace(playerID))) {
			currentlySelected = true;
		}
				
        if (moving && pathComplete) {
            CalculateBounds();
        }

        if (attacking) {
            if (currentTarget) {
                Pursuit();
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

                Vector3 direction = (path.vectorPath [currentWaypoint] - transform.position).normalized;
                direction *= speed * deltaTime;
                
//				transform.position = Vector3.MoveTowards(transform.position, path.vectorPath[currentWaypoint], speed*deltaTime);
				transform.Translate(direction);

                //Check if we are close enough to the next waypoint
                //If we are, proceed to follow the next waypoint
                if (Vector3.Distance(transform.position, path.vectorPath [currentWaypoint]) < nextWaypointDistance) {
                    currentWaypoint++;
                    return;
                }
            }
        }
    }

    public override void TakeDamage(int damage) {
        base.TakeDamage(damage);
    }

//    TODO: Make attention work
    public virtual void ScanForEnemies() {
//ß        int layerMask = RTSGameMechanics.GetAttentionPhysicsLayer(this.gameObject.layer);
      	Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10);
        if(hitColliders.Length > 0){
//			Debug.Log ("---------------Found Enemy---------");
//          currentTarget = hitColliders[0].gameObject;
//          attacking = true;

			foreach(Collider obj in hitColliders) {
				if(obj.transform.tag == "Kill" && obj.gameObject.layer != this.gameObject.layer) {
					Debug.Log ("---------------Found Enemy---------");
					currentTarget = obj.gameObject;
					attacking = true;
					idle = false;
				}
			}
      }
    }

    //TODO: Handle Getting Attacked
}
