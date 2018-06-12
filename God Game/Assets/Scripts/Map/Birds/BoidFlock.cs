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

            Debug.Log("v1:" +v1+ " v2: " + v2 + " v3:" + v3);
            var velocity = (v1 + v2 + v3) * speed;
            bird.GetComponent<Rigidbody>().velocity = velocity;
            bird.transform.position += velocity;
        }
    }

    private static Vector3 rule1(BirdScript bird, List<BirdScript> boids) {
        var centerOfOtherBoids = boids
            .Where(boid => boid != bird)
            .Aggregate(new Vector3(0,0,0), (sum, boid) => sum + boid.transform.position) / (boids.Count - 1);
        return Vector3.Normalize(centerOfOtherBoids - bird.transform.position);
    }

    private static Vector3 rule2(BirdScript bird, IEnumerable<BirdScript> boids) {
        const float kDistance = 2;
        var colliders = Physics.OverlapSphere(bird.transform.position, kDistance)
            .Where(collider => collider.gameObject != bird.gameObject).ToList();
        if (colliders.Count == 0) {
            return Vector3.zero;
        }
        var sumDistances = Vector3.zero;
        foreach (var collider in colliders) {
            sumDistances += bird.transform.position - collider.transform.position;
        }
        return sumDistances / colliders.Count;
    }

    private static Vector3 rule3(BirdScript bird, IEnumerable<BirdScript> boids) {
        const float kDistance = 5;
        var colliders = Physics.OverlapSphere(bird.transform.position, kDistance)
            .Where(collider => collider.gameObject != bird.gameObject).ToList();
        if (colliders.Count == 0) {
            return Vector3.zero;
        }
        var sumVelocities = Vector3.zero;
        foreach (var collider in colliders) {
            sumVelocities += collider.GetComponent<Rigidbody>().velocity;
        }
        return sumVelocities / colliders.Count;
    }
}