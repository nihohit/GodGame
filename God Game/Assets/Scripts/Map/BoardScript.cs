using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Base;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class BoardScript : MonoBehaviour {
  public float heightChangeRate;
  public int x, z;
  public bool flatten, changeHeight;
  public Slider slider;
  public GameObject projector;

  private TileScript[,] tileScripts;
  private GameObject[,] tiles;
  private InteractionMode interactionMode;
  private TerrainObjectScript currentTree;
  private Vector3 currentTreeEulerRotation;
  private GameObject[] treePrefabs;
  private bool ignoreTreeAddition;
  private NativeArray<JobHandle> adjustVerticesHandles;
  private NativeArray<float> cornerHeights;
  private NativeArray<float> centerHeights;
  private JobHandle[] childMovingJobs;

  public void OnDestroy() {
    adjustVerticesHandles.Dispose();
    cornerHeights.Dispose();
    centerHeights.Dispose();
  }

  // Use this for initialization
  void Start() {
    adjustVerticesHandles = new NativeArray<JobHandle>(9, Allocator.Persistent);
    var numberOfPotentialJobs = (int)math.pow(((slider.maxValue + 1) * 2) + 1, 2);
    childMovingJobs = new JobHandle[numberOfPotentialJobs];
    cornerHeights = new NativeArray<float>((x + 1) * (z + 1), Allocator.Persistent);
    for (int i = 0; i < cornerHeights.Length; i++) {
      cornerHeights[i] = 0;
    }
    centerHeights = new NativeArray<float>(x * z, Allocator.Persistent);
    for (int i = 0; i < centerHeights.Length; i++) {
      cornerHeights[i] = 0;
    }
    initializeTiles();
  }

  private void initializeTiles() {
    GameObject prefab = (GameObject)Resources.Load("Prefabs/Tile");
    treePrefabs = Resources.LoadAll<GameObject>("Prefabs/Trees/RegularTrees");

    var scale = new Vector3(Constants.SizeOfTile, 0, Constants.SizeOfTile);
    var halfSize = Constants.SizeOfTile / 2;
    tiles = new GameObject[x * 2, z * 2];
    tileScripts = new TileScript[x * 2, z * 2];
    for (int i = -x; i < x; i++) {
      for (int j = -z; j < z; j++) {
        var tile = instantiateObject(prefab, Vector3.Scale(scale, new Vector3(i, 0, j)));
        tiles[i + x, j + z] = tile;
        tileScripts[i + x, j + z] = tile.GetComponent<TileScript>();
        tile.name = string.Format("Tile {0}, {1}", i + x, j + z);
        tile.transform.parent = transform;

        var tree = instantiateObject(Randomizer.ChooseValue(treePrefabs), Vector3.zero);
        tree.transform.parent = tile.transform;
        tree.transform.localPosition = new Vector3((float)Randomizer.NextDouble(-halfSize, halfSize), 0, (float)Randomizer.NextDouble(-halfSize, halfSize));
        randomRotationAndScale(tree.transform);
      }
    }

    setupNeighbours();
  }

  private void setupNeighbours() {
    for (int i = 0; i < x * 2; i++) {
      for (int j = 0; j < z * 2; j++) {
        List<TileScript> directNeighbours = new List<TileScript>();
        List<TileScript> indirectNeighbours = new List<TileScript>();
        bool left = i > 0;
        bool right = i < x * 2 - 1;
        bool up = j > 0;
        bool down = j < z * 2 - 1;
        if (left) {
          directNeighbours.Add(tileScripts[i - 1, j]);
          if (up) {
            indirectNeighbours.Add(tileScripts[i - 1, j - 1]);
          }
          if (down) {
            indirectNeighbours.Add(tileScripts[i - 1, j + 1]);
          }
        }
        if (right) {
          directNeighbours.Add(tileScripts[i + 1, j]);
          if (up) {
            indirectNeighbours.Add(tileScripts[i + 1, j - 1]);
          }
          if (down) {
            indirectNeighbours.Add(tileScripts[i + 1, j + 1]);
          }
        }
        if (up) {
          directNeighbours.Add(tileScripts[i, j - 1]);
        }
        if (down) {
          directNeighbours.Add(tileScripts[i, j + 1]);
        }

        var tile = tileScripts[i, j];
        tile.directNeighbours = directNeighbours;
        tile.indirectNeighbours = indirectNeighbours;
      }
    }
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

    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
      return;
    }

    if (interactionMode == InteractionMode.AddTree) {
      handleTreeInteraction();
    } else {
      handleTileInteraction();
    }
  }

  #region tree interaction
  private void handleTreeInteraction() {
    projector.SetActive(false);
    var hit = currentMousePointedLocation();
    if (!hit.HasValue) {
      return;
    }

    moveCurrentTree(hit.Value);

    if (Input.GetMouseButton(0)) {
      addCurrentTree(hit.Value);
    } else if (Input.GetMouseButton(1)) {
      removeExistingTrees();
    }
  }

  private void removeExistingTrees() {
    currentTree.RemoveCollidingObjects();
  }

  private void addCurrentTree(RaycastHit hit) {
    if (currentTree == null || !currentTree.CanBePlanted() || ignoreTreeAddition) {
      return;
    }

    TileScript tile = hit.collider.GetComponent<TileScript>();
    currentTree.transform.parent = tile.transform;
    currentTree.TemporaryObject = false;
    foreach (var material in currentTree.GetComponent<Renderer>().materials) {
      var color = material.color;
      color.a = 1f;
      material.color = color;
    }

    createNewTree();
    StartCoroutine(BlockTreeAddition());
  }

  private IEnumerator BlockTreeAddition() {
    ignoreTreeAddition = true;
    yield return new WaitForSeconds(0.05f);
    ignoreTreeAddition = false;
  }

  private void moveCurrentTree(RaycastHit hit) {
    if (currentTree == null) {
      return;
    }
    currentTree.transform.position = hit.point;
    currentTree.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
    currentTree.transform.Rotate(currentTreeEulerRotation);
  }

  public void setAddTree(bool active) {
    if (!active) {
      Destroy(currentTree);
      return;
    }
    createNewTree();
  }

  private void createNewTree() {
    currentTree = instantiateObject(Randomizer.ChooseValue(treePrefabs), Vector3.zero).GetComponent<TerrainObjectScript>();
    currentTree.transform.position = new Vector3(999, 0, 999);
    currentTree.TemporaryObject = true;
    randomRotationAndScale(currentTree.transform);
    currentTreeEulerRotation = currentTree.transform.localEulerAngles;
    foreach (var material in currentTree.GetComponent<Renderer>().materials) {
      var color = material.color;
      color.a = 0.3f;
      material.color = color;
    }
    interactionMode = InteractionMode.AddTree;
  }

  private void randomRotationAndScale(Transform obj) {
    var rotation = Randomizer.Next(360);
    obj.localRotation = Quaternion.Euler(0f, rotation, 0f);
    var scale = (float)Randomizer.NextDouble(-0.2, 0.2);
    obj.localScale += new Vector3(scale, scale, scale);
    obj.localScale = obj.localScale * 0.2f;
  }

  #endregion

  #region tile interaction
  private void handleTileInteraction() {
    var hit = currentMousePointedLocation();
    if (!hit.HasValue) {
      return;
    }

    var hitPoint = hit.Value.point;
    float3 basoluteHitPoint = hitPoint / Constants.SizeOfTile;
    projector.transform.position = hitPoint + (Vector3.up * slider.value);
    projector.SetActive(true);
    var currentHeightChangeRate = heightChangeRate * Constants.SizeOfTile;

    TileUpdateDirection direction;
    if (Input.GetMouseButton(0)) {
      direction = TileUpdateDirection.Up;
    } else if (Input.GetMouseButton(1)) {
      direction = TileUpdateDirection.Down;
    } else {
      return;
    }

    if (InteractionMode.FlattenTile == interactionMode) {
      TileScript tileToUpdate = hit.Value.collider.GetComponent<TileScript>();
      updateTile(tileToUpdate, direction);
    } else if (InteractionMode.LowerRaiseTile == interactionMode) {
      var actionDirection = direction == TileUpdateDirection.Up ? 1 : -1;
      var computedValue = slider.value;
      var lookingRange = slider.value + 1;
      var deltaTime = Time.deltaTime;

      int length = 0;
      var adjustedX = hitPoint.x / Constants.SizeOfTile;
      var adjustedZ = hitPoint.z / Constants.SizeOfTile;

      var minX = Mathf.FloorToInt(Math.Max(adjustedX - lookingRange, -x));
      var maxX = Mathf.CeilToInt(Math.Min(adjustedX + lookingRange, x));
      var minZ = Mathf.FloorToInt(Math.Max(adjustedZ - lookingRange, -z));
      var maxZ = Mathf.CeilToInt(Math.Min(adjustedZ + lookingRange, z));
      var coords = new NativeArray<int2>((maxX - minX) * (maxZ - minZ), Allocator.TempJob);
      var lineWidth = (maxX - minX);
      for (int i = minX; i < maxX; i++) {
        for (int j = minZ; j < maxZ; j++) {
          coords[(i - minX) + ((j - minZ) * lineWidth)] = math.int2(i, j);
        }
      }

      var job = new ComputeVertices {
        indices = coords,
        heights = cornerHeights,
        HitPoint = basoluteHitPoint,
        computedValue = computedValue,
        deltaTime = deltaTime,
        actionDirection = actionDirection,
        heightChangeRate = currentHeightChangeRate,
        lineWidth = lineWidth
      };
      var handle = job.Schedule(coords.Length, 64);

      for (int i = minX; i < maxX; i++) {
        for (int j = minZ; j < maxZ; j++) {
          var xIndex = i + x;
          var zIndex = j + z;
          var tile = tileScripts[xIndex, zIndex];
          childMovingJobs[length] = TileScript.adjustChildrenLocation(tile, handle);
          length++;
        }
      };

      var newVertices = new List<Vector3>(Constants.NumberOfVerticesInTile);
      var populated = false;
      handle.Complete();

      for (int i = minX; i < maxX; i++) {
        for (int j = minZ; j < maxZ; j++) {
          var xIndex = i + x;
          var zIndex = j + z;
          var tile = tileScripts[xIndex, zIndex];
          var center =
          childMovingJobs[length] = TileScript.adjustChildrenLocation(tile, handle);

          job.vertices.ConvertInto(newVertices);
          length++;
        }
      };
      for (int i = 0; i < length; i++) {
        var tile = tileScripts[job.xCoord, job.yCoord];



        tile.vertices = newVertices;
      }

      var nativeHandles = new NativeArray<JobHandle>(length, Allocator.Temp);
      for (int i = 0; i < length; i++) {
        nativeHandles[i] = childMovingJobs[i];
      }
      JobHandle.CompleteAll(nativeHandles);
      nativeHandles.Dispose();
    }
  }

  [BurstCompile]
  private struct ComputeVertices : IJobParallelFor {
    [ReadOnly]
    public NativeArray<int2> indices;

    public NativeArray<float> heights;

    [ReadOnly]
    public float3 HitPoint;

    [ReadOnly]
    public float computedValue;

    [ReadOnly]
    public float deltaTime;

    [ReadOnly]
    public float actionDirection;

    [ReadOnly]
    public float heightChangeRate;

    [ReadOnly]
    public int lineWidth;

    public void Execute(int index) {
      var coords = indices[index];
      var heightsIndex = coords.x + (coords.y * lineWidth);
      var height = heights[heightsIndex];
      var vertex = math.float3(coords.x, height, coords.y);
      var distance = math.distance(vertex, HitPoint) / Constants.SizeOfTile;
      if (distance > computedValue) {
        return;
      }

      var intensity = math.cos(distance / computedValue) * heightChangeRate;
      heights[heightsIndex] = height + (actionDirection * intensity * deltaTime);
    }
  }

  private System.Nullable<RaycastHit> currentMousePointedLocation() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1 << 8)) {
      return null;
    }
    return hit;
  }

  public void updateTile(TileScript tile, TileUpdateDirection direction) {
    float change = heightChangeRate * Time.deltaTime * Constants.SizeOfTile;
    BoardScript.adjustVertices(tile, change, interactionMode, direction, adjustVerticesHandles);
  }

  public static void adjustVertices(TileScript tile, float changeRate, InteractionMode type, TileUpdateDirection direction, NativeArray<JobHandle> adjustVerticesHandles) {
    var newVertices = type == InteractionMode.LowerRaiseTile ? raisedVertices(tile, changeRate, direction) :
      flattenVertices(tile, changeRate, direction);

    int count = 0;
    foreach (var neighbour in tile.neighbours) {
      var offset = tile.transform.position - neighbour.transform.position;
      var offsetedVertices = newVertices.Select(vertex => vertex + offset).ToList();
      neighbour.vertices = TileScript.adjustedVertices(neighbour.vertices, offsetedVertices);

      adjustVerticesHandles[count] = TileScript.adjustChildrenLocation(neighbour, default(JobHandle));
      count++;
    }

    JobHandle.CompleteAll(adjustVerticesHandles);
  }

  private static List<Vector3> raisedVertices(TileScript tile, float changeRate, TileUpdateDirection direction) {
    var change = direction == TileUpdateDirection.Up ? changeRate : -changeRate;
    return TileScript.changeAllVerticesHeight(tile.vertices, change).ToList();
  }

  private static List<Vector3> flattenVertices(TileScript tile, float changeRate, TileUpdateDirection direction) {
    return TileScript.flattenVertices(tile.vertices, changeRate, direction).ToList();
  }

  #endregion

  public void setFlatten(bool active) {
    if (!active) {
      return;
    }
    interactionMode = InteractionMode.FlattenTile;
  }

  public void setChangeHeight(bool active) {
    if (!active) {
      return;
    }
    interactionMode = InteractionMode.LowerRaiseTile;
  }
}