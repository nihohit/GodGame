﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardScript : MonoBehaviour {
    public int heightChangeRate = 20;
    public int x, z;
    public bool flatten, changeHeight;

    private TileScript[,] tileScripts;
    private GameObject[,] tiles;
    private TileUpdateType updateType;

    // Use this for initialization
    void Start() {
        initializeTiles();
        AstarPath.active.Scan(AstarPath.active.graphs[0]);
    }

    private void initializeTiles() {
        int manCount = 0;
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Tile");
        GameObject treePrefab = (GameObject)Resources.Load("Prefabs/Trees/RegularTrees/tree001");
        GameObject manPrefab = (GameObject)Resources.Load("Prefabs/man");
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

                var tree = instantiateObject(treePrefab, Vector3.zero);
                tree.transform.parent = tile.transform;
                tree.transform.localPosition = randomLocationOnTile();
                tree.AddComponent<TerrainObjectScript>();
                tree.tag = "TerrainObject";

                if (Mathf.Abs((i + j) % 4) == 1) {
                    manCount++;
                    var man = instantiateObject(manPrefab, Vector3.zero);
                    man.transform.parent = tile.transform;
                    man.transform.localPosition = randomLocationOnTile();
                }
            }
        }
        Debug.Log(string.Format("man count: {0}", manCount));

        setupNeighbours();
    }

    private static Vector3 randomLocationOnTile() {
        var low = -Constants.TileLength / 2;
        var high = Constants.TileLength / 2;
        var x = (float)Assets.Scripts.Base.Randomizer.NextDouble(low, high);
        var z = (float)Assets.Scripts.Base.Randomizer.NextDouble(low, high);
        return new Vector3(x, 0, z);
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
        BoardScript.adjustVertices(tile, change, updateType, direction, AstarPath.active);
    }

    public static void adjustVertices(TileScript tile, float changeRate, TileUpdateType type, TileUpdateDirection direction, AstarPath path) {
        var newVertices = type == TileUpdateType.LowerRaise ? raisedVertices(tile, changeRate, direction) :
            flattenVertices(tile, changeRate, direction);
        foreach (var neighbour in tile.neighbours) {
            var offset = tile.transform.position - neighbour.transform.position;
            var offsetedVertices = newVertices.Select(vertex => vertex + offset).ToList();
            neighbour.vertices = TileScript.adjustedVertices(neighbour.vertices, offsetedVertices);
            TileScript.adjustChildrenLocation(neighbour);
            path.UpdateGraphs(new Bounds(tile.transform.position, 
                new Vector3(Constants.TileLength, 0, Constants.TileLength)));
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

    public GameObject TileInPosition(Vector3 position) {
        return tiles[Mathf.RoundToInt(((int)position.x) / Constants.TileLength) + x, 
            Mathf.RoundToInt(((int)position.z) / Constants.TileLength) + z];
    }
}