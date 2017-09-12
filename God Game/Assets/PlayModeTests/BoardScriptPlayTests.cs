using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class BoardScriptPlayTests {
    private TileScript newTile(Vector3 position) {
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshCollider>();
        obj.transform.position = position;
        return obj.AddComponent<TileScript>();
    }

    [UnityTest]
    public IEnumerator adjustVertices() {
        var tile1 = newTile(Vector3.zero);
        var tile2 = newTile(new Vector3(10, 0, 0));
        var tile3 = newTile(new Vector3(-10, 0, -10));

        yield return null;

        tile1.directNeighbours = new[] { tile2 };
        tile1.indirectNeighbours = new[] { tile3 };

        BoardScript.adjustVertices(tile1, 10, TileUpdateType.LowerRaise, true);

        var expectedVertices = new List<Vector3>{
            new Vector3(-5, 10, -5),
            new Vector3(5, 10, -5),
            new Vector3(0, 10, 0),
            new Vector3(-5, 10, 5),
            new Vector3(5, 10, 5)
        };

        CollectionAssert.AreEqual(expectedVertices, tile1.vertices);

        expectedVertices = new List<Vector3>{
            new Vector3(-5, 10, -5),
            new Vector3(5, 0, -5),
            new Vector3(0, 5, 0),
            new Vector3(-5, 10, 5),
            new Vector3(5, 0, 5)
        };

        CollectionAssert.AreEqual(expectedVertices, tile2.vertices);

        expectedVertices = new List<Vector3>{
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, -5),
            new Vector3(0, 2.5f, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 10, 5)
        };

        CollectionAssert.AreEqual(expectedVertices, tile3.vertices);
    }

    [UnityTest]
    public IEnumerator Board_InitializesChildren() {
        var gameObject = new GameObject();
        var boardScript = gameObject.AddComponent<BoardScript>();
        boardScript.x = 1;
        boardScript.z = 1;

        Assert.AreEqual(0, gameObject.transform.childCount);

        yield return null;

        Assert.AreEqual(4, gameObject.transform.childCount);
        Assert.AreEqual("Tile 0, 0", gameObject.transform.GetChild(0).name);
        Assert.AreEqual(new Vector3(-10, 0, -10), gameObject.transform.GetChild(0).transform.position);

        Assert.AreEqual("Tile 0, 1", gameObject.transform.GetChild(1).name);
        Assert.AreEqual(new Vector3(-10, 0, 0), gameObject.transform.GetChild(1).transform.position);

        Assert.AreEqual("Tile 1, 0", gameObject.transform.GetChild(2).name);
        Assert.AreEqual(new Vector3(0, 0, -10), gameObject.transform.GetChild(2).transform.position);

        Assert.AreEqual("Tile 1, 1", gameObject.transform.GetChild(3).name);
        Assert.AreEqual(new Vector3(0, 0, 0), gameObject.transform.GetChild(3).transform.position);

        GameObject.Destroy(gameObject);
    }

    [UnityTest]
    public IEnumerator Board_SetNeighbours() {
        var gameObject = new GameObject();
        var boardScript = gameObject.AddComponent<BoardScript>();
        boardScript.x = 2;
        boardScript.z = 2;

        yield return null;

        Assert.AreEqual(16, gameObject.transform.childCount);
        var tile = GameObject.Find("Tile 1, 1").GetComponent<TileScript>();
        Assert.NotNull(tile);
        Assert.AreEqual(4, tile.directNeighbours.Count());
        //Debug.Log(string.Join(", ", tile.indirectNeighbours.Select(script => script.transform.name)));
        Assert.AreEqual(4, tile.indirectNeighbours.Count());
        CollectionAssert.AreEqual(new List<string> { "Tile 0, 1", "Tile 2, 1", "Tile 1, 0", "Tile 1, 2" }, 
            tile.directNeighbours.Select(script => script.transform.name).ToList());
        CollectionAssert.AreEqual(new List<string> { "Tile 0, 0", "Tile 0, 2", "Tile 2, 0", "Tile 2, 2" },
             tile.indirectNeighbours.Select(script => script.transform.name).ToList());

        GameObject.Destroy(gameObject);
    }
}
