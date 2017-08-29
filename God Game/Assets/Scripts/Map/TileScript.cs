using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileScript : MonoBehaviour {

    private int heightChange;

    private int sqrtOfMesh;

    public IEnumerable<TileScript> directNeighbours { get; set; }

    public IEnumerable<TileScript> indirectNeighbours { get; set; }

    public void updateVertices(float heightChange, Vector3 offset) {
        var vertices = new List<Vector3>();
        var mesh = transform.GetComponent<MeshFilter>().mesh;
        mesh.GetVertices(vertices);
        var newVertices = adjustVertices(vertices, heightChange, offset);
        mesh.SetVertices(newVertices.ToList());
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public IEnumerable<Vector3> adjustVertices(IEnumerable<Vector3> vertices, float heightChange, Vector3 offset) {
        var adjustedVertices = vertices.Select(vertex => {
            bool xCompatible = offset.x == 0 || Math.Sign(offset.x) == Math.Sign(vertex.x);
            bool yCompatible = offset.z == 0 || Math.Sign(offset.z) == Math.Sign(vertex.z);
            if (xCompatible && yCompatible) {
                var result = vertex + new Vector3(0, heightChange, 0);
                return result;
            }
            return vertex;
        }).ToList();

        adjustedVertices[2] = getCorners(adjustedVertices).Aggregate((sum, corner) => sum + corner) / 4;
        return adjustedVertices;
    }

    // Use this for initialization
    void Start () {
        var mesh = transform.GetComponent<MeshFilter>().mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.Clear();
        mesh.SetVertices(new List<Vector3> {
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, -5),
            new Vector3(0, 0, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        });
        mesh.uv = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0.5f, 0.5f),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.triangles = new int[] {
            2, 1, 0,
            3, 2, 0,
            1, 2, 4,
            2, 3, 4
        };
    }

    private List<Vector3> getCorners(List<Vector3> vertices) {
        return new List<Vector3> { 
            vertices[0], 
            vertices[1], 
            vertices[3], 
            vertices[4]
        };
    }
	
	// Update is called once per frame
	void Update () {
        if (heightChange != 0) {
            float change = heightChange * Time.deltaTime;
            updateVertices(change, Vector3.zero);
            foreach (var tile in directNeighbours) {
                tile.updateVertices(change, transform.position - tile.transform.position);
            }
            foreach(var tile in indirectNeighbours) {
                tile.updateVertices(change, transform.position - tile.transform.position);
            }
            heightChange = 0;
        }
	}

    private void OnMouseOver() {
        if (Input.GetMouseButton(0)) {
            heightChange = 10;
        }
        if (Input.GetMouseButton(1)) {
            heightChange = -10;
        }
    }

    private void OnMouseExit() {
        heightChange = 0;
    }
}
