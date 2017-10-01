﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardScript : MonoBehaviour {
    private TileUpdateDirection direction;

    public int heightChangeRate = 20;
    public int x, z;
    public bool flatten, changeHeight;

    private TileScript[,] tileScripts;
    private GameObject[,] tiles;

    private TileScript tileToUpdate;
    private TileUpdateType updateType;

    // Use this for initialization
    void Start() {
        initializeTiles();
    }

    private void initializeTiles() {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Tile");
        tiles = new GameObject[x * 2, z * 2];
        tileScripts = new TileScript[x * 2, z * 2];
        for (int i = -x; i < x; i++) {
            for (int j = -z; j < z; j++) {
                tiles[i + x, j + z] = instantiateObject(prefab, Vector3.Scale(prefab.GetComponent<Renderer>().bounds.size, new Vector3(i, 0, j)));
                tileScripts[i + x, j + z] = tiles[i + x, j + z].GetComponent<TileScript>();
                tileScripts[i + x, j + z].board = this;
                tiles[i + x, j + z].name = string.Format("Tile {0}, {1}", i + x, j + z);
                tiles[i + x, j + z].transform.parent = transform;
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

    private GameObject instantiateObject(Object prefab, Vector3 position) {
        return (GameObject)Instantiate(prefab, position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update() {
        if (tileToUpdate != null) {
            float change = heightChangeRate * Time.deltaTime;
            BoardScript.adjustVertices(tileToUpdate, change, updateType, direction);
            tileToUpdate = null;
        }
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void tileWasPressed(TileScript tile, int mouseButtonCode) {
        tileToUpdate = tile;
        direction = mouseButtonCode == 0 ? TileUpdateDirection.Up : TileUpdateDirection.Down;
    }

    public static void adjustVertices(TileScript tile, float changeRate, TileUpdateType type, TileUpdateDirection direction) {
        var newVertices = type == TileUpdateType.LowerRaise ? raisedVertices(tile, changeRate, direction) :
            flattenVertices(tile, changeRate, direction);
        foreach (var neighbour in tile.neighbours) {
            var offset = tile.transform.position - neighbour.transform.position;
            var offsetedVertices = newVertices.Select(vertex => vertex + offset).ToList();
            neighbour.vertices = TileScript.adjustedVertices(neighbour.vertices, offsetedVertices);
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