using Assets.Scripts.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileScript : MonoBehaviour {
    private const int kExpectedNumberOfVertices = 5;

    public IEnumerable<TileScript> directNeighbours { get; set; }

    public IEnumerable<TileScript> indirectNeighbours { get; set; }

    public IEnumerable<TileScript> neighbours {
        get {
            return directNeighbours.Concat(indirectNeighbours).Concat(new []{this});
        }
    }

    public BoardScript board { get; set; }

    public List<Vector3> vertices {
        get {
            var vertices = new List<Vector3>();
            transform.GetComponent<MeshFilter>().mesh.GetVertices(vertices);
            return vertices;
        }

        set {
            Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
            var mesh = transform.GetComponent<MeshFilter>().mesh;
            mesh.SetVertices(value);
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
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

    private void OnMouseOver() {
        if (Input.GetMouseButton(0)) {
            board.tileWasPressed(this, 0);
        } else if (Input.GetMouseButton(1)) {
            board.tileWasPressed(this, 1);
        }
    }

    static public IEnumerable<Vector3> changeAllVerticesHeight(List<Vector3> vertices, 
        float heightChange) {
        Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
        return vertices.Select(vertex => vertex + new Vector3(0, heightChange, 0));
    }

    static public IEnumerable<Vector3> flattenVertices(List<Vector3> vertices,
        float heightChange, bool moveUp) {
        Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
        float min = vertices.Min(vertex => vertex.y);
        float max = vertices.Max(vertex => vertex.y);
        float difference = max - min;
        if (difference == 0f) {
            return vertices;
        }
        float goal = moveUp ? max : min;
        return vertices.Select(vertex => {
            var result = vertex.y + ((goal - vertex.y) * heightChange) / difference;
            result = Mathf.Clamp(result, min, max);
            return new Vector3(vertex.x, result, vertex.z);
        });
    }

    static private List<Vector3> getCorners(List<Vector3> vertices) {
        Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
        return new List<Vector3> {
            vertices[0],
            vertices[1],
            vertices[3],
            vertices[4]
        };
    }

    static public List<Vector3> adjustedVertices(List<Vector3> vertices, List<Vector3> templateVertices) {
        var adjustedVertices = vertices.Select(vertex => {
            Vector3 template = templateVertices.FirstOrDefault(templateVertex => templateVertex.x == vertex.x && templateVertex.z == vertex.z);
            return template != default(Vector3) ? template : vertex;
        }).ToList();

        adjustedVertices[2] = getCorners(adjustedVertices).Aggregate((sum, corner) => sum + corner) / 4;
        return adjustedVertices;
    }
}
