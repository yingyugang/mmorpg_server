using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace MMO
{
	public class CastAction : UnitBaseAction
	{
		NavMeshAgent mAgent;
		Transform mTrans;
		float mStopSqrDistance;
		BaseSkill mBaseSkill;

		public override void OnAwake ()
		{
			base.OnAwake ();
			mTrans = GO.transform;
			mAgent = GO.GetComponent<NavMeshAgent> ();
		}

		public override void OnEnter ()
		{
			base.OnEnter ();
			//TODO this should be not in cast action.
			if (mAgent != null) {
				mAgent.stoppingDistance = mmoUnit.stopDistance;
				mStopSqrDistance = mAgent.stoppingDistance * mAgent.stoppingDistance;
			}
			//TODO to set the current skill by logic.
			if(mmoUnit.mmoUnitSkill.currentSkill == null){
				mmoUnit.mmoUnitSkill.currentSkill = mmoUnit.mmoUnitSkill.controllableSkills [0];
			}
			mBaseSkill = mmoUnit.mmoUnitSkill.currentSkill;
			mBaseSkill.OnEnter ();
			//考虑到网络延迟和主控端玩家体验的问题，这里可能会又玩家电脑自己完成，而不是发送消息。
			//但是目前的做法更通用。
			SendAction ();
		}

		protected override void SendAction ()
		{
			base.SendAction ();
			//to tell client skill start,main skill only.
			StatusInfo action = new StatusInfo ();
			action.casterId = mmoUnit.unitInfo.attribute.unitId;
			action.status = BattleConst.UnitMachineStatus.CAST;
			action.actionId = mBaseSkill.unitSkillCSVStructure.id;
			if (this.mmoUnit.target != null && this.mmoUnit.target.GetComponent<MMOUnit> () != null) {
				action.targetId = this.mmoUnit.target.GetComponent<MMOUnit> ().unitInfo.attribute.unitId;
			}
			MMOBattleServerManager.Instance.AddAction (action);
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			mBaseSkill.OnUpdate ();
			//AI
			if (!mmoUnit.IsPlayer) {
				//LookAt the move target.
				if (mmoUnit.target != null) {
					Vector3 dirct = mmoUnit.target.position - mmoUnit.transform.position;
					dirct = new Vector3 (dirct.x, 0, dirct.z).normalized;
					if (dirct != Vector3.zero)
						mmoUnit.transform.forward = dirct;
				}
				if (mBaseSkill.IsEnd) {
					if (!mBaseSkill.IsInCooldown && mmoUnit.mmoUnitSkill.isLoopCurrentSkill) {
						mBaseSkill.OnEnter ();
					} else {
						//TODO to get useable skill or back to idle.
						if (mmoUnit.target != null && Vector3.SqrMagnitude (mmoUnit.target.position - mTrans.position) > mStopSqrDistance * 1.1f) {
							statusMachine.ChangeStatus (BattleConst.UnitMachineStatus.MOVE);
							mmoUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.CAST;
						}else if (mmoUnit.target == null || mmoUnit.target.GetComponent<MMOUnit> ().IsDeath) {
							mmoUnit.target = null;
							mmoUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.STANDBY;
							statusMachine.ChangeStatus (BattleConst.UnitMachineStatus.STANDBY);
						} else {
							statusMachine.ChangeStatus (BattleConst.UnitMachineStatus.STANDBY);
						}
					}
				}
			}
		}
	}
}