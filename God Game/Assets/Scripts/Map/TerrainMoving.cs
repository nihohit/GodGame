using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

class TileMovingIdentifier {
  public int xCoord;
  public int yCoord;
}

public interface ITileMoving : IJob {
  NativeSlice<bool> WasChanged { get; }
  NativeArray<float3> Vertices { get; }
}

[BurstCompile]
struct ElevateVertices : ITileMoving {
  public NativeArray<float3> vertices;

  [ReadOnly]
  public float3 centerOfAction;

  [ReadOnly]
  public float maximumChangeDistance;

  [ReadOnly]
  public float deltaTime;

  [ReadOnly]
  public float3 actionDirection;

  [ReadOnly]
  public float heightChangeRate;

  [WriteOnly]
  public NativeSlice<bool> wasChanged;

  public NativeSlice<bool> WasChanged { get { return wasChanged; } }
  public NativeArray<float3> Vertices { get { return vertices; } }

  public void Execute() {
    wasChanged[0] = false;
    for (var i = 0; i < vertices.Length; i++) {
      var vertex = vertices[i];
      var distance = math.distance(vertex, centerOfAction) / Constants.SizeOfTile;
      if (distance > maximumChangeDistance) {
        continue;
      }
      wasChanged[0] = true;
      var intensity = math.cos(distance / maximumChangeDistance) * heightChangeRate;
      vertices[i] = vertex + (actionDirection * intensity * deltaTime);
    }
  }
}

[BurstCompile]
struct FlattenVertices : ITileMoving {
  public NativeArray<float3> vertices;

  [ReadOnly]
  public float3 centerOfAction;

  [ReadOnly]
  public float maximumChangeDistance;

  [ReadOnly]
  public float deltaTime;

  [WriteOnly]
  public NativeSlice<bool> wasChanged;

  public NativeSlice<bool> WasChanged { get { return wasChanged; } }
  public NativeArray<float3> Vertices { get { return vertices; } }

  public void Execute() {
    wasChanged[0] = false;
    for (var i = 0; i < vertices.Length; i++) {
      var vertex = vertices[i];
      var distance = math.distance(vertex, centerOfAction) / Constants.SizeOfTile;
      if (distance > maximumChangeDistance) {
        continue;
      }
      wasChanged[0] = true;
      var intensity = math.cos(distance / maximumChangeDistance) * deltaTime;
      vertex.y = Mathf.Lerp(vertex.y, centerOfAction.y, intensity);
      vertices[i] = vertex;
    }
  }
}