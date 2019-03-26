using Assets.Scripts.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public struct Tile {
	private const int kExpectedNumberOfVertices = 5;

	public MeshFilter meshFilter;
	public MeshCollider meshCollider;
	public NativeArray<Vector3> vertices;
	public NativeArray<Tile> neighbours;

	private void updateMesh(Mesh newMesh) {
		newMesh.RecalculateNormals();
		meshFilter.mesh = newMesh;
		meshCollider.sharedMesh = newMesh;
	}

	private void updateVertices(NativeArray<Vector3> newVertices) {
		var oldVertices = vertices;
		var internalMesh = meshFilter.mesh;
		internalMesh.vertices = newVertices.ToArray();
		updateMesh(internalMesh);
		oldVertices.Dispose();
	}
}

public class TileScript: MonoBehaviour {
  private const int kExpectedNumberOfVertices = 5;

  public IEnumerable<TileScript> directNeighbours { get; set; }

  public IEnumerable<TileScript> indirectNeighbours { get; set; }

  public IEnumerable<TileScript> neighbours {
    get {
      return directNeighbours.Concat(indirectNeighbours).Concat(new[] { this });
    }
  }

  private MeshFilter meshFilter;
  private MeshCollider meshCollider;

  private Mesh internalMesh {
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

  static public IEnumerable<Vector3> transformedVectorsWithDistance(List<Vector3> vertices, Vector3 source, Func<Vector3, float, Vector3> vectorAndDistanceToVector) {
    return vertices.Select(vertex => {
      return vectorAndDistanceToVector(vertex, vertex.DistanceIn2D(source));
    });
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
    var distances = new float[kExpectedNumberOfVertices];

    var closestDistances = new [] { 1000f, 1000f, 1000f};
    var indicesOfNearestPoints = new int[3];
    for (int i = 0; i < kExpectedNumberOfVertices; i++) {
      var vertex = vertices[i];
      vertex.y = 0;
      var distance = Vector3.Distance(vertex, positionWithoutHeight);
      distances[i] = distance;
      if (distance < closestDistances[0]) {
        closestDistances[2] = closestDistances[1];
        closestDistances[1] = closestDistances[0];
        closestDistances[0] = distance;
        indicesOfNearestPoints[2] = indicesOfNearestPoints[1];
        indicesOfNearestPoints[1] = indicesOfNearestPoints[0];
        indicesOfNearestPoints[0] = i;
      } else if (distance < closestDistances[1]) {
        closestDistances[2] = closestDistances[1];
        closestDistances[1] = distance;
        indicesOfNearestPoints[2] = indicesOfNearestPoints[1];
        indicesOfNearestPoints[1] = i;
      } else if (distance < closestDistances[2]) {
        closestDistances[2] = distance;
        indicesOfNearestPoints[2] = i;
      }
    }

    var newHeight = 0f;
    var newNormal = Vector3.zero;
    var sumOfDistances = 0f;
    

    foreach (var index in indicesOfNearestPoints) {
      var distanceAsWeight = 1f / distances[index];
      sumOfDistances += distanceAsWeight;
      newHeight += vertices[index].y * distanceAsWeight;
      newNormal += normals[index] * distanceAsWeight;
    }

    child.transform.localPosition = new Vector3(positionWithoutHeight.x, newHeight / sumOfDistances, positionWithoutHeight.z);
    child.rotation = Quaternion.FromToRotation(Vector3.up, newNormal / sumOfDistances);
  }
}
