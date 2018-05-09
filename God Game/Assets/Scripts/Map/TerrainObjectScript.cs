using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainObjectScript: MonoBehaviour {
  private readonly float kMaxAngle = 45;

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

    if (transform.parent == null && !temporaryObject) {
      return;
    }

    if (!holdableAngle()) {
      if (temporaryObject) {
        SetRedColor();
      } else {
        TerrainObjectScript.freeObject(transform);
      }
    }
  }

  public static void freeObject(Transform obj) {
    obj.parent = null;
    Rigidbody rigidBody = obj.gameObject.AddComponent<Rigidbody>();
    rigidBody.mass = 5;
    rigidBody.useGravity = true;
    removePerchChildren(obj);
  }

  private static void removePerchChildren(Transform obj) {
    foreach(Transform child in obj) {
      if (child.tag == "lb_perchTarget") {
        Destroy(child.gameObject);
      }
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (!TemporaryObject || other.gameObject.GetComponent<TerrainObjectScript>() == null) {
      return;
    }
    if (collidingObjects.Count == 0) {
      SetRedColor();
    }
    collidingObjects.Add(other.gameObject);
    other.gameObject.GetComponent<TerrainObjectScript>().SetRedColor();
  }

  private void OnTriggerExit(Collider other) {
    if (!TemporaryObject || other.gameObject.GetComponent<TerrainObjectScript>() == null) {
      return;
    }
    collidingObjects.Remove(other.gameObject);
    other.gameObject.GetComponent<TerrainObjectScript>().SetOriginalColors();
    if (collidingObjects.Count == 0) {
      SetOriginalColors();
    }
  }

  public void SetRedColor() {
    if (originalColors != null) {
      return;
    }
    var materials = GetComponent<Renderer>().materials;
    originalColors = materials.Select(material => material.color).ToList();
    foreach (var material in materials) {
      material.color = Color.red;
    }
  }

  public void SetOriginalColors() {
    if (originalColors == null) {
      return;
    }
    var materials = GetComponent<Renderer>().materials.ToList();
    foreach (var material in materials) {
      var index = materials.IndexOf(material);
      material.color = originalColors[index];
    }
    originalColors = null;
  }

  public bool CanBePlanted() {
    return temporaryObject && holdableAngle() && collidingObjects.Count == 0;
  }

  private bool holdableAngle() {
    return Vector3.Angle(transform.up, Vector3.up) < kMaxAngle;
  }

  public void RemoveCollidingObjects() {
    foreach(var obj in collidingObjects) {
      Destroy(obj);
    }
    collidingObjects.Clear();
    SetOriginalColors();
  }
}
