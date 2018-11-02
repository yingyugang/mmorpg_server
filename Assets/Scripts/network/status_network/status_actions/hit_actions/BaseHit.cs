using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	public class BaseHit : UnitBaseAction
	{
		protected BaseSkill mBaseSkill;
		protected Vector3 mHitPos;
		protected bool mIsFriends;
		protected MMOUnit mainTarget;

		public virtual void OnHit (Vector3 hitPos, bool isFriends, MMOUnit target)
		{
			mHitPos = hitPos;
			this.mIsFriends = isFriends;
			this.mainTarget = target;
			HitInfo hitInfo = GetHitInfo ();
			if (hitInfo != null)
				MMOBattleServerManager.Instance.AddHitInfo (hitInfo);
		}

		//TODO 如果attacker在击中前就被销毁，比如被击杀，那么后面的逻辑就会报空指针异常。
		//TODO 如果attacker被销毁了就无法计算伤害值，如果对数值要求不是很高对话，在计算的时候进行一下空指针判断就好。
		//TODO 如果是远程攻击，那么应该以攻击时的攻击者属性为准，而不是击中时候的属性。
		public virtual void Init (MMOUnit attacker, BaseSkill skill)
		{
			this.mmoUnit = attacker;
			this.mBaseSkill = skill;
		}

		public virtual HitInfo GetHitInfo ()
		{
			HitInfo hitInfo = new HitInfo ();
			return hitInfo;
		}

		Collider[] mColls = new Collider[BattleConst.MAX_SCANE_ABLE_COUNT];
		//TODO 一般的に、不会去计算最近和正前面，排序的销毁有一点不可控。
		protected virtual List<MMOUnit> GetHittedUnits ()
		{
			List<MMOUnit> hittedUnits = new List<MMOUnit> ();
			if (mmoUnit == null)
				return hittedUnits;
			int hitCount = 0;
			//枪械类，由于子弹飞行太快时间太短，视觉上几乎不能判断，并且如果做碰撞检测的化，性能消耗极高，所以直接造成伤害，客户端只做模拟的弹道演出，和开火特效演出
			//或者把子弹长度改为原来的数十倍的碰撞检测，然后按照原来的频率进行碰撞检测，这样有没有必要，需要检讨。
//			if (mBaseSkill.skillCSVStructure.impact_type == 1) {
//				Vector3 forward = mmoUnit.Forward;
//				Vector3 position = mmoUnit.BodyPos;
//				//由客户端控制的射击
//				if(mmoUnit.targetStatusInfo!=null){
//					//加上unit的视角。
//					forward = IntVector3.ToVector3(mmoUnit.targetStatusInfo.forward);
//					position = IntVector3.ToVector3 (mmoUnit.targetStatusInfo.position);
//				}
//				//子弹
////				if(mBaseSkill.skillCSVStructure.impact_count == 1){
////					RaycastHit hit;
////					if(Physics.Raycast(position,forward,out hit,Mathf.Infinity)){
////						Vector3 pos = hit.point;
////						Vector3 normal = hit.normal;
////					}
////				}
//				int count = Physics.RaycastNonAlloc (position,forward, mRayHits, mBaseSkill.skillCSVStructure.range, 1 << LayerConstant.LAYER_UNIT);
//				Debug.Log (string.Format("{0}:{1}:{2}",JsonUtility.ToJson(position),JsonUtility.ToJson(forward),count));
//				for (int i = 0; i < count; i++) {
//					MMOUnit unit = mRayHits [i].collider.GetComponent<MMOUnit> ();
//					if (!unit.IsDeath) {
//						if (!mIsFriends && this.mmoUnit.camp != unit.camp) {
//							hittedUnits.Add (unit);
//						} else if (mIsFriends && this.mmoUnit.camp == unit.camp) {
//							hittedUnits.Add (unit);
//						}
//					}
//				}
//			} else {

				if (mBaseSkill.skillCSVStructure.hit_impact_count > 0) {
					hitCount = Physics.OverlapSphereNonAlloc (this.mHitPos, mmoUnit.unitCSVStructure.width + mBaseSkill.skillCSVStructure.hit_range, mColls, 1 << LayerConstant.LAYER_UNIT);
					Vector3 forward = this.mmoUnit.transform.forward;
					forward = new Vector3 (forward.x, 0, forward.z).normalized;
					int maxHitCount = mBaseSkill.skillCSVStructure.hit_impact_count;
					if(mainTarget!=null){
						hittedUnits.Add (mainTarget);
						maxHitCount--;
					}
					for (int i = 0; i < hitCount; i++) {
						if(hittedUnits.Count>=maxHitCount){
							break;
						}
						MMOUnit unit = mColls [i].GetComponent<MMOUnit> ();
						if(mainTarget!=null && mainTarget == unit){
							continue;
						}
						if (unit.IsDeath)
							continue;
						if (mIsFriends && unit.camp != this.mmoUnit.camp) {
							continue;
						}
						if (!mIsFriends && unit.camp == this.mmoUnit.camp) {
							continue;
						}
						//攻撃者前の数度以内。
						if (mBaseSkill.skillCSVStructure.hit_check_radiu < 360 && mBaseSkill.skillCSVStructure.hit_check_radiu > 0) {
							//コストがある
							Vector3 direct = new Vector3 (unit.transform.position.x - this.mmoUnit.transform.position.x, 0, unit.transform.position.z - this.mmoUnit.transform.position.z);
							direct = direct.normalized;
							float acos = Vector3.Dot (direct, forward);
							if (acos > 1.0f)
								acos = 1.0f;
							else if (acos < -1.0f)
								acos = -1.0f;
							float angle = Mathf.Acos (acos);
							if (acos == 1 || acos == -1) {
								angle = 0;
							}
							angle = angle * 180 / Mathf.PI;
							if (Mathf.Abs (angle) < mBaseSkill.skillCSVStructure.hit_check_radiu / 2f) {
								hittedUnits.Add (unit);
							}
						} else {
							hittedUnits.Add (unit);
						}
					}
				}
//			}
			return hittedUnits;
		}

	}
}

