using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainObjectScript : MonoBehaviour {
  private readonly float kMaxAngle = 45;
  private static int id = 0;

  private bool temporaryObject;
  public bool TemporaryObject {
    get {
      return temporaryObject;
    }
    set {
      temporaryObject = value;
      collidingObjects = value ? new List<TerrainObjectScript>() : null;
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
  private List<TerrainObjectScript> collidingObjects;
  public bool markedForDestruction = false;

  private void Start() {
    gameObject.name += ++id;
  }

  private void OnDestroy() {
    if (collidingObjects == null) {
      return;
    }
    foreach (var obj in collidingObjects) {
      obj.setOriginalColors();
    }
    collidingObjects = null;
  }

  public void freeObject() {
    transform.parent = null;
    Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
    rigidBody.mass = 5;
    rigidBody.useGravity = true;
  }

  public bool isOutOfPlayableHeight() {
    return transform.position.y > Constants.MaxHeight || transform.position.y < Constants.MinHeight;
  }

  private void OnTriggerEnter(Collider other) {
    if (!TemporaryObject || other.gameObject.GetComponent<TerrainObjectScript>() == null) {
      return;
    }
    var terrainObject = other.GetComponent<TerrainObjectScript>();
    collidingObjects.Add(terrainObject);
    terrainObject.setRedColor();
  }

  private void OnTriggerExit(Collider other) {
    if (!TemporaryObject || other.gameObject.GetComponent<TerrainObjectScript>() == null) {
      return;
    }
    var terrainObject = other.GetComponent<TerrainObjectScript>();
    collidingObjects.Remove(terrainObject);
    terrainObject.setOriginalColors();
    if (collidingObjects.Count == 0) {
      setOriginalColors();
    }
  }

  public void setRedColor() {
    if (originalColors != null) {
      return;
    }
    var materials = GetComponent<Renderer>().materials;
    originalColors = materials.Select(material => material.color).ToList();
    foreach (var material in materials) {
      material.color = Color.red;
    }
  }

  public void setOriginalColors() {
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

  public bool holdableAngle() {
    return Vector3.Angle(transform.up, Vector3.up) < kMaxAngle;
  }

  public void MarkCollidingObjectsForDestruction() {
    foreach (var obj in collidingObjects) {
      obj.markedForDestruction = true;
    }
    collidingObjects.Clear();
    setOriginalColors();
  }

  public bool isColliding() {
    return collidingObjects.Count > 0;
  }
}