using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class TerrainObjectScriptPlayTests {
	[UnityTest]
	public IEnumerator objectShouldBeDestroyedWhenTooHigh() {
        var gameObject = new GameObject();
        var fallingScripts = gameObject.AddComponent<TerrainObjectScript>();

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
        var fallingScripts = gameObject.AddComponent<TerrainObjectScript>();

        yield return null;

        Assert.IsFalse(gameObject == null);

        fallingScripts.transform.position = new Vector3(0, Constants.MaxHeight + 1, 0);

        yield return null;
        yield return null;

        Assert.IsTrue(gameObject == null);
    }

    [UnityTest]
    public IEnumerator objectShouldDisconnectWhenRotationIsLarge() {
        var gameObject = new GameObject();
        var parent = new GameObject();
        var fallingScripts = gameObject.AddComponent<TerrainObjectScript>();
        gameObject.transform.parent = parent.transform;

        yield return null;

        Assert.AreEqual(parent.transform, gameObject.transform.parent);
        Assert.IsNull(gameObject.GetComponent<Rigidbody>());

        fallingScripts.transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(1, 1, 0));

        yield return null;

        Assert.AreEqual(parent.transform, gameObject.transform.parent);
        Assert.IsNull(gameObject.GetComponent<Rigidbody>());

        fallingScripts.transform.rotation = fallingScripts.transform.rotation = Quaternion.FromToRotation(Vector3.up, new Vector3(1, 1, 1));

        yield return null;

        Assert.IsNull(gameObject.transform.parent);
        Assert.IsNotNull(gameObject.GetComponent<Rigidbody>());

        yield return null;
    }
}
