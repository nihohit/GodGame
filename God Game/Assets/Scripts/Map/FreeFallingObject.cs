using UnityEngine;

public class FreeFallingObject : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		if (transform.position.y > Constants.MaxHeight || transform.position.y < Constants.MinHeight) {
            Destroy(gameObject);
        }
	}

    public static void freeObject(Transform obj) {
        obj.parent = null;
        Rigidbody rigidBody = obj.gameObject.AddComponent<Rigidbody>();
        rigidBody.mass = 5;
        rigidBody.useGravity = true;
        obj.GetComponent<Collider>().enabled = true;
        obj.gameObject.AddComponent<FreeFallingObject>();
    }
}
