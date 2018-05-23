// Copyright (c) 2018 Shachar Langbeheim. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BirdControlScript: MonoBehaviour {
  public int maximumNumberOfBirds;
  public bool highQuality = true;
  public LayerMask groundLayer;
  public float birdScale = 1.0f;

  public bool robin = true;
  public bool blueJay = true;
  public bool cardinal = true;
  public bool chickadee = true;
  public bool sparrow = true;
  public bool goldFinch = true;
  public bool crow = true;

  bool pause = false;
  List<GameObject> myBirds = new List<GameObject>();
  List<string> myBirdTypes = new List<string>();
  List<GameObject> birdGroundTargets = new List<GameObject>();
  List<PerchScript> birdPerchTargets = new List<PerchScript>();
  GameObject[] featherEmitters = new GameObject[3];

  public void Pause() {
    if (pause) {
      AllUnPause();
    } else {
      AllPause();
    }
  }

  public void AllPause() {
    pause = true;
    for (int i = 0; i < myBirds.Count; i++) {
      if (myBirds[i].activeSelf) {
        myBirds[i].SendMessage("PauseBird");
      }
    }
  }

  public void AllUnPause() {
    pause = false;
    for (int i = 0; i < myBirds.Count; i++) {
      if (myBirds[i].activeSelf) {
        myBirds[i].SendMessage("UnPauseBird");
      }
    }
  }

  void Start() {
    //set up the bird types to use
    if (robin) {
      myBirdTypes.Add("lb_robin");
    }
    if (blueJay) {
      myBirdTypes.Add("lb_blueJay");
    }
    if (cardinal) {
      myBirdTypes.Add("lb_cardinal");
    }
    if (chickadee) {
      myBirdTypes.Add("lb_chickadee");
    }
    if (sparrow) {
      myBirdTypes.Add("lb_sparrow");
    }
    if (goldFinch) {
      myBirdTypes.Add("lb_goldFinch");
    }
    if (crow) {
      myBirdTypes.Add("lb_crow");
    }

    //instantiate 3 feather emitters for killing the birds
    GameObject fEmitter = Resources.Load("featherEmitter", typeof(GameObject)) as GameObject;
    for (int i = 0; i < 3; i++) {
      featherEmitters[i] = Instantiate(fEmitter, Vector3.zero, Quaternion.identity) as GameObject;
      featherEmitters[i].transform.parent = transform;
      featherEmitters[i].SetActive(false);
    }
  }

  public void AddBird(PerchScript perch) {
    var bird = instantiateBird();
    bird.Perch = perch;
    bird.transform.position = perch.transform.position;
    bird.transform.parent = perch.transform;
  }

  BirdScript instantiateBird() {
    GameObject birdPrefab;
    if (highQuality) {
      birdPrefab = Resources.Load(myBirdTypes[Random.Range(0, myBirdTypes.Count)] + "HQ", typeof(GameObject)) as GameObject;
    } else {
      birdPrefab = Resources.Load(myBirdTypes[Random.Range(0, myBirdTypes.Count)], typeof(GameObject)) as GameObject;
    }
    var newBird = Instantiate<GameObject>(birdPrefab).GetComponent<BirdScript>();
    newBird.Controller = this;
    newBird.transform.localScale = Vector3.one * birdScale;
    myBirds.Add(birdPrefab);
    return newBird;
  }

  void OnEnable() {
    StartCoroutine(updateTargets());
  }

  IEnumerator updateTargets() {
    while(true) {
      birdPerchTargets = FindObjectsOfType<PerchScript>().ToList();

      yield return new WaitForSeconds(0.5f);
    }
  }

  Vector3 FindPointInGroundTarget(GameObject target) {
    //find a random point within the collider of a ground target that touches the ground
    Vector3 point;
    point.x = Random.Range(target.GetComponent<Collider>().bounds.max.x, target.GetComponent<Collider>().bounds.min.x);
    point.y = target.GetComponent<Collider>().bounds.max.y;
    point.z = Random.Range(target.GetComponent<Collider>().bounds.max.z, target.GetComponent<Collider>().bounds.min.z);
    //raycast down until it hits the ground
    RaycastHit hit;
    if (Physics.Raycast(point, -Vector3.up, out hit, target.GetComponent<Collider>().bounds.size.y, groundLayer)) {
      return hit.point;
    }

    return point;
  }

  bool AreThereActiveTargets() {
    if (birdGroundTargets.Count > 0 || birdPerchTargets.Count > 0) {
      return true;
    } else {
      return false;
    }
  }

  public void BirdFindTarget(BirdScript bird) {
    //yield return new WaitForSeconds(1);
    GameObject target;
    if (birdGroundTargets.Count > 0 || birdPerchTargets.Count > 0) {
      //pick a random target based on the number of available targets vs the area of ground targets
      //each perch target counts for .3 area, each ground target's area is calculated
      float gtArea = 0.0f;
      float ptArea = birdPerchTargets.Count * 0.3f;

      for (int i = 0; i < birdGroundTargets.Count; i++) {
        gtArea += birdGroundTargets[i].GetComponent<Collider>().bounds.size.x * birdGroundTargets[i].GetComponent<Collider>().bounds.size.z;
      }
      if (ptArea == 0.0f || Random.value < gtArea / (gtArea + ptArea)) {
        target = birdGroundTargets[Mathf.FloorToInt(Random.Range(0, birdGroundTargets.Count))];
        StartCoroutine(bird.FlyToTarget(FindPointInGroundTarget(target)));
      } else {
        var perch = birdPerchTargets[Mathf.FloorToInt(Random.Range(0, birdPerchTargets.Count))];
        StartCoroutine(bird.FlyToPerch(perch));
      }
    }
  }

  public void EmitFeather(Vector3 pos) {
    foreach (GameObject fEmit in featherEmitters) {
      if (!fEmit.activeSelf) {
        fEmit.transform.position = pos;
        fEmit.SetActive(true);
        StartCoroutine("DeactivateFeathers", fEmit);
        break;
      }
    }
  }

  IEnumerator DeactivateFeathers(GameObject featherEmit) {
    yield return new WaitForSeconds(4.5f);
    featherEmit.SetActive(false);
  }
}
