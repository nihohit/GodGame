using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainObjectScript: MonoBehaviour {
  private bool temporaryObject;
  public bool TemporaryObject { get {
      return temporaryObject;
    } set {
      temporaryObject = value;
      collidingObjects = value ? new List<GameObject>() : null;
      gameObject.GetComponent<Collider>().isTrigger = value;
      if (value) {
        var body = gameObject.AddComponent<Rigidbody>();
        body.useGravity = false;
      } else {
        Destroy(gameObject.GetComponent<Rigidbody>());
      }
    }
  }
  private List<Color> originalColors;
  private List<GameObject> collidingObjects;

  void Update() {
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

  private void OnTriggerEnter(Collider other) {
    if (!TemporaryObject || other.gameObject.GetComponent<TerrainObjectScript>() == null) {
      return;
    }
    if (collidingObjects.Count == 0) {
      setRedColor();
    }
    collidingObjects.Add(other.gameObject);
    other.gameObject.GetComponent<TerrainObjectScript>().setRedColor();
  }

  private void OnTriggerExit(Collider other) {
    if (!TemporaryObject || other.gameObject.GetComponent<TerrainObjectScript>() == null) {
      return;
    }
    collidingObjects.Remove(other.gameObject);
    other.gameObject.GetComponent<TerrainObjectScript>().setOriginalColors();
    if (collidingObjects.Count == 0) {
      setOriginalColors();
    }
  }

  private void setRedColor() {
    var materials = GetComponent<Renderer>().materials;
    originalColors = materials.Select(material => material.color).ToList();
    foreach (var material in materials) {
      material.color = Color.red;
    }
  }

  private void setOriginalColors() {
    var materials = GetComponent<Renderer>().materials.ToList();
    foreach (var material in materials) {
      var index = materials.IndexOf(material);
      material.color = originalColors[index];
    }
    originalColors = null;
  }
}
