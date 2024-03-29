﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Base;
using System;

public class BoardScript: MonoBehaviour {
  public int heightChangeRate = 20;
  public int x, z;
  public bool flatten, changeHeight;

  private TileScript[,] tileScripts;
  private GameObject[,] tiles;
  private InteractionMode interactionMode;
  private TerrainObjectScript currentTree;
  private Vector3 currentTreeEulerRotation;
  private GameObject[] treePrefabs;
  private bool ignoreTreeAddition;

  // Use this for initialization
  void Start() {
    initializeTiles();
  }

  private void initializeTiles() {
    GameObject prefab = (GameObject)Resources.Load("Prefabs/Tile");
    treePrefabs = Resources.LoadAll<GameObject>("Prefabs/Trees/RegularTrees");

    tiles = new GameObject[x * 2, z * 2];
    tileScripts = new TileScript[x * 2, z * 2];
    for (int i = -x; i < x; i++) {
      for (int j = -z; j < z; j++) {
        var tile = instantiateObject(prefab, Vector3.Scale(prefab.GetComponent<Renderer>().bounds.size, new Vector3(i, 0, j)));
        tiles[i + x, j + z] = tile;
        tileScripts[i + x, j + z] = tile.GetComponent<TileScript>();
        tileScripts[i + x, j + z].board = this;
        tile.name = string.Format("Tile {0}, {1}", i + x, j + z);
        tile.transform.parent = transform;

        var tree = instantiateObject(Randomizer.ChooseValue(treePrefabs), Vector3.zero);
        tree.transform.parent = tile.transform;
        tree.transform.localPosition = new Vector3((float)Randomizer.NextDouble(-5, 5), 0, (float)Randomizer.NextDouble(-5, 5));
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

  private void handleTreeInteraction() {
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

  private void handleTileInteraction() {
    TileUpdateDirection direction;
    if (Input.GetMouseButton(0)) {
      direction = TileUpdateDirection.Up;
    } else if (Input.GetMouseButton(1)) {
      direction = TileUpdateDirection.Down;
    } else {
      return;
    }

    var hit = currentMousePointedLocation();
    if (!hit.HasValue) {
      return;
    }

    TileScript tileToUpdate = hit.Value.collider.GetComponent<TileScript>();
    updateTile(tileToUpdate, direction);
  }

  private System.Nullable<RaycastHit> currentMousePointedLocation() {
    RaycastHit hit = new RaycastHit();
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out hit, float.MaxValue, 1 << 8)) {
      return null;
    }
    return hit;
  }

  public void updateTile(TileScript tile, TileUpdateDirection direction) {
    float change = heightChangeRate * Time.deltaTime;
    BoardScript.adjustVertices(tile, change, interactionMode, direction);
  }

  public static void adjustVertices(TileScript tile, float changeRate, InteractionMode type, TileUpdateDirection direction) {
    var newVertices = type == InteractionMode.LowerRaiseTile ? raisedVertices(tile, changeRate, direction) :
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
    interactionMode = InteractionMode.FlattenTile;
  }

  public void setChangeHeight(bool active) {
    if (!active) {
      return;
    }
    interactionMode = InteractionMode.LowerRaiseTile;
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
  }
}