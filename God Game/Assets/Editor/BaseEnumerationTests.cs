using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Base;

public class BaseEnumerationTests {

    private IEnumerator simpleEnumerator(List<int> list, int toAdd) {
        list.Add(toAdd);

        yield return null;
    }

    private IEnumerator actTwiceEnumerator(List<int> list, int toAdd) {
        list.Add(toAdd);

        yield return null;

        list.Add(toAdd + 1);

        yield return null;
    }

    private IEnumerator actAfterEnumerator(List<int> list, int toAdd) {
        list.Add(toAdd);

        yield return null;

        list.Add(toAdd + 1);
    }

    private IEnumerator assertEnumerator(List<int> list, int toAdd) {
        CollectionAssert.DoesNotContain(list, toAdd);

        yield return null;

        CollectionAssert.Contains(list, toAdd);
    }

    [Test]
	public void joinBasicEnumeratorsCorrectly() {
        var list = new List<int>();

        var enumerator = simpleEnumerator(list, 1).Join(simpleEnumerator(list, 2));

        while(enumerator.MoveNext()) {
        }

        CollectionAssert.AreEqual(new List<int> { 1, 2 }, list);
    }

    [Test]
    public void joinLongEnumeratorsCorrectly() {
        var list = new List<int>();

        var enumerator = actTwiceEnumerator(list, 1).Join(simpleEnumerator(list, 3));

        while (enumerator.MoveNext()) {
        }

        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, list);
    }

    [Test]
    public void joinComplexEnumeratorsCorrectly() {
        var list = new List<int>();

        var enumerator = simpleEnumerator(list, 1).Join(actAfterEnumerator(list, 2));

        while (enumerator.MoveNext()) {
        }

        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, list);
    }

    [Test]
    public void joinMultipleEnumeratorsCorrectly() {
        var list = new List<int>();

        var enumerator = simpleEnumerator(list, 3).Join(actAfterEnumerator(list, 4));
        enumerator = actTwiceEnumerator(list, 1).Join(enumerator);

        while (enumerator.MoveNext()) {
        }

        CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4, 5 }, list);
    }

    [Test]
    public void updateSideEffectsCorrectly() {
        var list = new List<int>();

        var enumerator = actAfterEnumerator(list, 1).Join(actAfterEnumerator(list, 3));

        CollectionAssert.AreEqual(new List<int>(), list);

        UnityEngine.Assertions.Assert.IsTrue(enumerator.MoveNext());

        CollectionAssert.AreEqual(new List<int> { 1 }, list);

        UnityEngine.Assertions.Assert.IsTrue(enumerator.MoveNext());

        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, list);

        UnityEngine.Assertions.Assert.IsFalse(enumerator.MoveNext());

        CollectionAssert.AreEqual(new List<int> { 1, 2, 3, 4 }, list);
    }

    [Test]
    public void assertByOrderCorrectly() {
        var list = new List<int> { 1 };

        var enumerator = assertEnumerator(list, 1).Join(assertEnumerator(list, 2));

        list.Clear();
        UnityEngine.Assertions.Assert.IsTrue(enumerator.MoveNext());
        list.Add(1);
        
        UnityEngine.Assertions.Assert.IsTrue(enumerator.MoveNext());
        list.Add(2);

        UnityEngine.Assertions.Assert.IsFalse(enumerator.MoveNext());
    }

    [Test]
    public void assertEnumeration() {
        var list = new List<int>();

        var enumerator = simpleEnumerator(list, 1).Join(assertEnumerator(list, 2));

        CollectionAssert.AreEqual(new List<int>(), list);

        UnityEngine.Assertions.Assert.IsTrue(enumerator.MoveNext());

        CollectionAssert.AreEqual(new List<int> { 1 }, list);

        UnityEngine.Assertions.Assert.IsTrue(enumerator.MoveNext());
        list.Add(2);

        UnityEngine.Assertions.Assert.IsFalse(enumerator.MoveNext());
    }
}
