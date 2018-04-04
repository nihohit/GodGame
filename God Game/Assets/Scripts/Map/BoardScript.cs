using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        var vertexIndex = i + (j * ((2 * x) + 1));
        vertices[vertexIndex] = new Vector3(i, 0, j) * Constants.SizeOfTile;
        uv[vertexIndex] = new Vector2(vertices[vertexIndex].x / x, vertices[vertexIndex].z / z) / Constants.SizeOfTile;
        if (i == x || j == z) {
          continue;
        }

        var centerIndex = vertexIndex + x + 1;
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

  private GameObject instantiateObject(Object prefab, Vector3 position) {
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

    TileScript tileToUpdate = hit.collider.GetComponent<TileScript>();
    updateTile(tileToUpdate, direction);
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