using UnityEngine;
using System.Collections;

public class BirdScript: MonoBehaviour {
  enum birdBehaviors {
    sing,
    preen,
    ruffle,
    peck,
    hopForward,
    hopBackward,
    hopLeft,
    hopRight,
  }

  private enum birdState {
	  flying, idle, landing, landed, 
  }

  public AudioClip song1;
  public AudioClip song2;
  public AudioClip flyAway1;
  public AudioClip flyAway2;

  public bool fleeCrows = true;

  Animator anim;
  public BirdControlScript Controller { get; set; }

  bool paused = false;
  bool Perched { get { return Perch != null && state == birdState.landed; } }
  bool OnGround { get { return Ground != null && state == birdState.landed; } }
 birdState state;
	float distanceToTarget = 0.0f;
	float agitationLevel = .5f;
	float originalAnimSpeed = 1.0f;
	Vector3 originalVelocity = Vector3.zero;

	//hash variables for the animation states and animation properties
	int idleAnimationHash;
	int flyAnimationHash;
	int hopIntHash;
	int flyingBoolHash;
	int peckBoolHash;
	int ruffleBoolHash;
	int preenBoolHash;
	int landingBoolHash;
	int singTriggerHash;
	int flyingDirectionHash;

  public PerchScript Perch { get; set; }

  public GameObject Ground { get; set; }

  void OnEnable () {
		anim = gameObject.GetComponent<Animator>();

		idleAnimationHash = Animator.StringToHash("Base Layer.Idle");
		flyAnimationHash = Animator.StringToHash ("Base Layer.fly");
		hopIntHash = Animator.StringToHash ("hop");
		flyingBoolHash = Animator.StringToHash("flying");
		peckBoolHash = Animator.StringToHash("peck");
		ruffleBoolHash = Animator.StringToHash("ruffle");
		preenBoolHash = Animator.StringToHash("preen");
		landingBoolHash = Animator.StringToHash("landing");
		singTriggerHash = Animator.StringToHash ("sing");
		flyingDirectionHash = Animator.StringToHash("flyingDirectionX");
		anim.SetFloat ("IdleAgitated",agitationLevel);
		state = birdState.idle;
	}

	void PauseBird(){
		originalAnimSpeed = anim.speed;
		anim.speed = 0;
		if(!GetComponent<Rigidbody>().isKinematic){originalVelocity = GetComponent<Rigidbody>().velocity;}
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<AudioSource>().Stop ();
		paused = true;
	}

	void UnPauseBird(){
		anim.speed = originalAnimSpeed;
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().velocity = originalVelocity;
		paused = false;
	}

  public IEnumerator FlyToGround(GameObject ground) {
    this.Perch = null;
    this.Ground = ground;
    return FlyToTarget(FindPointInGroundTarget(ground));
  }

  
  Vector3 FindPointInGroundTarget(GameObject target) {
    //find a random point within the collider of a ground target that touches the ground
    Vector3 point;
    point.x = Random.Range(target.GetComponent<Collider>().bounds.max.x, target.GetComponent<Collider>().bounds.min.x);
    point.y = target.GetComponent<Collider>().bounds.max.y;
    point.z = Random.Range(target.GetComponent<Collider>().bounds.max.z, target.GetComponent<Collider>().bounds.min.z);
    //raycast down until it hits the ground
    RaycastHit hit;
    // TODO - keep raycasting until you find a clear spot
    if (Physics.Raycast(point, -Vector3.up, out hit, target.GetComponent<Collider>().bounds.size.y)) {
      return hit.point;
    }

    return point;
  }

  public IEnumerator FlyToPerch(PerchScript perch) {
    this.Ground = null;
    this.Perch = perch;
    return FlyToTarget(perch.transform.position);
  }

