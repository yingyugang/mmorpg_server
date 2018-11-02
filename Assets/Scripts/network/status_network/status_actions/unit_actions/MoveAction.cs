using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MMO
{
	public class MoveAction : UnitBaseAction
	{

		Transform mTrans;
		NavMeshAgent mAgent;
		float mStopSqrDistance;
		Vector3 mTargetPos;
		//TODO move to target only.
		float mBackToDefaultTime;

		public override void OnAwake ()
		{
			base.OnAwake ();
			mTrans = GO.transform;
			mAgent = GO.GetComponent<NavMeshAgent> ();
			NavMeshHit hit;
			if(NavMesh.SamplePosition(mTrans.position, out hit, Mathf.Infinity, NavMesh.AllAreas)){
				mTrans.position = hit.position;
			}
		}

		public override void OnEnter ()
		{
			base.OnEnter ();
			mAgent.isStopped = false;
			mBackToDefaultTime = Time.time + BattleConst.BACK_TO_DEFAULT_DELAY;
			mAgent.speed = mmoUnit.speed;
			if (mmoUnit.isFollowTarget) {
				mAgent.SetDestination (mmoUnit.target.position);
				mTargetPos = mmoUnit.target.position;
			} else {
				mAgent.SetDestination (mmoUnit.moveTargetPos);
				mTargetPos = mmoUnit.moveTargetPos;
			}
			//TODO
			mTrans.LookAt (mTargetPos);
			mAgent.velocity = mTrans.forward * mAgent.speed;
//			mAgent.stoppingDistance = mmoUnit.stopDistance;
			mStopSqrDistance = mmoUnit.stopDistance * mmoUnit.stopDistance;
			SendAction ();
		}

		protected override void SendAction ()
		{
			base.SendAction ();
			StatusInfo action = new StatusInfo ();
			action.casterId = mmoUnit.unitInfo.attribute.unitId;
			action.status = BattleConst.UnitMachineStatus.MOVE;
			MMOBattleServerManager.Instance.AddAction (action);
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			if (mAgent != null) {
				if(mmoUnit.isFollowTarget && (mBackToDefaultTime < Time.time || mmoUnit.target == null || mmoUnit.target.GetComponent<MMOUnit>().IsDeath)){
					//Back to default pos;
					mmoUnit.isFollowTarget = false;
					mmoUnit.target = null;
					statusMachine.ChangeStatus (BattleConst.UnitMachineStatus.STANDBY);
					//this logic should in STANDBY.
//					mTargetPos = mmoUnit.defaultPos;
//					mmoUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.STANDBY;
//					mmoUnit.SetZeroStopDistance ();
//					mStopSqrDistance = mmoUnit.stopDistance * mmoUnit.stopDistance;
//					mAgent.SetDestination(mTargetPos);
//					mAgent.speed = mmoUnit.speed * 2;
					return;
				}

				if (mmoUnit.isFollowTarget && mmoUnit.target!=null) {
					mAgent.SetDestination (mmoUnit.target.position);
				} 
//				float distance;
//				if(mmoUnit.isFollowTarget)
//					distance = Vector3.SqrMagnitude (mmoUnit.moveTarget.position - mTrans.position);
//				else
//					distance = Vector3.SqrMagnitude (mTargetPos - mTrans.position);

				if (mAgent.remainingDistance <=  mAgent.stoppingDistance) {
					statusMachine.ChangeStatus (mmoUnit.onMoveDoneStatus);
					return;
				}
			}
		}

		public override void OnExit ()
		{
			base.OnExit ();
			if (mAgent != null) {
				mAgent.isStopped = true;
			}
		}

	}
}
