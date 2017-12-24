using UnityEngine;

public class TerrainObjectScript : MonoBehaviour {
    void Update () {
		if (transform.position.y > Constants.MaxHeight || transform.position.y < Constants.MinHeight) {
            Destroy(gameObject);
            return;
        }
    }

    public static bool shouldFreeObject(Transform obj) {
        return Vector3.Angle(obj.up, Vector3.up) > 45;
    }

    public static void freeObject(Transform obj) {
        obj.parent = null;
        Rigidbody rigidBody = obj.gameObject.AddComponent<Rigidbody>();
        rigidBody.mass = 20;
        rigidBody.useGravity = true;
    }
}
