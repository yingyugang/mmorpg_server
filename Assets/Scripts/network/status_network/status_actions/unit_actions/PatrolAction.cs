using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MMO
{
	public class PatrolAction : UnitBaseAction
	{
		Transform mTrans;
		public List<Vector3> wayPoints;
		int mCurrentWayPointIndex = 0;
		float stepLength = 15f;
		const float PATROL_INTERVAL = 2;
		float mNextPatrolTime = 0;

		public override void OnAwake ()
		{
			base.OnAwake ();
			mTrans = mmoUnit.transform;
			InitLoopPositions ();
		}

		public override void OnEnter ()
		{
			base.OnEnter ();
			mNextPatrolTime = Time.time + PATROL_INTERVAL;
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			if(mNextPatrolTime<Time.time){
				AIMove ();
				mNextPatrolTime = Time.time + PATROL_INTERVAL;
			}
		}

		void InitLoopPositions ()
		{
			wayPoints = new List<Vector3> ();
			Vector3 startPos = mTrans.position;
			wayPoints.Add (GetCloesNavMeshPosition( startPos + new Vector3 (0, 0, -stepLength)));
			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (stepLength * Mathf.Cos (60 / 180f), 0, -stepLength * .5f)));
			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (stepLength * Mathf.Cos (60 / 180f), 0, stepLength * .5f)));
			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (0, 0, stepLength)));
			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (-stepLength * Mathf.Cos (60 / 180f), 0, stepLength * .5f)));
			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (-stepLength * Mathf.Cos (60 / 180f), 0, -stepLength * .5f)));
		}

		Vector3 GetCloesNavMeshPosition(Vector3 pos){
			NavMeshHit hit;
			if(NavMesh.SamplePosition(pos, out hit, Mathf.Infinity, NavMesh.AllAreas)){
				return hit.position;
			}
			return pos;
		}

		void AIMove ()
		{
			mmoUnit.moveTargetPos = GetNextMoveTargetPos ();
			mmoUnit.isFollowTarget = false;
//			mmoUnit.speed = BattleConst.UnitSpeed.NORMAL_PATROL;
			mmoUnit.moveClip = AnimationConstant.UNIT_ANIMATION_CLIP_PATROL;
			mmoUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.STANDBY;
			mmoUnit.SetDefaultStopDistance ();
			mmoUnit.fsm.ChangeStatus (BattleConst.UnitMachineStatus.MOVE_PATROL);
		}

		Vector3 GetNextMoveTargetPos ()
		{
			mCurrentWayPointIndex++;
			mCurrentWayPointIndex = mCurrentWayPointIndex % wayPoints.Count;
			Vector3 pos = wayPoints [mCurrentWayPointIndex];
			return pos;
		}

	}
}