  public IEnumerator FlyToTarget(Vector3 target){
		if(Random.value < .5){
			GetComponent<AudioSource>().PlayOneShot (flyAway1,.1f);
		}else{
			GetComponent<AudioSource>().PlayOneShot (flyAway2,.1f);
		}
		state = birdState.flying;
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		GetComponent<Rigidbody>().drag = 0.5f;
		anim.applyRootMotion = false;
		anim.SetBool (flyingBoolHash,true);
		anim.SetBool(landingBoolHash, false);

		//Wait to apply velocity until the bird is entering the flying animation
		while(anim.GetCurrentAnimatorStateInfo(0).fullPathHash != flyAnimationHash){
			yield return 0;
		}

		//birds fly up and away from their perch for 1 second before orienting to the next target
		GetComponent<Rigidbody>().AddForce((transform.forward * 50.0f*Controller.birdScale)+(transform.up * 100.0f*Controller.birdScale));
		float t = 0.0f;
		while (t<1.0f){
			if(!paused){
				t+= Time.deltaTime;
			}
			yield return 0;
		}
		//start to rotate toward target
		Vector3 vectorDirectionToTarget = (target-transform.position).normalized;
		Quaternion finalRotation = Quaternion.identity;
		Quaternion startingRotation = transform.rotation;
		distanceToTarget = Vector3.Distance (transform.position,target);
		Vector3 forwardStraight;//the forward vector on the xz plane
		RaycastHit hit;
		Vector3 tempTarget = target;
		t = 0.0f;

		//if the target is directly above the bird the bird needs to fly out before going up
		//this should stop them from taking off like a rocket upwards
		if(vectorDirectionToTarget.y>.5f){
			tempTarget = transform.position + (new Vector3(transform.forward.x,.5f,transform.forward.z)*distanceToTarget);

			while(vectorDirectionToTarget.y>.5f){
				//Debug.DrawLine (tempTarget,tempTarget+Vector3.up,Color.red);
				vectorDirectionToTarget = (tempTarget-transform.position).normalized;
				finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
				transform.rotation = Quaternion.Slerp (startingRotation,finalRotation,t);
				anim.SetFloat (flyingDirectionHash,FindBankingAngle(transform.forward,vectorDirectionToTarget));
				t += Time.deltaTime*0.5f;
				GetComponent<Rigidbody>().AddForce(transform.forward * 70.0f*Controller.birdScale * Time.deltaTime);

				//Debug.DrawRay (transform.position,transform.forward,Color.green);

				vectorDirectionToTarget = (target-transform.position).normalized;//reset the variable to reflect the actual target and not the temptarget

				if (Physics.Raycast(transform.position,-Vector3.up,out hit,0.15f*Controller.birdScale) && GetComponent<Rigidbody>().velocity.y < 0){
					//if the bird is going to collide with the ground zero out vertical velocity
					if(shouldAbortOnHit(hit.collider)) {
						GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0.0f,GetComponent<Rigidbody>().velocity.z);
					}
				}
				if (Physics.Raycast(transform.position,Vector3.up,out hit,0.15f*Controller.birdScale) && GetComponent<Rigidbody>().velocity.y > 0){
					//if the bird is going to collide with something overhead zero out vertical velocity
					if(shouldAbortOnHit(hit.collider)) {
						GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0.0f,GetComponent<Rigidbody>().velocity.z);
					}
				}
				//check for collisions with non trigger colliders and abort flight if necessary
				forwardStraight = transform.forward;
				forwardStraight.y = 0.0f;
				//Debug.DrawRay (transform.position+(transform.forward*.1f),forwardStraight*.75f,Color.green);
				if (Physics.Raycast (transform.position+(transform.forward*.15f*Controller.birdScale),forwardStraight,out hit,.75f*Controller.birdScale)){
					if(shouldAbortOnHit(hit.collider)) {
						AbortFlyToTarget();
					}
				}
				yield return null;
			}
		}

		finalRotation = Quaternion.identity;
		startingRotation = transform.rotation;
		distanceToTarget = Vector3.Distance (transform.position,target);

		//rotate the bird toward the target over time
		while(transform.rotation != finalRotation || distanceToTarget >= 1.5f){
			if(!paused){
				distanceToTarget = Vector3.Distance (transform.position,target);
				vectorDirectionToTarget = (target-transform.position).normalized;
				if(vectorDirectionToTarget==Vector3.zero){
					vectorDirectionToTarget = new Vector3(0.0001f,0.00001f,0.00001f);
				}
				finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
				transform.rotation = Quaternion.Slerp (startingRotation,finalRotation,t);
				anim.SetFloat (flyingDirectionHash,FindBankingAngle(transform.forward,vectorDirectionToTarget));
				t += Time.deltaTime*0.5f;
				GetComponent<Rigidbody>().AddForce(transform.forward * 70.0f*Controller.birdScale * Time.deltaTime);
				if (Physics.Raycast(transform.position,-Vector3.up,out hit,0.15f*Controller.birdScale) && GetComponent<Rigidbody>().velocity.y < 0){
					//if the bird is going to collide with the ground zero out vertical velocity
					if(shouldAbortOnHit(hit.collider)) {
						GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0.0f,GetComponent<Rigidbody>().velocity.z);
					}
				}
				if (Physics.Raycast(transform.position,Vector3.up,out hit,0.15f*Controller.birdScale) && GetComponent<Rigidbody>().velocity.y > 0){
					//if the bird is going to collide with something overhead zero out vertical velocity
					if(shouldAbortOnHit(hit.collider)) {
						GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0.0f,GetComponent<Rigidbody>().velocity.z);
					}
				}

				//check for collisions with non trigger colliders and abort flight if necessary
				forwardStraight = transform.forward;
				forwardStraight.y = 0.0f;
				//Debug.DrawRay (transform.position+(transform.forward*.1f),forwardStraight*.75f,Color.green);
				if (Physics.Raycast (transform.position+(transform.forward*.15f*Controller.birdScale),forwardStraight,out hit,.75f*Controller.birdScale)){
					if(shouldAbortOnHit(hit.collider)) {
						AbortFlyToTarget();
					}
				}
			}
			yield return 0;
		}

		//keep the bird pointing at the target and move toward it
		float flyingForce = 50.0f*Controller.birdScale;
		while(true){
			if(!paused){
				//do a raycast to see if the bird is going to hit the ground
				if (Physics.Raycast(transform.position,-Vector3.up,0.15f*Controller.birdScale) && GetComponent<Rigidbody>().velocity.y < 0){
					GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0.0f,GetComponent<Rigidbody>().velocity.z);
				}
				if (Physics.Raycast(transform.position,Vector3.up,out hit,0.15f*Controller.birdScale) && GetComponent<Rigidbody>().velocity.y > 0){
					//if the bird is going to collide with something overhead zero out vertical velocity
					if(shouldAbortOnHit(hit.collider)) {
						GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0.0f,GetComponent<Rigidbody>().velocity.z);
					}
				}

				//check for collisions with non trigger colliders and abort flight if necessary
				forwardStraight = transform.forward;
				forwardStraight.y = 0.0f;
				//Debug.DrawRay (transform.position+(transform.forward*.1f),forwardStraight*.75f,Color.green);
				if (Physics.Raycast (transform.position+(transform.forward*.15f*Controller.birdScale),forwardStraight,out hit,.75f*Controller.birdScale)){
					if(shouldAbortOnHit(hit.collider)) {
						AbortFlyToTarget();
					}
				}

				vectorDirectionToTarget = (target-transform.position).normalized;
				finalRotation = Quaternion.LookRotation(vectorDirectionToTarget);
				anim.SetFloat (flyingDirectionHash,FindBankingAngle(transform.forward,vectorDirectionToTarget));
				transform.rotation = finalRotation;
				GetComponent<Rigidbody>().AddForce(transform.forward * flyingForce * Time.deltaTime);
				distanceToTarget = Vector3.Distance (transform.position,target);
				if(distanceToTarget <= 1.5f*Controller.birdScale){
					if(distanceToTarget < 0.5f*Controller.birdScale){
						break;	
					}else{
						GetComponent<Rigidbody>().drag = 2.0f;
						flyingForce = 50.0f*Controller.birdScale;
					}
				}else if (distanceToTarget <= 5.0f*Controller.birdScale){
					GetComponent<Rigidbody>().drag = 1.0f;
					flyingForce = 50.0f*Controller.birdScale;
				}
			}
			yield return 0;
		}

		anim.SetFloat (flyingDirectionHash,0);
		//initiate the landing for the bird to finally reach the target
		Vector3 vel = Vector3.zero;
		state = birdState.landing;
		anim.SetBool(landingBoolHash, true);
		anim.SetBool (flyingBoolHash,false);
		t = 0.0f;
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		//tell any birds that are in the way to move their butts
		Collider[] hitColliders = Physics.OverlapSphere(target,0.05f*Controller.birdScale);
		for(int i=0;i<hitColliders.Length;i++){
			if (hitColliders[i].tag == "lb_bird" && hitColliders[i].transform != transform){
				hitColliders[i].SendMessage ("FlyAway");
			}
		}

		//this while loop will reorient the rotation to vertical and translate the bird exactly to the target
		startingRotation = transform.rotation;
		transform.localEulerAngles = new Vector3(0.0f,transform.localEulerAngles.y,0.0f);
		finalRotation = transform.rotation;
		transform.rotation = startingRotation;
		while (distanceToTarget>0.05f*Controller.birdScale){
			if(!paused){
				transform.rotation = Quaternion.Slerp (startingRotation,finalRotation,t*4.0f);
				transform.position = Vector3.SmoothDamp(transform.position,target,ref vel,0.5f);
				t += Time.deltaTime;
				distanceToTarget = Vector3.Distance (transform.position,target);
				if(t>2.0f){
					break;//failsafe to stop birds from getting stuck
				}
			}
			yield return 0;
		}
		GetComponent<Rigidbody>().drag = .5f;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		anim.SetBool(landingBoolHash, false);
		state = birdState.landed;
		transform.localEulerAngles = new Vector3(0.0f,transform.localEulerAngles.y,0.0f);
		transform.position = target;
		anim.applyRootMotion = true;
	}

	//Sets a variable between -1 and 1 to control the left and right banking animation
	float FindBankingAngle(Vector3 birdForward, Vector3 dirToTarget){
		Vector3 cr = Vector3.Cross (birdForward,dirToTarget);
		float ang = Vector3.Dot (cr,Vector3.up);
		return ang;
	}
	
	bool shouldAbortOnHit(Collider col) {
		if (col.isTrigger) {
			return false;
		}
		Debug.Log(string.Format("Hit {0}", col.gameObject.name));
		bool result = true;
		if (Perch != null) {
			result = col.gameObject != Perch.gameObject &&
				col.gameObject != Perch.transform.parent.gameObject;
			Debug.Log(string.Format("Was aiming towards {0} in {1}, hit {2}, should abort: {3}", 
				Perch.name, Perch.transform.parent.name, col.gameObject.name, result));
			return result;
		} else if (Ground != null) {
			result = col.gameObject != Ground.gameObject;
			Debug.Log(string.Format("Was aiming towards {0}, hit {1}, should abort: {2}", 
				Ground.name, col.gameObject.name, result));
		}

		return result;
	}

	void OnGroundBehaviors() {
		GetComponent<Rigidbody>().isKinematic = true;
		if(anim.GetCurrentAnimatorStateInfo(0).fullPathHash == idleAnimationHash){
			//the bird is in the idle animation, lets randomly choose a behavior every 3 seconds
			if (Random.value < Time.deltaTime*.33){
				//bird will display a behavior
				//in the Perched state the bird can only sing, preen, or ruffle
				float rand = Random.value;
				if (rand < .3){
					DisplayBehavior(birdBehaviors.sing);
				}else if (rand < .5){
					DisplayBehavior(birdBehaviors.peck);
				}else if (rand < .6){
					DisplayBehavior(birdBehaviors.preen);	
				}else if (!Perched && rand<.7){
					DisplayBehavior(birdBehaviors.ruffle);	
				}else if (!Perched && rand <.85){
					DisplayBehavior(birdBehaviors.hopForward);	
				}else if (!Perched && rand < .9){
					DisplayBehavior(birdBehaviors.hopLeft);	
				}else if (!Perched && rand <.95){
					DisplayBehavior(birdBehaviors.hopRight);
				}else if (!Perched && rand <= 1){
					DisplayBehavior(birdBehaviors.hopBackward);	
				}else{
					DisplayBehavior(birdBehaviors.sing);	
				}
				//lets alter the agitation level of the brid so it uses a different mix of idle animation next time
				anim.SetFloat ("IdleAgitated",Random.value);
			}
			//birds should fly to a new target about every 10 seconds
			if (Random.value < Time.deltaTime*.1){
				FlyAway ();
			}
		}
	}
	
	void DisplayBehavior(birdBehaviors behavior){
		state = birdState.landed;
		switch (behavior){
		case birdBehaviors.sing:
			anim.SetTrigger(singTriggerHash);			
			break;
		case birdBehaviors.ruffle:
			anim.SetTrigger(ruffleBoolHash);
			break;
		case birdBehaviors.preen:
			anim.SetTrigger(preenBoolHash);			
			break;
		case birdBehaviors.peck:
			anim.SetTrigger(peckBoolHash);			
			break;
		case birdBehaviors.hopForward:
			anim.SetInteger (hopIntHash, 1);			
			break;
		case birdBehaviors.hopLeft:
			anim.SetInteger (hopIntHash, -2);			
			break;
		case birdBehaviors.hopRight:
			anim.SetInteger (hopIntHash, 2);
			break;
		case birdBehaviors.hopBackward:
			anim.SetInteger (hopIntHash, -1);			
			break;
		}
	}

	void OnTriggerEnter(Collider col){
		if (col.tag == "lb_bird"){
			FlyAway ();
		}
	}

	void OnTriggerExit(Collider col){
		//if bird has hopped out of the target area lets fly
		if (OnGround && (col.tag == "lb_groundTarget" || col.tag == "lb_perchTarget")){
			FlyAway ();
		}
	}

	void AbortFlyToTarget(){
		StopCoroutine("FlyToTarget");
		anim.SetBool(landingBoolHash, false);
		anim.SetFloat (flyingDirectionHash,0);
		transform.localEulerAngles = new Vector3(
			0.0f,
			transform.localEulerAngles.y,
			0.0f);
		FlyAway ();
	}
	
	void FlyAway(){
    Perch = null;
    Ground = null;
    transform.parent = null;
		StopCoroutine("FlyToTarget");
		anim.SetBool(landingBoolHash, false);
		Controller.BirdFindTarget(this);
	}

	void CrowIsClose(){
		if (fleeCrows){
			Flee ();
		}
	}
		
	void Flee(){
		Perch = null;
    	Ground = null;
    	transform.parent = null;
		StopCoroutine("FlyToTarget");
		GetComponent<AudioSource>().Stop();
		anim.Play(flyAnimationHash);
		Vector3 farAwayTarget = transform.position;
		farAwayTarget += new Vector3(Random.Range (-100,100)*Controller.birdScale,10*Controller.birdScale,Random.Range (-100,100)*Controller.birdScale);
		StartCoroutine("FlyToTarget",farAwayTarget);
	}

	void ResetHopInt(){
		anim.SetInteger (hopIntHash, 0);
	}

	void PlaySong(){
		if(Random.value < .5){
			GetComponent<AudioSource>().PlayOneShot (song1,1);
		}else{
			GetComponent<AudioSource>().PlayOneShot (song2,1);
		}
	}

	void Update () {
		if (paused) {
		return;
		}
		if(OnGround){
			OnGroundBehaviors();	
		}
		if (Perch != null && !Perch.IsStable()) {
			FlyAway();
		}
	}
}
