using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManScript : MonoBehaviour {
    private Seeker seeker;
    private Vector3 destination;

	// Use this for initialization
	void Start () {
        seeker = GetComponent<Seeker>();
	}
	
	// Update is called once per frame
	void Update () {
        if (destination != null && Vector3.Distance(transform.position, destination) > 0.5)
            return;

        var randomCircle = Random.insideUnitCircle;
        destination = new Vector3(randomCircle.x * 200f, 0, randomCircle.y * 200f);
        seeker.StartPath(transform.position, destination, OnPathComplete);
    }

    private void OnPathComplete() {
    }
}
