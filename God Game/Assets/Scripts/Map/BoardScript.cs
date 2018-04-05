using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class BoardScript : MonoBehaviour {
  public int heightChangeRate = 20;
  public int x, z;
  public bool flatten, changeHeight;

  private TileUpdateType updateType;
  private MeshFilter filter;
  private MeshCollider meshCollider;

  private void Awake() {
    filter = GetComponent<MeshFilter>();
    GetComponent<MeshRenderer>().material.SetFloat("_TileSize", Constants.SizeOfTile);
    meshCollider = GetComponent<MeshCollider>();
  }

  void Start() {
    initializeMesh();
    addFeatures();
  }

  private void addFeatures() {
    GameObject treePrefab = (GameObject)Resources.Load("Prefabs/Trees/RegularTrees/tree001");
    GameObject manPrefab = (GameObject)Resources.Load("Prefabs/man");

    
    for (int i = 0; i <= x; i++) {
      for (int j = 0; j <= z; j++) {
        var tree = instantiateObject(treePrefab, Vector3.zero);
        tree.transform.parent = transform;
        tree.transform.localPosition = randomPositionOnTile(i, j);
        tree.AddComponent<TerrainObjectScript>();

        var man = instantiateObject(manPrefab, Vector3.zero);
        man.transform.parent = transform;
        man.transform.localPosition = randomPositionOnTile(i, j);
      }
    }
  }

  Vector3 randomPositionOnTile(int xCoord, int zCoord) {
    var halfSize = Constants.SizeOfTile / 2;
    var zOffset = (float)Assets.Scripts.Base.Randomizer.NextDouble(-halfSize, halfSize);
    var xOffset = (float)Assets.Scripts.Base.Randomizer.NextDouble(-halfSize, halfSize);
    return new Vector3(xOffset + xCoord * Constants.SizeOfTile, 0, zOffset + zCoord * Constants.SizeOfTile);
  }

  void initializeMesh() {
    var mesh = newMesh(x, z);
    filter.mesh = mesh;
    meshCollider.sharedMesh = mesh;
  }

  private static Mesh newMesh(int x, int z) {
    var vertices = new Vector3[(x + 1) * (z + 1) + (x * z)];
    var uv = new Vector2[(x + 1) * (z + 1) + (x * z)];
    var triangles = new int[12 * x * z];
    
    for (int i = 0; i <= x; i++) {
      for (int j = 0; j <= z; j++) {
        var vertexIndex = indexOfCornerVertex(i, j, x);
        vertices[vertexIndex] = new Vector3(i, 0, j) * Constants.SizeOfTile;
        uv[vertexIndex] = new Vector2(vertices[vertexIndex].x / x, vertices[vertexIndex].z / z) / Constants.SizeOfTile;
        if (i == x || j == z) {
          continue;
        }

        var centerIndex = indexOfCenterVertex(i, j, x);
        vertices[centerIndex] = new Vector3(i + 0.5f, 0, j + 0.5f) * Constants.SizeOfTile;
        uv[centerIndex] = new Vector2(vertices[centerIndex].x / x, vertices[centerIndex].z / z) / Constants.SizeOfTile;

        var nextRowVertex = centerIndex + x;
        var triangleIndex = (i * 12) + (j * x * 12);
        triangles[triangleIndex] = centerIndex;
        triangles[triangleIndex + 1] = vertexIndex + 1;
        triangles[triangleIndex + 2] = vertexIndex;
        triangles[triangleIndex + 3] = nextRowVertex;
        triangles[triangleIndex + 4] = centerIndex;
        triangles[triangleIndex + 5] = vertexIndex;
        triangles[triangleIndex + 6] = vertexIndex + 1;
        triangles[triangleIndex + 7] = centerIndex;
        triangles[triangleIndex + 8] = nextRowVertex + 1;
        triangles[triangleIndex + 9] = centerIndex;
        triangles[triangleIndex + 10] = nextRowVertex;
        triangles[triangleIndex + 11] = nextRowVertex + 1;
      }
    }

    var mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uv;
    mesh.RecalculateNormals();
    return mesh;
  }

  static int indexOfCornerVertex(int xCoord, int zCoord, int maxX) {
    return xCoord + (zCoord * ((2 * maxX) + 1));
  }

  static int indexOfCenterVertex(int xCoord, int zCoord, int maxX) {
    return indexOfCornerVertex(xCoord, zCoord, maxX) + maxX + 1;
  }

  private GameObject instantiateObject(UnityEngine.Object prefab, Vector3 position) {
    return (GameObject)Instantiate(prefab, position, Quaternion.identity);
  }

  // Update is called once per frame
  void Update() {
    if (Input.GetKey(KeyCode.Escape)) {
      Application.Quit();
      return;
    }

    TileUpdateDirection direction;

    if (Input.GetMouseButton(0)) {
      direction = TileUpdateDirection.Up;
    } else if (Input.GetMouseButton(1)) {
      direction = TileUpdateDirection.Down;
    } else {
      return;
    }

    RaycastHit hit = new RaycastHit();
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out hit, float.MaxValue, 1 << 8)) {
      return;
    }

    adjustVertices(hit.point, direction);
  }

  private void adjustVertices(Vector3 point, TileUpdateDirection direction) {
    var resizedPoint = point / Constants.SizeOfTile;
    var minX = Mathf.FloorToInt(resizedPoint.x);
    var minZ = Mathf.FloorToInt(resizedPoint.z);

    var vertices = filter.mesh.vertices;
    if (updateType == TileUpdateType.Flatten) {
      flattenVertices(minX, minZ, vertices, direction);
    } else {
      adjustCornersUniformly(minX, minZ, vertices, direction);
    }
    adjustCenters(minX, minZ, vertices);

    filter.mesh.SetVertices(vertices.ToList());
    filter.mesh.RecalculateNormals();
    meshCollider.sharedMesh = filter.mesh;
  }

  private void adjustCornersUniformly(int sourceX, int sourceZ, Vector3[] vertices, TileUpdateDirection direction) {
    var change = new Vector3(0, heightChangeRate * Time.deltaTime, 0);
    change = direction == TileUpdateDirection.Down ? -change : change;
    var maxX = sourceX + 1;
    var maxZ = sourceZ + 1;
    vertices[indexOfCornerVertex(sourceX, sourceZ, x)] += change;
    vertices[indexOfCornerVertex(maxX, sourceZ, x)] += change;
    vertices[indexOfCornerVertex(sourceX, maxZ, x)] += change;
    vertices[indexOfCornerVertex(maxX, maxZ, x)] += change;
  }

  private void flattenVertices(int sourceX, int sourceZ, Vector3[] vertices, TileUpdateDirection direction) {
    var heightChange = heightChangeRate * Time.deltaTime;
    var maxX = sourceX + 1;
    var maxZ = sourceZ + 1;
    var heights = new float[] {
      vertices[indexOfCornerVertex(sourceX, sourceZ, x)].y,
      vertices[indexOfCornerVertex(maxX, sourceZ, x)].y,
      vertices[indexOfCornerVertex(sourceX, maxZ, x)].y,
      vertices[indexOfCornerVertex(maxX, maxZ, x)].y
    };
    var min = heights.Min();
    var max = heights.Max();
    if (Mathf.Approximately(min, max)) {
      return;
    }
    float difference = max - min;
    float goal = direction == TileUpdateDirection.Up ? max : min;
    flattenVertex(sourceX, sourceZ, vertices, difference, goal, heightChange, min, max);
    flattenVertex(maxX, sourceZ, vertices, difference, goal, heightChange, min, max);
    flattenVertex(sourceX, maxZ, vertices, difference, goal, heightChange, min, max);
    flattenVertex(maxX, maxZ, vertices, difference, goal, heightChange, min, max);
  }

  void flattenVertex(int sourceX, int sourceZ, Vector3[] vertices, 
    float difference, float goal, float heightChange, float min, float max) {
    var vertex = vertices[indexOfCornerVertex(sourceX, sourceZ, x)];
    var result = vertex.y + ((goal - vertex.y) * heightChange) / difference;
    result = Mathf.Clamp(result, min, max);
    vertices[indexOfCornerVertex(sourceX, sourceZ, x)] = new Vector3(vertex.x, result, vertex.z);
  }

  private void adjustCenters(int sourceX, int sourceZ, Vector3[] vertices) {
    var minX = sourceX - 1;
    var minZ = sourceZ - 1;
    var maxX = sourceX + 1;
    var maxZ = sourceZ + 1;

    adjustCenter(minX, minZ, vertices);
    adjustCenter(minX, sourceZ, vertices);
    adjustCenter(minX, maxZ, vertices);
    adjustCenter(sourceX, minZ, vertices);
    adjustCenter(sourceX, sourceZ, vertices);
    adjustCenter(sourceX, maxZ, vertices);
    adjustCenter(maxX, minZ, vertices);
    adjustCenter(maxX, sourceZ, vertices);
    adjustCenter(maxX, maxZ, vertices);
  }

  private void adjustCenter(int minX, int minZ, Vector3[] vertices) {
    var maxX = minX + 1;
    var maxZ = minZ + 1;
    var accumulatedValues = Vector3.zero;
    accumulatedValues += vertices[indexOfCornerVertex(minX, minZ, x)];
    accumulatedValues += vertices[indexOfCornerVertex(maxX, minZ, x)];
    accumulatedValues += vertices[indexOfCornerVertex(minX, maxZ, x)];
    accumulatedValues += vertices[indexOfCornerVertex(maxX, maxZ, x)];
    vertices[indexOfCenterVertex(minX, minZ, x)] = accumulatedValues / 4;
  }

  public void setFlatten(bool active) {
    if (!active) {
      return;
    }
    updateType = TileUpdateType.Flatten;
  }

  public void setChangeHeight(bool active) {
    if (!active) {
      return;
    }
    updateType = TileUpdateType.LowerRaise;
  }
}