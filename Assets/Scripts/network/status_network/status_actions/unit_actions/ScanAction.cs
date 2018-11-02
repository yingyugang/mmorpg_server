using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	//一部AIがある。
	public class ScanAction : UnitBaseAction
	{
		float mNextScanInterval = 0.5f;

		public override void OnAwake ()
		{
			base.OnAwake ();
		}

		public override void OnEnter ()
		{
			base.OnEnter ();
			mmoUnit.nextScanTime = Time.time + mNextScanInterval;
			//Reset the stop distance to default skill range.
//			mmoUnit.stopDistance = BattleConst.MIN_SKILL_RANGE;
		}

		public override void OnUpdate ()
		{
			if (mmoUnit.nextScanTime > Time.time) {
				return;
			}
			MMOUnit targetUnit = null;
			//TODO 需要对技能优先级进行排序，并检测。
			mmoUnit.nextScanTime = Time.time + mNextScanInterval;
			//二つ目スキルからチェックする。检测是否有主动技能可以释放。
			for(int i = 1;i < mmoUnit.mmoUnitSkill.controllableSkills.Count;i++){
				if (!mmoUnit.mmoUnitSkill.controllableSkills [i].IsInCooldown) {
					BaseSkill baseSkill = mmoUnit.mmoUnitSkill.controllableSkills[i];
					List<MMOUnit> mmoUnits = baseSkill.GetSkillableUnits ();
					if(mmoUnits.Count > 0){
						mmoUnit.mmoUnitSkill.currentSkill = baseSkill;
						targetUnit = mmoUnits [0];
						break;
					}
				}
			}

			if(targetUnit==null){
				if (mmoUnit.target != null && mmoUnit.mmoUnitSkill.currentSkill!=null && !mmoUnit.mmoUnitSkill.currentSkill.IsInCooldown &&  mmoUnit.mmoUnitSkill.currentSkill.IsSkillable(mmoUnit.target.GetComponent<MMOUnit>())) {
					targetUnit = mmoUnit.target.GetComponent<MMOUnit> ();
				} else {
					//一つ目スキルをチェックする。
					targetUnit = null;
					BaseSkill baseSkill = mmoUnit.mmoUnitSkill.controllableSkills [0];
					List<MMOUnit> mmoUnits = baseSkill.GetSkillableUnits ();
					if (mmoUnits.Count > 0) {
						mmoUnit.mmoUnitSkill.currentSkill = baseSkill;
						if (mmoUnit.target != null && mmoUnits.Contains (mmoUnit.target.GetComponent<MMOUnit> ())) {
							targetUnit = mmoUnit.target.GetComponent<MMOUnit> ();
						} else {
							targetUnit = mmoUnits [0];
						}
					}
				}
			}
			if (targetUnit != null) {
				OnFindTarget (targetUnit);
			}
		}

		void OnFindTarget (MMOUnit targetUnit)
		{
			//TODO 这个逻辑有问题，不能用这个属性来判断是否用枪械，应该把枪械放到isremote里去。
			float stopDistance = mmoUnit.mmoUnitSkill.currentSkill.skillCSVStructure.range;
			if (mmoUnit.mmoUnitSkill.currentSkill.skillCSVStructure.hit_check_type != 1 && mmoUnit.mmoUnitSkill.currentSkill.skillCSVStructure.skillShoot == null) {
				stopDistance = Mathf.Min (mmoUnit.mmoUnitSkill.currentSkill.skillCSVStructure.hit_range * 0.9f, mmoUnit.mmoUnitSkill.currentSkill.skillCSVStructure.range);
			} 
			mmoUnit.SetStopDistance (stopDistance + mmoUnit.unitCSVStructure.width);
			this.mmoUnit.isFollowTarget = true;
			this.mmoUnit.moveClip = AnimationConstant.UNIT_ANIMATION_CLIP_RUN;
			this.mmoUnit.target = targetUnit.transform;
			if (CheckTargetCastable (targetUnit.transform)) {
				this.mmoUnit.fsm.ChangeStatus (BattleConst.UnitMachineStatus.CAST);
			} else {
				this.mmoUnit.fsm.ChangeStatus (BattleConst.UnitMachineStatus.MOVE);
				this.mmoUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.CAST;
			}
		}

		bool CheckTargetCastable(Transform target){
			float sqrRange = mmoUnit.stopDistance * mmoUnit.stopDistance;
			float distance = (mmoUnit.transform.position - target.position).sqrMagnitude;
			return sqrRange > distance;
		}

		//TODO check has the skill and has the skill's target.
		BaseSkill GetNextSkill ()
		{
			if (mmoUnit.mmoUnitSkill.controllableSkills.Count == 0) {
				return null;
			}
			//第二個スキルからチェックする。
			for (int i = 1; i < mmoUnit.mmoUnitSkill.controllableSkills.Count; i++) {
				if (!mmoUnit.mmoUnitSkill.controllableSkills [i].IsInCooldown) {
					return mmoUnit.mmoUnitSkill.controllableSkills [i];
				}
			}
			//TODO NULLの場合で特に設定するロジックがいなければいけない。
			return mmoUnit.mmoUnitSkill.defaultSkill;
		}

		public override void OnExit ()
		{
			base.OnExit ();
		}
	}
}