using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRes : MonoBehaviour{

	public string MOVE_ANIM_CLIP = "run";
	public string ATTACK_ANIM_CLIP = "attack";
	public string STANDBY_ANIM_CLIP = "idle";

	[SerializeField]
	Animation mAnimation;
	[HideInInspector]
	public Unit unit;

	[SerializeField]
	Transform mWeaponRef;

	Transform mTrans;
	void Awake(){
		mTrans = transform;
	}

	public Vector3 weaponPoint{
		get{
			if (mWeaponRef != null)
				return mWeaponRef.position;
			else
				return mTrans.position;
		}
	}

	public void PlayMove(){
		if (mAnimation != null)
			mAnimation.Play (MOVE_ANIM_CLIP);
	}

	public void PlayStandBy(){
		if (mAnimation != null)
			mAnimation.Play (STANDBY_ANIM_CLIP);
	}

	public void PlayAttack()
	{
		if (mAnimation != null)
			mAnimation.Play (ATTACK_ANIM_CLIP);
	}

}
