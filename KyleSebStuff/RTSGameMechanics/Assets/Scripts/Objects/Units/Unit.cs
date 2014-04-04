using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

[RequireComponent(typeof(CharacterController))]
public class Unit : WorldObject {

    protected Vector3 destination;
    protected bool pathComplete = false;
    protected bool moving = false;
    protected bool attacking = false;
    protected Seeker seeker;
    protected CharacterController characterController;
    protected Path path;
    protected Vector3 oldEnemyPosition;
    public GameObject currentTarget = null;
    public GameObject PROJECTILE;
    public int currentWaypoint = 0;
    public float nextWaypointDistance = 3;
    public float speed = 50;
    public float attackRange = 10f;
    public bool reloading = false;

    protected override void Awake() {
        base.Awake();
    }
    // Use this for initialization
    protected override void Start() {
        base.Start();
        seeker = GetComponent<Seeker>();
        characterController = GetComponent<CharacterController>();
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
    }

    protected virtual void Pursuit() {
        if (WithinAttackRange()) {
            if (!reloading)
                AttackHandler();
        } else {
            if (oldEnemyPosition != currentTarget.transform.position) {
                StartMovement(currentTarget.transform.position);
                oldEnemyPosition = currentTarget.transform.position;
            }
        }
    }

    protected virtual void Reload() {
        reloading = false;
    }

    protected virtual void AttackHandler() {
        //Stop moving in order to attack
        moving = false;
        Vector3 projectilePosition = Vector3.MoveTowards(transform.position, currentTarget.transform.position, 1f);
        projectilePosition.y += 1;
        GameObject projectile = (GameObject)Instantiate(PROJECTILE, projectilePosition, Quaternion.identity);
        projectile.transform.parent = this.transform;
        reloading = true;
        Invoke("Reload", .75f);
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
            currentWaypoint = 0;
            pathComplete = true;
        } else
            pathComplete = false;
    }

    public void ReachedDestination() {
        pathComplete = false;
        moving = false;
    }

	public override void GameUpdate(float deltaTime) {
		base.GameUpdate(deltaTime);

		if (moving && pathComplete) {
			CalculateBounds();
		}

        if (attacking && currentTarget) {
            Pursuit();
        }

        if (!moving) {
            return;
        }
        if (pathComplete && moving) {
            if (currentWaypoint >= path.vectorPath.Count) {
                ReachedDestination();
                return;
            }

            Vector3 direction = (path.vectorPath [currentWaypoint] - transform.position).normalized;
            direction *= speed * deltaTime;
            characterController.Move(direction);

            //Check if we are close enough to the next waypoint
            //If we are, proceed to follow the next waypoint
            if (Vector3.Distance(transform.position, path.vectorPath [currentWaypoint]) < nextWaypointDistance) {
                currentWaypoint++;
                return;
            }
        }
    }
}
