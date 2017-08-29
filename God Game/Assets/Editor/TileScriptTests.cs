// using UnityEngine;
// using UnityEditor;
// using UnityEngine.TestTools;
// using NUnit.Framework;
// using System.Collections.Generic;

// public class TileScriptTests {

// 	[Test]
// 	public void adjustCornersTest() {
// 		var gameObject = new GameObject();
// 		var tile = gameObject.AddComponent<TileScript>();

// 		var corners = new List<Vector3>{
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		var heightChange = 5;

// 		var offset = new Vector3(10, 0, 0);
// 		var newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		var expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 7, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 9, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);
		
// 		offset = new Vector3(0, 0, 10);
// 		newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 8, 5), 
// 			new Vector3(5, 9, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);

// 		offset = new Vector3(-10, 0, 0);
// 		newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 6, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 8, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);
		
// 		offset = new Vector3(0, 0, -10);
// 		newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 6, -5), 
// 			new Vector3(5, 7, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);
// 	}

// 	[Test]
// 	public void adjustIndirectCornersTest() {
// 		var gameObject = new GameObject();
// 		var tile = gameObject.AddComponent<TileScript>();

// 		var corners = new List<Vector3>{
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		var heightChange = 5;

// 		var offset = new Vector3(10, 0, 10);
// 		var newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		var expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 9, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);
		
// 		offset = new Vector3(-10, 0, 10);
// 		newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 8, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);

// 		offset = new Vector3(-10, 0, -10);
// 		newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 6, -5), 
// 			new Vector3(5, 2, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);
		
// 		offset = new Vector3(10, 0, -10);
// 		newCorners = tile.adjustCorners(heightChange, offset, corners);
// 		expectedCorners = new List<Vector3> {
// 			new Vector3(-5, 1, -5), 
// 			new Vector3(5, 7, -5), 
// 			new Vector3(-5, 3, 5), 
// 			new Vector3(5, 4, 5)
// 		};
// 		CollectionAssert.AreEqual(newCorners, expectedCorners);
// 	}

// 	[Test]
// 	public void adjustVertices() {
// 		var gameObject = new GameObject();
// 		var tile = gameObject.AddComponent<TileScript>();

// 		var vertices = new List<Vector3>{
// 			new Vector3(-1, 0, -1),
// 			new Vector3(0, 0, -1), 
// 			new Vector3(1, 0, -1), 
// 			new Vector3(-1, 0, 0),
// 			new Vector3(0, 0, 0), 
// 			new Vector3(1, 0, 0), 
// 			new Vector3(-1, 0, 1),
// 			new Vector3(0, 0, 1), 
// 			new Vector3(1, 0, 1), 
// 		}; 

// 		var corners = new List<Vector3>{
// 			new Vector3(-1, 2, -1), 
// 			new Vector3(1, 2, -1), 
// 			new Vector3(-1, 0, 1), 
// 			new Vector3(1, 0, 1)
// 		};
// 		var expectedVertices = new List<Vector3>{
// 			new Vector3(-1, 2, -1),
// 			new Vector3(0, 2, -1), 
// 			new Vector3(1, 2, -1), 
// 			new Vector3(-1, 1, 0),
// 			new Vector3(0, 1, 0), 
// 			new Vector3(1, 1, 0), 
// 			new Vector3(-1, 0, 1),
// 			new Vector3(0, 0, 1), 
// 			new Vector3(1, 0, 1), 
// 		};

// 		var result = tile.adjustVertices(vertices, corners);

// 		CollectionAssert.AreEqual(result, expectedVertices); 
// 	}

// 		[Test]
// 	public void adjustVerticesSingleCorner() {
// 		var gameObject = new GameObject();
// 		var tile = gameObject.AddComponent<TileScript>();

// 		var vertices = new List<Vector3>{
// 			new Vector3(-1, 0, -1),
// 			new Vector3(0, 0, -1), 
// 			new Vector3(1, 0, -1), 
// 			new Vector3(-1, 0, 0),
// 			new Vector3(0, 0, 0), 
// 			new Vector3(1, 0, 0), 
// 			new Vector3(-1, 0, 1),
// 			new Vector3(0, 0, 1), 
// 			new Vector3(1, 0, 1), 
// 		}; 

// 		var corners = new List<Vector3>{
// 			new Vector3(-1, 4, -1), 
// 			new Vector3(1, 0, -1), 
// 			new Vector3(-1, 0, 1), 
// 			new Vector3(1, 0, 1)
// 		};
// 		var expectedVertices = new List<Vector3>{
// 			new Vector3(-1, 4, -1),
// 			new Vector3(0, 2, -1), 
// 			new Vector3(1, 2, -1), 
// 			new Vector3(-1, 1, 0),
// 			new Vector3(0, 1, 0), 
// 			new Vector3(1, 0, 0), 
// 			new Vector3(-1, 0, 1),
// 			new Vector3(0, 0, 1), 
// 			new Vector3(1, 0, 1), 
// 		};

// 		var result = tile.adjustVertices(vertices, corners);

// 		CollectionAssert.AreEqual(result, expectedVertices); 
// 	}
// }
