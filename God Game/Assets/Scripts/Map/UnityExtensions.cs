// Copyright (c) 2018 Shachar Langbeheim. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UnityExtensions {
  public static GameObject InstantiateObject(this Object obj, Object prefab, Vector3 position) {
    return (GameObject)Object.Instantiate(prefab, position, Quaternion.identity);
  }

  public static GameObject InstantiateObject(this Object obj, Object prefab) {
    return obj.InstantiateObject(prefab, Vector3.zero);
  }

  public static System.Nullable<RaycastHit> CurrentMousePointedTile(this Object obj) {
    return obj.CurrentMousePointedObject(1 << 8);
  }

  public static System.Nullable<RaycastHit> CurrentMousePointedTree(this Object obj) {
    return obj.CurrentMousePointedObject(1 << 9);
  }

  public static System.Nullable<RaycastHit> CurrentMousePointedObject(this Object obj, LayerMask mask) {
    RaycastHit hit = new RaycastHit();
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (!Physics.Raycast(ray, out hit, float.MaxValue, mask)) {
      return null;
    }
    return hit;
  }
}
