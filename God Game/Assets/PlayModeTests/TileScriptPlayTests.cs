using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

public class TileScriptPlayTests {
    [UnityTest]
    public IEnumerator verticesProperty() {
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshCollider>();
        var tileScript = obj.AddComponent<TileScript>();

        yield return null;

        var expectedVertices = new List<Vector3>{
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, -5),
            new Vector3(0, 0, 0),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        };

        var result = tileScript.vertices;
        CollectionAssert.AreEqual(expectedVertices, result);

        var newVertices = new List<Vector3>{
            new Vector3(-5, 1, -5),
            new Vector3(5, 2, -5),
            new Vector3(0, 3, 0),
            new Vector3(-5, 4, 5),
            new Vector3(5, 5, 5)
        };

        tileScript.vertices = newVertices;

        yield return null;

        tileScript.GetComponent<MeshFilter>().sharedMesh.GetVertices(result);
        CollectionAssert.AreEqual(newVertices, result);
    }
}
