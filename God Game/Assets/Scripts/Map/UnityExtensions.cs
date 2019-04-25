using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class UnityExtensions {
  public static float DistanceIn2D(this Vector3 source, Vector3 target) {
    return Vector2.Distance(new Vector2(source.x, source.z), new Vector2(target.x, target.z));
  }

  public static Vector3 ToVector(this float3 vec) {
    return new Vector3(vec.x, vec.y, vec.z);
  }

  public static void CopyToVector(this float3 vec, Vector3 target) {
    target.x = vec.x;
    target.y = vec.y;
    target.z = vec.z;
  }

  public static float3 ToSlim(this Vector3 vec) {
    return math.float3(vec.x, vec.y, vec.z);
  }

  public static void ConvertInto(this List<Vector3> vectors, NativeArray<float3> floats) {
    for (int i = 0; i < floats.Length; i++) {
      floats[i] = vectors[i].ToSlim();
    }
  }

  public static void ConvertInto(this NativeArray<float3> floats, List<Vector3> vectors) {
    vectors.Clear();
    for (int i = 0; i < floats.Length; i++) {
      vectors.Add(floats[i].ToVector());
    }
  }
}
