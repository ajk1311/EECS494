using UnityEngine;
using System.Collections;
using RTS;
using Pathfinding;

public class Unit : WorldObject {

    protected Vector3 destination;
    protected bool pathComplete = false;
    protected bool moving = false;
    protected bool attacking = false;
    protected Seeker seeker;
    protected CharacterController characterController;
    protected Path path;
    public int currentWaypoint = 0;
    public float nextWaypointDistance = 3;
    public float speed = 100;

    void Awake() {
        base.Awake();
    }
    // Use this for initialization
    void Start() {
        base.Start();
        seeker = GetComponent<Seeker>();
        characterController = GetComponent<CharacterController>();
    }
    
    // Update is called once per frame
    void Update() {
        base.Update();
        if(moving && pathComplete){
            CalculateBounds();
        }
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

    public void startMovement(Vector3 destination){
        seeker.StartPath(transform.position, destination, OnPathComplete);
        moving = true;
    }

    public void OnPathComplete(Path p) {
        if(!p.error) {
            path = p;
            currentWaypoint = 0;
            pathComplete = true;
        } else
            pathComplete = false;
    }

    public void ReachedDestination(){
        pathComplete = false;
        moving = false;
    }

    public void FixedUpdate() {
        if(!moving){
            return;
        }
        if(pathComplete && moving) {
            if(currentWaypoint >= path.vectorPath.Count){
                ReachedDestination();
                return;
            }

            Vector3 direction = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            direction *= speed * Time.fixedDeltaTime;
            characterController.Move(direction);

            //Check if we are close enough to the next waypoint
            //If we are, proceed to follow the next waypoint
            if(Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
                currentWaypoint++;
                return;
            }
        }
    }
}
