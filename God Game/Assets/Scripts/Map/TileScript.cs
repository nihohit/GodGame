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
      return directNeighbours.Concat(indirectNeighbours).Concat(new[] { this });
    }
  }

  public BoardScript board { get; set; }

  private MeshFilter meshFilter;
  private MeshCollider meshCollider;

  public Mesh internalMesh {
    get {
      return meshFilter.mesh;
    }
    set {
      value.RecalculateNormals();
      meshFilter.mesh = value;
      meshCollider.sharedMesh = value;
    }
  }

  public List<Vector3> vertices {
    get {
      var vertices = new List<Vector3>();
      internalMesh.GetVertices(vertices);
      return vertices;
    }

    set {
      Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
      internalMesh.SetVertices(value);
      internalMesh = internalMesh;
    }
  }

  private void Awake() {
    meshFilter = GetComponent<MeshFilter>();
    meshCollider = GetComponent<MeshCollider>();
  }

  // Use this for initialization
  void Start() {
    var mesh = internalMesh;
    mesh.Clear();
    var halfSize = Constants.SizeOfTile / 2;
    mesh.SetVertices(new List<Vector3> {
      new Vector3(-halfSize, 0, -halfSize),
      new Vector3(halfSize, 0, -halfSize),
      new Vector3(0, 0, 0),
      new Vector3(-halfSize, 0, halfSize),
      new Vector3(halfSize, 0, halfSize)
    });
    mesh.uv = new Vector2[] {new Vector2(0, 0),
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
    internalMesh = mesh;
  }

  static public IEnumerable<Vector3> changeAllVerticesHeight(List<Vector3> vertices,
      float heightChange) {
    Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
    return vertices.Select(vertex => vertex + new Vector3(0, heightChange, 0));
  }

  static public IEnumerable<Vector3> flattenVertices(List<Vector3> vertices,
      float heightChange, TileUpdateDirection direction) {
    Assert.AreEqual(vertices.Count, kExpectedNumberOfVertices);
    float min = vertices.Min(vertex => vertex.y);
    float max = vertices.Max(vertex => vertex.y);
    if (Mathf.Approximately(min, max)) {
      return vertices;
    }
    float difference = max - min;
    float goal = direction == TileUpdateDirection.Up ? max : min;
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
      Vector3 template = templateVertices.FirstOrDefault(templateVertex => Mathf.Approximately(templateVertex.x, vertex.x) && Mathf.Approximately(templateVertex.z, vertex.z));
      return template != default(Vector3) ? template : vertex;
    }).ToList();

    adjustedVertices[2] = centerFromCorners(getCorners(adjustedVertices));
    return adjustedVertices;
  }

  public static Vector3 centerFromCorners(IEnumerable<Vector3> corners) {
    return corners.Aggregate((sum, corner) => sum + corner) / 4;
  }

  public static void adjustChildrenLocation(TileScript tile) {
    foreach (Transform child in tile.transform) {
      setPositionAndNormal(tile, child);
    }
  }

  private static void setPositionAndNormal(TileScript tile, Transform child) {
    var positionWithoutHeight = child.transform.localPosition;
    positionWithoutHeight.y = 0;

    var vertices = tile.vertices;
    var normals = tile.meshFilter.mesh.normals;
    var distances = vertices
      .Select(vertex => {
        var vertexWithoutHeight = new Vector3(vertex.x, 0, vertex.z);
        return Vector3.Distance(vertexWithoutHeight, positionWithoutHeight);
      })
      .ToList();
    var indicesOfNearestPoints = distances
      .OrderBy(distance => distance)
      .Take(3)
      .Select(distance => distances.IndexOf(distance));

    var newHeight = 0f;
    var newNormal = Vector3.zero;
    var sumOfDistances = 0f;

    foreach (var index in indicesOfNearestPoints) {
      var distanceAsWeight = 1 / distances[index];
      sumOfDistances += distanceAsWeight;
      newHeight += vertices[index].y * distanceAsWeight;
      newNormal += normals[index] * distanceAsWeight;
    }

    child.transform.localPosition = new Vector3(positionWithoutHeight.x, newHeight / sumOfDistances, positionWithoutHeight.z);
    child.rotation = Quaternion.FromToRotation(Vector3.up, newNormal / sumOfDistances);
  }
}
