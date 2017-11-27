using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManScript : MonoBehaviour {
    // The point to move to
    public Vector3 destination;
    private Seeker seeker;
    private CharacterController controller;
    // The calculated path
    public Path path;
    // The AI's speed in meters per second
    public float speed = 2;
    // The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 0.5f;
    // The waypoint we are currently moving towards
    private int currentWaypoint = 0;
    // How often to recalculate the path (in seconds)
    public float repathRate = 1f;
    private float lastRepath = -9999;

    // Use this for initialization
    void Start () {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        destination = transform.position;
        seekNewPath();
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.time - lastRepath > repathRate && seeker.IsDone()) {
            lastRepath = Time.time + Random.value * repathRate * 0.5f;
            // Start a new path to the targetPosition, call the the OnPathComplete function
            // when the path has been calculated (which may take a few frames depending on the complexity)
            seeker.StartPath(transform.position, destination, OnPathComplete);
        }
        if (path == null) {
            // We have no path to follow yet, so don't do anything
            return;
        }
        if (currentWaypoint == path.vectorPath.Count) {
            seekNewPath();
            return;
        }
        // Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        dir *= speed;
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        controller.SimpleMove(dir);
        // The commented line is equivalent to the one below, but the one that is used
        // is slightly faster since it does not have to calculate a square root
        //if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
        if ((transform.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance) {
            currentWaypoint++;
        }
    }

    private void seekNewPath() {
        var randomCircle = Random.insideUnitCircle;
        destination = new Vector3(randomCircle.x * 60f, 0, randomCircle.y * 60f) + transform.position;
        destination.x = Mathf.Clamp(destination.x, -98, 98);
        destination.z = Mathf.Clamp(destination.z, -98, 98);
        Debug.Log(destination);
        seeker.StartPath(transform.position, destination, OnPathComplete);
        path = null;
    }

    private void OnPathComplete(Path p) {
        if (!p.error) {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        } else {
            //seekNewPath();
        }
    }
}
