﻿using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

  private BoardScript board;

  // Use this for initialization
  void Start() {
    seeker = GetComponent<Seeker>();
    controller = GetComponent<CharacterController>();
    destination = transform.position;
    // seekNewPath();
    board = FindObjectOfType<BoardScript>();
  }

  // Update is called once per frame
  void Update() {
    //if (Time.time - lastRepath > repathRate && seeker.IsDone() && !destination.Equals(transform.position)) {
    //  lastRepath = Time.time + Random.value * repathRate * 0.5f;
    //  // Start a new path to the targetPosition, call the the OnPathComplete function
    //  // when the path has been calculated (which may take a few frames depending on the complexity)
    //  seeker.StartPath(transform.position, destination, OnPathComplete);
    //}
    if (path == null) {
      // We have no path to follow yet, so don't do anything
      return;
    }
    if (currentWaypoint == path.vectorPath.Count) {
      //seekNewPath();
      return;
    }
    // Direction to the next waypoint
    Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
    dir *= speed;
    
    transform.position += dir * Time.deltaTime;
    // The commented line is equivalent to the one below, but the one that is used
    // is slightly faster since it does not have to calculate a square root
    //if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
    if ((transform.position - path.vectorPath[currentWaypoint]).sqrMagnitude < nextWaypointDistance * nextWaypointDistance) {
      currentWaypoint++;
    }
  }

  //private void seekNewPath() {
  //  var randomCircle = Random.insideUnitCircle;
  //  destination = new Vector3(randomCircle.x * 60f, 0, randomCircle.y * 60f) + transform.position;
  //  destination.x = Mathf.Clamp(destination.x, -198, 198);
  //  destination.z = Mathf.Clamp(destination.z, -198, 198);
  //  seeker.StartPath(transform.position, destination, OnPathComplete);
  //  Debug.Log(this.name + " is going to: " + destination);
  //  path = null;
  //}

  public void seekPath(Vector3 destination) {
    if (this.destination.Equals(destination)) {
      return;
    }
    this.destination = destination;
    seeker.StartPath(transform.position, destination, OnPathComplete);
    Debug.Log(this.name + " is going to: " + destination);
    path = null;
  }

  private void OnPathComplete(Path p) {
    if (!p.error) {
      path = p;
      Debug.Log(this.name + " received path to: " + path.vectorPath.Last());
      // Reset the waypoint counter so that we start to move towards the first point in the path
      currentWaypoint = 0;
    } else {
      //seekNewPath();
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (other.tag != "Tiles") {
      return;
    }
    transform.parent = other.transform;
  }
}
