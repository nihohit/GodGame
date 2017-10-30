using Assets.Scripts.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ManScript : MonoBehaviour {

    private NavMeshAgent agent;
    private BoardScript board;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        board = FindObjectOfType<BoardScript>();
    }
	
	// Update is called once per frame
	void Update () {
        if (agent.pathPending || agent.remainingDistance > 0.1f)
            return;
        var randomCircle = Random.insideUnitCircle;
        agent.destination = new Vector3(randomCircle.x * 200f, 0, randomCircle.y * 200f);
    }

    private void OnCollisionEnter(Collision collision) {
        TileScript tile = collision.gameObject.GetComponent<TileScript>();
        if (tile) {
            transform.parent = tile.transform;
        }
    }
}
