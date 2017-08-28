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
        var corners = getCorners(vertices);
        var newCorners = adjustCorners(heightChange, offset, corners);
        var newVertices = adjustVertices(vertices, newCorners);
        mesh.SetVertices(newVertices.ToList());
    }

    public IEnumerable<Vector3> adjustCorners(float heightChange, Vector3 offset, IEnumerable<Vector3> corners) {
        return corners.Select(corner => {
            bool xCompatible = offset.x == 0 || Math.Sign(offset.x) == Math.Sign(corner.x);
            bool yCompatible = offset.z == 0 || Math.Sign(offset.z) == Math.Sign(corner.z);
            if (xCompatible && yCompatible) {
                var result = corner + new Vector3(0, heightChange, 0);
                return result;
            }
            return corner;
        });
    }

    public IEnumerable<Vector3> adjustVertices(IEnumerable<Vector3> vertices, IEnumerable<Vector3> adjustedCorners) {
        var center = adjustedCorners.Aggregate((sum, corner) => sum + corner) / 4;
        return vertices.Select(vertex => {
            var distances = adjustedCorners.OrderBy(corner => twoDimensionalDistance(corner, vertex)).ToList();
            Debug.Log(vertex + ": " + string.Join(", ", distances));
            return bariocentricInterpolation(vertex, center, distances[0], distances[1]);
        });
    }

    private Vector3 bariocentricInterpolation(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3) {
        var f1 = p1-point;
        f1.y = 0;
        var f2 = p2-point;
        f2.y = 0;
        var f3 = p3-point;
        f3.y = 0;
        // calculate the areas and factors (order of parameters doesn't matter):
        var a = Vector3.Cross(p1-p2, p1-p3).magnitude; // main triangle area a
        var a1 = Vector3.Cross(f2, f3).magnitude / a; // p1's triangle area / a
        var a2 = Vector3.Cross(f3, f1).magnitude / a; // p2's triangle area / a 
        var a3 = Vector3.Cross(f1, f2).magnitude / a; // p3's triangle area / a
        // find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
        point.y = a1 * p1.y + a2 * p2.y + a3 * p3.y;
        return point;
    } 

    private float twoDimensionalDistance(Vector3 first, Vector3 second) {
        first.y = 0;
        second.y = 0;
        return Vector3.Distance(first, second);
    }

    // Use this for initialization
    void Start () {
        var mesh = transform.GetComponent<MeshFilter>().mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        List<Vector3> myVertices = new List<Vector3>();
        mesh.GetVertices(myVertices);
        sqrtOfMesh = Convert.ToInt32(Math.Sqrt(myVertices.Count));
    }

    private List<Vector3> getCorners(List<Vector3> vertices) {
        return new List<Vector3> { 
            vertices[0], 
            vertices[sqrtOfMesh - 1], 
            vertices[vertices.Count - sqrtOfMesh], 
            vertices[vertices.Count - 1]
        };
    }
	
	// Update is called once per frame
	void Update () {
        if (heightChange != 0) {
            float change = heightChange * Time.deltaTime;
            updateVertices(change, Vector3.zero);
            foreach (var tile in directNeighbours) {
                tile.updateVertices(change, tile.transform.position - transform.position);
            }
            foreach(var tile in indirectNeighbours) {
                tile.updateVertices(change, tile.transform.position - transform.position);
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
