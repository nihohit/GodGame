using UnityEngine;

public class TerrainObjectScript : MonoBehaviour {

    void Update () {
		if (transform.position.y > Constants.MaxHeight || transform.position.y < Constants.MinHeight) {
            Destroy(gameObject);
            return;
        }

        if (transform.parent == null) {
            return;
        }

        if (Vector3.Angle(transform.up, Vector3.up) > 45) {
            TerrainObjectScript.freeObject(transform);
        }
    }

    public static void freeObject(Transform obj) {
        obj.parent = null;
        Rigidbody rigidBody = obj.gameObject.AddComponent<Rigidbody>();
        rigidBody.mass = 5;
        rigidBody.useGravity = true;
    }
}
