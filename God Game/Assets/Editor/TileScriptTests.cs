// using UnityEngine;
// using UnityEditor;
// using UnityEngine.TestTools;
// using NUnit.Framework;
// using System.Collections;
// using System.Collections.Generic;

// public class TileScriptTests {

//     [Test]
//     public void changeAllVerticesHeight() {
//         var vertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 3, 0),
//             new Vector3(-5, 4, 5),
//             new Vector3(5, 5, 5)
//         };
//         var heightChange = 5;

//         var expectedVertices = new List<Vector3>{
//             new Vector3(-5, 6, -5),
//             new Vector3(5, 7, -5),
//             new Vector3(0, 8, 0),
//             new Vector3(-5, 9, 5),
//             new Vector3(5, 10, 5)
//         };

//         var result = TileScript.changeAllVerticesHeight(vertices, heightChange);

//         CollectionAssert.AreEqual(expectedVertices, result);

//         heightChange = -5;

//         expectedVertices = new List<Vector3>{
//             new Vector3(-5, -4, -5),
//             new Vector3(5, -3, -5),
//             new Vector3(0, -2, 0),
//             new Vector3(-5, -1, 5),
//             new Vector3(5, 0, 5)
//         };

//         result = TileScript.changeAllVerticesHeight(vertices, heightChange);

//         CollectionAssert.AreEqual(expectedVertices, result);
//     }

//     [Test]
//     public void flattenVerticesUp() {
//         var vertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 3, 0),
//             new Vector3(-5, 4, 5),
//             new Vector3(5, 5, 5)
//         };
//         var heightChange = 2f;

//         var expectedVertices = new List<Vector3>{
//             new Vector3(-5, 3, -5),
//             new Vector3(5, 3.5f, -5),
//             new Vector3(0, 4, 0),
//             new Vector3(-5, 4.5f, 5),
//             new Vector3(5, 5, 5)
//         };

//         var result = TileScript.flattenVertices(vertices, heightChange, TileUpdateDirection.Up);
//         CollectionAssert.AreEqual(expectedVertices, result);
//     }

//     [Test]
//     public void flattenVerticesDown() {
//         var vertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 3, 0),
//             new Vector3(-5, 4, 5),
//             new Vector3(5, 5, 5)
//         };
//         var heightChange = 2f;

//         var expectedVertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 1.5f, -5),
//             new Vector3(0, 2, 0),
//             new Vector3(-5, 2.5f, 5),
//             new Vector3(5, 3, 5)
//         };

//         var result = TileScript.flattenVertices(vertices, heightChange, TileUpdateDirection.Down);

//         CollectionAssert.AreEqual(expectedVertices, result);
//     }

//     [Test]
//     public void flattenVerticesLowPrecision() {
//         var vertices = new List<Vector3>{
//             new Vector3(-5, 2, -5),
//             new Vector3(5, 1.95f, -5),
//             new Vector3(0, 1.97f, 0),
//             new Vector3(-5, 1.98f, 5),
//             new Vector3(5, 1.99f, 5)
//         };
//         var heightChange = 0.1f;

//         var expectedVertices = new List<Vector3>{
//             new Vector3(-5, 2, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 2, 0),
//             new Vector3(-5, 2, 5),
//             new Vector3(5, 2, 5)
//         };

//         var result = TileScript.flattenVertices(vertices, heightChange, TileUpdateDirection.Up);

//         CollectionAssert.AreEqual(expectedVertices, result);
//     }

//     [Test]
//     public void flattenVerticesNoChange() {
//         var vertices = new List<Vector3>{
//             new Vector3(-5, 0, -5),
//             new Vector3(5, 0, -5),
//             new Vector3(0, 0, 0),
//             new Vector3(-5, 0, 5),
//             new Vector3(5, 0, 5)
//         };
//         var heightChange = 0.1f;

//         var result = TileScript.flattenVertices(vertices, heightChange, TileUpdateDirection.Up);

//         CollectionAssert.AreEqual(vertices, result);
//     }

//     [Test]
//     public void adjustVertices() {
//         var vertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 3, 0),
//             new Vector3(-5, 4, 5),
//             new Vector3(5, 5, 5)
//         };

//         var templateVertices = new List<Vector3>{
//             new Vector3(5, 6, -5),
//             new Vector3(15, 52, -5),
//             new Vector3(10, 53, 0),
//             new Vector3(5, 9, 5),
//             new Vector3(15, 55, 5)
//         };

//         var expectedVertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 6, -5),
//             new Vector3(0, 5, 0),
//             new Vector3(-5, 4, 5),
//             new Vector3(5, 9, 5)
//         };

//         var result = TileScript.adjustedVertices(vertices, templateVertices);
//         CollectionAssert.AreEqual(expectedVertices, result);

//         templateVertices = new List<Vector3>{
//             new Vector3(-5, 3, 5),
//             new Vector3(5, 4, 5),
//             new Vector3(0, 53, 10),
//             new Vector3(-5, 54, 15),
//             new Vector3(5, 55, 15)
//         };

//         expectedVertices = new List<Vector3>{
//             new Vector3(-5, 1, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 2.5f, 0),
//             new Vector3(-5, 3, 5),
//             new Vector3(5, 4, 5)
//         };

//         result = TileScript.adjustedVertices(vertices, templateVertices);
//         CollectionAssert.AreEqual(expectedVertices, result);

//         templateVertices = new List<Vector3>{
//             new Vector3(-15, 51, -15),
//             new Vector3(-5, 52, -15),
//             new Vector3(-10, 53, -10),
//             new Vector3(-15, 54, -5),
//             new Vector3(-5, 13, -5)
//         };

//         expectedVertices = new List<Vector3>{
//             new Vector3(-5, 13, -5),
//             new Vector3(5, 2, -5),
//             new Vector3(0, 6, 0),
//             new Vector3(-5, 4, 5),
//             new Vector3(5, 5, 5)
//         };

//         result = TileScript.adjustedVertices(vertices, templateVertices);
//         CollectionAssert.AreEqual(expectedVertices, result);
//     }
// }