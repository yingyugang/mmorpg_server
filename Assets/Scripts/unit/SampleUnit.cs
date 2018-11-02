using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleUnit : MonoBehaviour {

	public int playerId;
	Vector2 mTargetPos;
	Vector3 mDirect;
	public float speed;
	Transform mTrans;
	bool mIsMoving;

	void Awake(){
		mTrans = transform;
	}

	public void MoveTo(Vector2 targetPos){
		mTargetPos = targetPos;
		mTrans.LookAt (new Vector3(targetPos.x / 1000,mTrans.position.y,targetPos.y / 1000));
		mDirect = mTrans.forward;
		mIsMoving = true;
	}

	public void FrameUpdate(){
		if (mIsMoving) {
			float currentDic = Vector3.Distance (mTrans.position, new Vector3 (mTargetPos.x / 1000, mTrans.position.y, mTargetPos.y / 1000));
			if (currentDic > speed) {
				mTrans.position += mDirect * speed;
			} else {
				mTrans.position += mDirect * currentDic;
				mIsMoving = false;
			}
		}
	}


}
