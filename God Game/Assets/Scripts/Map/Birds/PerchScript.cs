// Copyright (c) 2018 Shachar Langbeheim. All rights reserved.

using UnityEngine;

public class PerchScript : MonoBehaviour {
  private TerrainObjectScript parentObject;

  private void OnStart() {
    parentObject = transform.gameObject.GetComponent<TerrainObjectScript>();
  }

  public bool IsStable() {
    return parentObject.isPlanted();
  }
}
