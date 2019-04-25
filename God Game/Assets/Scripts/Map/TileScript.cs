using Assets.Scripts.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

[BurstCompile]
public struct AdjustChildrenJob : IJobParallelForTransform {
  [ReadOnly]
  public NativeArray<float3> vertices;
  [ReadOnly]
  public NativeArray<float3> normals;

  [BurstCompile]
  public void Execute(int runIndex, TransformAccess child) {
    var positionWithoutHeight = math.float3(child.localPosition.x, 0, child.localPosition.z);

    var distances = new NativeArray<float>(Constants.NumberOfVerticesInTile, Allocator.Temp);

    var closestDistances = new NativeArray<float>(3, Allocator.Temp);
    closestDistances[0] = 1000f;
    closestDistances[1] = 1000f;
    closestDistances[2] = 1000f;
    var indicesOfNearestPoints = new NativeArray<int>(3, Allocator.Temp);
    for (int i = 0; i < Constants.NumberOfVerticesInTile; i++) {
      var vertex = vertices[i];
      vertex.y = 0;
      var distance = math.distance(vertex, positionWithoutHeight);
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
    var newNormal = math.float3(0);
    var sumOfDistances = 0f;


    for (int i = 0; i < indicesOfNearestPoints.Length; i++) {
      var index = indicesOfNearestPoints[i];
      var distanceAsWeight = 1f / distances[index];
      sumOfDistances += distanceAsWeight;
      newHeight += vertices[index].y * distanceAsWeight;
      newNormal += normals[index] * distanceAsWeight;
    }

    child.localPosition = new Vector3(positionWithoutHeight.x, newHeight / sumOfDistances, positionWithoutHeight.z);
    var groundNormal = newNormal / sumOfDistances;
    var right = child.rotation * Vector3.right;
    Vector3 forwardsVector = -Vector3.Cross(groundNormal, right);
    child.localRotation = Quaternion.LookRotation(forwardsVector, groundNormal);
    indicesOfNearestPoints.Dispose();
    distances.Dispose();
    closestDistances.Dispose();
  }
}

public class TileScript : MonoBehaviour {
  private readonly List<Vector3> internalVertices = new List<Vector3>(Constants.NumberOfVerticesInTile);
  private readonly List<Vector3> internalNormals = new List<Vector3>();

  public NativeArray<float3> nativeVertices;
  public NativeArray<float3> nativeNormals;
  public TransformAccessArray transforms;
  public float3 Position;

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
      internalNormals.Clear();
      internalVertices.Clear();
      value.RecalculateNormals();
      meshFilter.mesh = value;
      meshCollider.sharedMesh = value;
    }
  }

  public List<Vector3> vertices {
    get {
      if (internalVertices.Count == 0) {
        internalMesh.GetVertices(internalVertices);
      }
      return internalVertices;
    }

    set {
      internalVertices.AddRange(value);
      internalMesh.SetVertices(value);
      internalMesh = internalMesh;
    }
  }

  public List<Vector3> normals {
    get {
      if (internalNormals.Count == 0) {
        internalMesh.GetNormals(internalNormals);
      }
      return internalNormals;
    }
  }

  private void Awake() {
    meshFilter = GetComponent<MeshFilter>();
    meshCollider = GetComponent<MeshCollider>();
    nativeVertices = new NativeArray<float3>(Constants.NumberOfVerticesInTile, Allocator.Persistent);
    nativeNormals = new NativeArray<float3>(Constants.NumberOfVerticesInTile, Allocator.Persistent);
    transforms = new TransformAccessArray(9);
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

    for (int index = 0; index < Constants.NumberOfVerticesInTile; index++) {
      nativeVertices[index] = vertices[index].ToSlim();
    }
    Position = transform.position;
  }

  static public IEnumerable<Vector3> transformedVectorsWithDistance(List<Vector3> vertices, Vector3 source, Func<Vector3, float, Vector3> vectorAndDistanceToVector) {
    return vertices.Select(vertex => {
      return vectorAndDistanceToVector(vertex, vertex.DistanceIn2D(source));
    });
  }

  static public IEnumerable<Vector3> changeAllVerticesHeight(List<Vector3> vertices,
      float heightChange) {
    Assert.AreEqual(vertices.Count, Constants.NumberOfVerticesInTile);
    return vertices.Select(vertex => vertex + new Vector3(0, heightChange, 0));
  }

  static public IEnumerable<Vector3> flattenVertices(List<Vector3> vertices,
      float heightChange, TileUpdateDirection direction) {
    Assert.AreEqual(vertices.Count, Constants.NumberOfVerticesInTile);
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
    Assert.AreEqual(vertices.Count, Constants.NumberOfVerticesInTile);
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

  public static JobHandle adjustChildrenLocation(TileScript tile, JobHandle dependency) {
    var childCount = tile.transform.childCount;
    for (int i = 0; i < tile.transforms.length; i++) {
      tile.transforms.RemoveAtSwapBack(0);
    }
    for (int i = 0; i < childCount; i++) {
      tile.transforms.Add(tile.transform.GetChild(i));
    }

    tile.normals.ConvertInto(tile.nativeNormals);
    var adjustLocationJob = new AdjustChildrenJob {
      vertices = tile.nativeVertices,
      normals = tile.nativeNormals
    };
    return adjustLocationJob.Schedule(tile.transforms, dependency);
  }

  public void OnDestroy() {
    nativeNormals.Dispose();
    nativeVertices.Dispose();
    transforms.Dispose();
  }
}
