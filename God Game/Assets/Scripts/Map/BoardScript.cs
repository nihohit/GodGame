﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TileUpdateType {
    LowerRaise,
    Flatten
}

public class BoardScript : MonoBehaviour {
    private bool moveUp;

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
            BoardScript.adjustVertices(tileToUpdate, change, updateType, moveUp);
            tileToUpdate = null;
        }
    }

    public void tileWasPressed(TileScript tile, int mouseButtonCode) {
        tileToUpdate = tile;
        moveUp = mouseButtonCode == 0;
    }

    public static void adjustVertices(TileScript tile, float changeRate, TileUpdateType type, bool moveUp) {
        var newVertices = type == TileUpdateType.LowerRaise ? raisedVertices(tile, changeRate, moveUp) :
            flattenVertices(tile, changeRate, moveUp);
        foreach (var neighbour in tile.neighbours) {
            var offset = tile.transform.position - neighbour.transform.position;
            var offsetedVertices = newVertices.Select(vertex => vertex + offset).ToList();
            neighbour.vertices = TileScript.adjustedVertices(neighbour.vertices, offsetedVertices);
        }
    }

    private static List<Vector3> raisedVertices(TileScript tile, float changeRate, bool moveUp) {
        var change = moveUp ? changeRate : -changeRate;
        return TileScript.changeAllVerticesHeight(tile.vertices, change).ToList();
    }

    private static List<Vector3> flattenVertices(TileScript tile, float changeRate, bool moveUp) {
        return TileScript.flattenVertices(tile.vertices, changeRate, moveUp).ToList();
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