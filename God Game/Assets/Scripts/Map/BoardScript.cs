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
    adjustCornersUniformly(minX, minZ, vertices, direction);
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

  public void updateTile(TileScript tile, TileUpdateDirection direction) {
    float change = heightChangeRate * Time.deltaTime;
    BoardScript.adjustVertices(tile, change, updateType, direction);
  }

  public static void adjustVertices(TileScript tile, float changeRate, TileUpdateType type, TileUpdateDirection direction) {
    var newVertices = type == TileUpdateType.LowerRaise ? raisedVertices(tile, changeRate, direction) :
        flattenVertices(tile, changeRate, direction);
    foreach (var neighbour in tile.neighbours) {
      var offset = tile.transform.position - neighbour.transform.position;
      var offsetedVertices = newVertices.Select(vertex => vertex + offset).ToList();
      neighbour.vertices = TileScript.adjustedVertices(neighbour.vertices, offsetedVertices);
      TileScript.adjustChildrenLocation(neighbour);
    }
  }

  private static List<Vector3> raisedVertices(TileScript tile, float changeRate, TileUpdateDirection direction) {
    var change = direction == TileUpdateDirection.Up ? changeRate : -changeRate;
    return TileScript.changeAllVerticesHeight(tile.vertices, change).ToList();
  }

  private static List<Vector3> flattenVertices(TileScript tile, float changeRate, TileUpdateDirection direction) {
    return TileScript.flattenVertices(tile.vertices, changeRate, direction).ToList();
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