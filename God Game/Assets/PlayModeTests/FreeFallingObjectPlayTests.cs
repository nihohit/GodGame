using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class FreeFallingObjectPlayTests {
	[UnityTest]
	public IEnumerator objectShouldBeDestroyedWhenTooHigh() {
        var gameObject = new GameObject();
        var fallingScripts = gameObject.AddComponent<FreeFallingObject>();

		yield return null;

        Assert.IsFalse(gameObject == null);

        fallingScripts.transform.position = new Vector3(0, Constants.MaxHeight + 1, 0);

        yield return null;
        yield return null;

        Assert.IsTrue(gameObject == null);
    }

    [UnityTest]
    public IEnumerator objectShouldBeDestroyedWhenTooLow() {
        var gameObject = new GameObject();
        var fallingScripts = gameObject.AddComponent<FreeFallingObject>();

        yield return null;

        Assert.IsFalse(gameObject == null);

        fallingScripts.transform.position = new Vector3(0, Constants.MaxHeight + 1, 0);

        yield return null;
        yield return null;

        Assert.IsTrue(gameObject == null);
    }
}
