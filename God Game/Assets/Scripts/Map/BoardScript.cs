﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour {

    public int x, z;

    private TileScript[,] tileScripts;
    private GameObject[,] tiles;

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

    }
}