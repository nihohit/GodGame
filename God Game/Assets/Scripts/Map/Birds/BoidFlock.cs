using System.Collections.Generic;
using System.Linq;

using UnityEngine;
// http://www.kfish.org/boids/pseudocode.html
public static class BoidFlock {

    public static void MoveBoids(List<BirdScript> boids, float speed) {
        foreach (var bird in boids) {
            var v1 = rule1(bird, boids);
            var v2 = rule2(bird, boids);
            var v3 = rule3(bird, boids);
        }

    }

    private static Vector3 rule1(BirdScript bird, List<BirdScript> boids) {
        var centerOfOtherBoids = boids.Where(boid => boid != bird).Sum(boid => boid.transform.position) / (boids.count - 1);
        return bird.transform.position - centerOfOtherBoids;
    }

    private static Vector3 rule2(BirdScript bird, IEnumerable<BirdScript> boids) {
        return Vector3.zero;
    }

    private static Vector3 rule3(BirdScript bird, IEnumerable<BirdScript> boids) {
        return Vector3.zero;
    }
}