using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    public IEnumerable<TileScript> directNeighbours { get; set; }

    public IEnumerable<TileScript> indirectNeighbours { get; set; }

    // Use this for initialization
    void Start () {
        GetComponent<MeshCollider>().sharedMesh = transform.GetComponent<MeshFilter>().mesh;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
