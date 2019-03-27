using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using Unity.Collections;
using Unity.Jobs;

public class BoardScriptPlayTests : IPrebuildSetup {

    public void Setup() {
        cleanObjects();
    }
    private const string kTag = "testingObject";

    private TileScript newTile(Vector3 position) {
        GameObject obj = new GameObject();
        obj.tag = kTag;
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshCollider>();
        obj.transform.position = position;
        return obj.AddComponent<TileScript>();
    }

    private void cleanObjects() {
        var gameObjects = GameObject.FindGameObjectsWithTag(kTag);

        foreach(var gameObject in gameObjects) {
            GameObject.Destroy(gameObject);
        }
    }

    private BoardScript newBoard(int x, int z) {
        var gameObject = new GameObject();
        gameObject.tag = kTag;
        var boardScript = gameObject.AddComponent<BoardScript>();
        boardScript.x = x;
        boardScript.z = z;
        return boardScript;
    }

    [UnityTest]
    public IEnumerator adjustVertices() {
        var tile1 = newTile(Vector3.zero);
        var tile2 = newTile(new Vector3(10, 0, 0));
        var tile3 = newTile(new Vector3(-10, 0, -10));

        yield return null;

        tile1.directNeighbours = new[] { tile2 };
        tile1.indirectNeighbours = new[] { tile3 };

		var handles = new NativeArray<JobHandle>(9, Allocator.Temp);

				BoardScript.adjustVertices(tile1, 10, InteractionMode.LowerRaiseTile, TileUpdateDirection.Up, handles);
		handles.Dispose();

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

        cleanObjects();
    }

    [UnityTest]
    public IEnumerator Board_InitializesChildren() {
        var gameObject = newBoard(1, 1).gameObject;

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

        cleanObjects();
    }

    [UnityTest]
    public IEnumerator Board_SetNeighbours() {
        cleanObjects();

        var gameObject = newBoard(2, 2).gameObject;

        yield return null;

        Assert.AreEqual(16, gameObject.transform.childCount);
        var tile = GameObject.Find("Tile 1, 1").GetComponent<TileScript>();
        Assert.NotNull(tile);
        Assert.AreEqual(4, tile.directNeighbours.Count());
        Assert.AreEqual(4, tile.indirectNeighbours.Count());
        CollectionAssert.AreEqual(new List<string> { "Tile 0, 1", "Tile 2, 1", "Tile 1, 0", "Tile 1, 2" }, 
            tile.directNeighbours.Select(script => script.transform.name).ToList());
        CollectionAssert.AreEqual(new List<string> { "Tile 0, 0", "Tile 0, 2", "Tile 2, 0", "Tile 2, 2" },
             tile.indirectNeighbours.Select(script => script.transform.name).ToList());

        cleanObjects();
    }

    [UnityTest]
    public IEnumerator raiseTileFullFlow() {
        cleanObjects();

        var boardScript = newBoard(1, 1);
        return testTileHeightChange(boardScript, 0);
    }

    [UnityTest]
    public IEnumerator lowerTileFullFlow() {
        cleanObjects();

        var boardScript = newBoard(1, 1);
        return testTileHeightChange(boardScript, TileUpdateDirection.Down);
    }

    [UnityTest]
    public IEnumerator flattenUpTileFullFlow() {
        cleanObjects();

        var boardScript = newBoard(1, 1);
        return testTileFlattening(boardScript, 0);
    }

    [UnityTest]
    public IEnumerator flattenDownTileFullFlow() {
        var boardScript = newBoard(1, 1);
        return testTileFlattening(boardScript, TileUpdateDirection.Down);
    }

