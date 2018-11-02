using System;
using UnityEngine;

namespace MMO
{
	public class BackToDefaultAction: UnitBaseAction
	{

		const float CHECK_INTERVAL = 3;
		float mNextCheckTime;

		public override void OnAwake ()
		{
			base.OnAwake ();
		}

		public override void OnEnter ()
		{
			base.OnEnter ();
			mNextCheckTime = Time.time + CHECK_INTERVAL;
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			if(mNextCheckTime < Time.time){
				mNextCheckTime = Time.time + CHECK_INTERVAL;
				CheckBackToDefault ();
			}
		}

		void CheckBackToDefault(){
			if(Vector3.Distance(mmoUnit.transform.position,mmoUnit.defaultPos) > BattleConst.MIN_STOP_DISTANCE){
				mmoUnit.isFollowTarget = false;
				mmoUnit.moveTargetPos = mmoUnit.defaultPos;
				mmoUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.STANDBY;
				mmoUnit.SetStopDistance (BattleConst.MIN_STOP_DISTANCE);
				statusMachine.ChangeStatus (BattleConst.UnitMachineStatus.MOVE);
			}
		}
	}
}

