﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoardScript : MonoBehaviour {
    private enum TileUpdateType {
        Raise,
        Lower,
        Flatten
    }

    public int heightChangeRate = 20;

    public int x, z;

    private TileScript[,] tileScripts;
    private GameObject[,] tiles;

    private TileScript tileToUpdate;
    private TileUpdateType updateType;

    // Use this for initialization
    void Start() {
        initializeTiles();
    }

    void initializeTiles() {
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

    void setupNeighbours() {
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

    GameObject instantiateObject(Object prefab, Vector3 position) {
        return (GameObject)Instantiate(prefab, position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update() {
        if (tileToUpdate != null) {
            float change = heightChangeRate * Time.deltaTime;
            change = updateType == TileUpdateType.Lower ? -change : change;
            BoardScript.adjustVertices(tileToUpdate, change);
            tileToUpdate = null;
        }
    }

    public void tileWasPressed(TileScript tile, int mouseButtonCode) {
        tileToUpdate = tile;
        updateType = mouseButtonCode == 1 ? TileUpdateType.Lower : TileUpdateType.Raise;
    }

    public static void adjustVertices(TileScript tile, float changedHeight) {
        var vertices = TileScript.changeVerticesHeight(tile.vertices, changedHeight).ToList();
        foreach(var neighbour in tile.neighbours) {
            var offset = tile.transform.position - neighbour.transform.position;
            var offsetedVertices = vertices.Select(vertex => vertex + offset).ToList();
            neighbour.vertices = TileScript.adjustedVertices(neighbour.vertices, offsetedVertices);
        }
    }
}