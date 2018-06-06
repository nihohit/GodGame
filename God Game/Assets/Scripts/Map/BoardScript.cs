using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Base;
using System;

public class BoardScript: MonoBehaviour {
  public int heightChangeRate = 20;
  public int x, z;
  public bool flatten, changeHeight;
	public GameObject birdControlObject;

	private BirdControlScript birdControl;
  private TileScript[,] tileScripts;
  private GameObject[,] tiles;
  private InteractionMode interactionMode;
  private TerrainObjectScript currentTree;
  private GameObject[] treePrefabs;
  private bool ignoreContentAddition;
  private TerrainObjectScript lastTouchedObject;
  private int treeCount = 0;

  #region initialization

  // Use this for initialization
  void Start() {
    birdControl = birdControlObject.GetComponent<BirdControlScript>();
    initializeTiles();
  }

  private void initializeTiles() {
    GameObject prefab = (GameObject)Resources.Load("Prefabs/Tile");
    treePrefabs = Resources.LoadAll<GameObject>("Prefabs/Trees/RegularTrees");

    tiles = new GameObject[x * 2, z * 2];
    tileScripts = new TileScript[x * 2, z * 2];
    for (int i = -x; i < x; i++) {
      for (int j = -z; j < z; j++) {
        var tile = this.InstantiateObject(prefab, Vector3.Scale(prefab.GetComponent<Renderer>().bounds.size, new Vector3(i, 0, j)));
        tiles[i + x, j + z] = tile;
        tileScripts[i + x, j + z] = tile.GetComponent<TileScript>();
        tileScripts[i + x, j + z].board = this;
        tile.name = string.Format("Tile {0}, {1}", i + x, j + z);
        tile.transform.parent = transform;

        var tree = instantiateTree(Vector3.zero);
        tree.transform.parent = tile.transform;
        tree.transform.localPosition = new Vector3((float)Randomizer.NextDouble(-5, 5), 0, (float)Randomizer.NextDouble(-5, 5));
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

  #endregion

  // Update is called once per frame
  void Update() {
    if (Input.GetKey(KeyCode.Escape)) {
      Application.Quit();
      return;
    }

    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
      return;
    }

    switch(interactionMode) {
      case InteractionMode.AddBird:
        handleBirdInteraction();
        break;
      case InteractionMode.AddTree:
        handleTreeInteraction();
        break;
      case InteractionMode.LowerRaiseTile:
      case InteractionMode.FlattenTile:
        handleTileInteraction();
        break;
    }
  }

  #region tree interaction

  private void handleTreeInteraction() {
    var hit = this.CurrentMousePointedTile();
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
    if (currentTree == null || !currentTree.CanBePlanted() || ignoreContentAddition) {
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
    StartCoroutine(BlockContentAddition());
  }

  private IEnumerator BlockContentAddition() {
    ignoreContentAddition = true;
    yield return new WaitForSeconds(0.05f);
    ignoreContentAddition = false;
  }

  private void moveCurrentTree(RaycastHit hit) {
    if (currentTree == null) {
      return;
    }
    currentTree.transform.position = hit.point;
    currentTree.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
  }

  #endregion

  #region tile interaction

  private void handleTileInteraction() {
    TileUpdateDirection direction;
    if (Input.GetMouseButton(0)) {
      direction = TileUpdateDirection.Up;
    } else if (Input.GetMouseButton(1)) {
      direction = TileUpdateDirection.Down;
    } else {
      return;
    }

    var hit = this.CurrentMousePointedTile();
    if (!hit.HasValue) {
      return;
    }

    TileScript tileToUpdate = hit.Value.collider.GetComponent<TileScript>();
    updateTile(tileToUpdate, direction);
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

  #endregion

  #region bird interaction

  private void handleBirdInteraction() {
    if (lastTouchedObject != null) {
      lastTouchedObject.SetOriginalColors();
    }

    var newObjectHit = this.CurrentMousePointedTree();
    if (!newObjectHit.HasValue) {
      lastTouchedObject = null;
      return;
    }

    lastTouchedObject = newObjectHit.Value.collider.gameObject.GetComponent<TerrainObjectScript>();
    if (!lastTouchedObject.isPlanted()) {
      return;
    }
    PerchScript freePerch = null;
    foreach (Transform child in lastTouchedObject.transform) {
      if (child.GetComponent<PerchScript>() != null && child.childCount == 0) {
        freePerch = child.GetComponent<PerchScript>();
        break;
      }
    }
    if (freePerch == null) {
      return;
    }
    lastTouchedObject.SetRedColor();

    if (Input.GetMouseButton(0) && lastTouchedObject != null && !ignoreContentAddition) {
      birdControl.AddBird(freePerch);
      StartCoroutine(BlockContentAddition());
    }
  }

  #endregion

  #region interaction mode setters

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
    interactionMode = InteractionMode.AddTree;
    createNewTree();
  }

  public void setAddBird(bool active) {
    if (!active) {
      if (lastTouchedObject != null) {
        lastTouchedObject.SetOriginalColors();
      }
      return;
    }
    interactionMode = InteractionMode.AddBird;
  }

  #endregion

  private void createNewTree() {
    currentTree = instantiateTree(new Vector3(999, 0, 999));
    currentTree.TemporaryObject = true;
    foreach (var material in currentTree.GetComponent<Renderer>().materials) {
      var color = material.color;
      color.a = 0.3f;
      material.color = color;
    }
  }

  private TerrainObjectScript instantiateTree(Vector3 position) {
    var tree = this.InstantiateObject(treePrefabs[0], position).GetComponent<TerrainObjectScript>();
    tree.gameObject.name += " num: " + treeCount++;
    return tree;
  }
}