using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour {

	Rigidbody mRigidbody;
	public Vector3 force = new Vector3(0,100,0);

	void Awake(){
		mRigidbody = GetComponent<Rigidbody> ();
	}

	Vector3 mVec;
	void Update(){
		if(Input.GetKeyDown(KeyCode.H)){
			mRigidbody.AddForce (force);
		}
		if(Input.GetKeyDown(KeyCode.G)){
			mRigidbody.AddForce (new Vector3(-force.x,force.y,force.z));
		}
		if(Input.GetKeyDown(KeyCode.F)){
			mRigidbody.useGravity = false;
			mVec = mRigidbody.velocity;
			mRigidbody.velocity = Vector3.zero;
		}
		if(Input.GetKeyDown(KeyCode.D)){
//			Time.fixedUnscaledTime = 1;
			mRigidbody.useGravity = true;
			mRigidbody.velocity = mVec;
		}
	}

}