    private IEnumerator testTileHeightChange(BoardScript boardScript, TileUpdateDirection direction, bool clean = true) {
        yield return null;

        var tile1 = GameObject.Find("Tile 0, 0").GetComponent<TileScript>();
        var tile2 = GameObject.Find("Tile 1, 0").GetComponent<TileScript>();
        var tile3 = GameObject.Find("Tile 0, 1").GetComponent<TileScript>();
        var tile4 = GameObject.Find("Tile 1, 1").GetComponent<TileScript>();

        boardScript.setChangeHeight(true);
        boardScript.updateTile(tile1, direction);
        boardScript.heightChangeRate = 20;

        yield return null;

        var expectedChange = tile1.vertices[0].y;
        Assert.NotZero(expectedChange);

        var expectedVertices1 = new List<Vector3>{
            new Vector3(-5, expectedChange, -5),
            new Vector3(5, expectedChange, -5),
            new Vector3(0, expectedChange, 0),
            new Vector3(-5, expectedChange, 5),
            new Vector3(5, expectedChange, 5)
        };

        var expectedVertices2 = new List<Vector3>{
            new Vector3(-5, expectedChange, -5),
            new Vector3(5, 0, -5),
            new Vector3(0, expectedChange / 2, 0),
            new Vector3(-5, expectedChange, 5),
            new Vector3(5, 0, 5)
        };

        var expectedVertices3 = new List<Vector3>{
            new Vector3(-5, expectedChange, -5),
            new Vector3(5, expectedChange, -5),
            new Vector3(0, expectedChange / 2, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        };

        var expectedVertices4 = new List<Vector3>{
            new Vector3(-5, expectedChange, -5),
            new Vector3(5, 0, -5),
            new Vector3(0, expectedChange / 4, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        };

        CollectionAssert.AreEqual(expectedVertices1, tile1.vertices);
        CollectionAssert.AreEqual(expectedVertices2, tile2.vertices);
        CollectionAssert.AreEqual(expectedVertices3, tile3.vertices);
        CollectionAssert.AreEqual(expectedVertices4, tile4.vertices);

        yield return null;

        CollectionAssert.AreEqual(expectedVertices1, tile1.vertices);
        CollectionAssert.AreEqual(expectedVertices2, tile2.vertices);
        CollectionAssert.AreEqual(expectedVertices3, tile3.vertices);
        CollectionAssert.AreEqual(expectedVertices4, tile4.vertices);

        if (clean) {
            cleanObjects();
        }
    }

    private IEnumerator testTileFlattening(BoardScript boardScript, TileUpdateDirection direction) {
        IEnumerator raiseTile = testTileHeightChange(boardScript, 0, false);
        return Assets.Scripts.Base.MyExtensions.Join(raiseTile, testTileFlatteningInternal(boardScript, direction));
    }

    private IEnumerator testTileFlatteningInternal(BoardScript boardScript, TileUpdateDirection direction) {
        var tile1 = GameObject.Find("Tile 0, 0").GetComponent<TileScript>();
        var tile2 = GameObject.Find("Tile 1, 0").GetComponent<TileScript>();
        var tile3 = GameObject.Find("Tile 0, 1").GetComponent<TileScript>();
        var tile4 = GameObject.Find("Tile 1, 1").GetComponent<TileScript>();

        boardScript.setFlatten(true);
        boardScript.updateTile(tile2, direction);

        var vertices = tile2.vertices;
        while (vertices[0].y != vertices[1].y ||
            vertices[1].y != vertices[2].y ||
            vertices[2].y != vertices[3].y ||
            vertices[3].y != vertices[4].y) {
            boardScript.updateTile(tile2, direction);
            yield return null;
            vertices = tile2.vertices;
        }

        var expectedHeight = tile1.vertices[0].y;
        var expectedFlattenedHeight = direction == TileUpdateDirection.Up ? expectedHeight : 0;
        Assert.NotZero(expectedHeight);

        var expectedVertices1 = new List<Vector3>{
            new Vector3(-5, expectedHeight, -5),
            new Vector3(5, expectedFlattenedHeight, -5),
            new Vector3(0, (expectedHeight + expectedFlattenedHeight) / 2, 0),
            new Vector3(-5, expectedHeight, 5),
            new Vector3(5, expectedFlattenedHeight, 5)
        };

        var expectedVertices2 = new List<Vector3>{
            new Vector3(-5, expectedFlattenedHeight, -5),
            new Vector3(5, expectedFlattenedHeight, -5),
            new Vector3(0, expectedFlattenedHeight, 0),
            new Vector3(-5, expectedFlattenedHeight, 5),
            new Vector3(5, expectedFlattenedHeight, 5)
        };

        var expectedVertices3 = new List<Vector3>{
            new Vector3(-5, expectedHeight, -5),
            new Vector3(5, expectedFlattenedHeight, -5),
            new Vector3(0, (expectedHeight + expectedFlattenedHeight) / 4, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        };

        var expectedVertices4 = new List<Vector3>{
            new Vector3(-5, expectedFlattenedHeight, -5),
            new Vector3(5, expectedFlattenedHeight, -5),
            new Vector3(0, expectedFlattenedHeight / 2, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        };

        CollectionAssert.AreEqual(expectedVertices1, tile1.vertices);
        CollectionAssert.AreEqual(expectedVertices2, tile2.vertices);
        CollectionAssert.AreEqual(expectedVertices3, tile3.vertices);
        CollectionAssert.AreEqual(expectedVertices4, tile4.vertices);

        yield return null;

        CollectionAssert.AreEqual(expectedVertices1, tile1.vertices);
        CollectionAssert.AreEqual(expectedVertices2, tile2.vertices);
        CollectionAssert.AreEqual(expectedVertices3, tile3.vertices);
        CollectionAssert.AreEqual(expectedVertices4, tile4.vertices);

        cleanObjects();
    }
}
