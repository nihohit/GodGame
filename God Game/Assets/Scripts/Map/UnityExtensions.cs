using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class UnityExtensions {
  public static float DistanceIn2D(this Vector3 source, Vector3 target) {
    return Vector2.Distance(new Vector2(source.x, source.z), new Vector2(target.x, target.z));
  }
}
