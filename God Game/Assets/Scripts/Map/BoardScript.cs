using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardScript : MonoBehaviour {

    public int x, z;

    private TileScript[,] tileScripts;

    // Use this for initialization
    void Start () {
        GameObject[,] tiles = initializeTiles(this.x, this.z);
	}

    GameObject[,] initializeTiles(int x, int z) {
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Tile");
        GameObject[,] tiles = new GameObject[x * 2, z * 2];
        for (int i = - x; i < x; i++) {
            for(int j = - z; j < z; j++) {
                tiles[i + x, j + z] = instantiateObject(prefab, Vector3.Scale(prefab.GetComponent<Renderer>().bounds.size, new Vector3(i, 0, j)));
                tiles[i + x, j + z].name = string.Format("Tile {0}, {1}", i + x, j + z);
            }
        }
        return tiles;
    }

    GameObject instantiateObject(Object prefab, Vector3 position) {
        return (GameObject)Instantiate(prefab, position, Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
