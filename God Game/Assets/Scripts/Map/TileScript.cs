using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour {

    private int heightChange;

    public IEnumerable<TileScript> directNeighbours { get; set; }

    public IEnumerable<TileScript> indirectNeighbours { get; set; }

    public void updateVertices(float heightChange, Vector3 offset) {
        Debug.Log(transform.name);
        List<Vector3> myVertices = new List<Vector3>();
        List<Vector3> newVertices = new List<Vector3>();
        var filter = transform.GetComponent<MeshFilter>();
        var mesh = filter.mesh;
        mesh.GetVertices(myVertices);
        foreach (var vertex in myVertices) {
            var offsetedVertex = vertex + offset;
            int index = myVertices.IndexOf(offsetedVertex);
            if (index >= 0) {
                newVertices.Add(vertex + new Vector3(0, heightChange, 0));
            } else {
                newVertices.Add(vertex);
            }
        }
        mesh.SetVertices(newVertices);

        Debug.Log(string.Join(", ", mesh.vertices));
        Debug.Log(string.Join(", ", GetComponent<MeshCollider>().sharedMesh.vertices));
    }

    // Use this for initialization
    void Start () {
        GetComponent<MeshCollider>().sharedMesh = transform.GetComponent<MeshFilter>().mesh;
    }
	
	// Update is called once per frame
	void Update () {
        if (heightChange != 0) {
            float change = heightChange * Time.deltaTime;
            updateVertices(heightChange, Vector3.zero);
            foreach (var tile in directNeighbours) {
                tile.updateVertices(heightChange, tile.transform.position - transform.position);
            }
            foreach(var tile in indirectNeighbours) {
                tile.updateVertices(heightChange, tile.transform.position - transform.position);
            }
            heightChange = 0;
        }
	}

    private void OnMouseOver() {
        if (Input.GetMouseButton(0)) {
            heightChange = 1;
        }
        if (Input.GetMouseButton(1)) {
            heightChange = -1;
        }
    }

    private void OnMouseExit() {
        heightChange = 0;
    }
}
