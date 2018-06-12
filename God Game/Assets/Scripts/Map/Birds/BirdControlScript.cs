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
  List<BirdScript> myBirds = new List<BirdScript>();
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
      if (myBirds[i].gameObject.activeSelf) {
        myBirds[i].SendMessage("PauseBird");
      }
    }
  }

  public void AllUnPause() {
    pause = false;
    for (int i = 0; i < myBirds.Count; i++) {
      if (myBirds[i].gameObject.activeSelf) {
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

    for (int i = 0; i < 30; i++) {
      var bird = instantiateBird();
      bird.GetComponent<BirdScript>().enabled = false;
      bird.transform.position = new Vector3(Random.Range(-50, 50), Random.Range(20, 30), Random.Range(-50, 50));
    }
  }

  private void Update() {
     BoidFlock.MoveBoids(myBirds, Time.deltaTime * 3);
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
    myBirds.Add(newBird.GetComponent<BirdScript>());
    return newBird;
  }

  void OnEnable() {
    //StartCoroutine(updateTargets());
  }

  IEnumerator updateTargets() {
    while(true) {
      birdPerchTargets = FindObjectsOfType<PerchScript>().ToList();
      birdGroundTargets = GameObject.FindGameObjectsWithTag("lb_groundTarget").ToList();

      yield return new WaitForSeconds(0.5f);
    }
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
      var totalCount = birdGroundTargets.Count + birdPerchTargets.Count;
      if (Random.value * totalCount > birdGroundTargets.Count) {
        target = birdGroundTargets[Mathf.FloorToInt(Random.Range(0, birdGroundTargets.Count))];
        StartCoroutine(bird.FlyToGround(target));
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
