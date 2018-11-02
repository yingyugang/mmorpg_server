using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MMO
{
	public class SimpleAI : MonoBehaviour
	{

		MMOUnit mMMOUnit;
//		public List<Vector3> wayPoints;
		int mCurrentWayPointIndex = 0;
		Transform mTrans;
		float stepLength = 15f;

		void Awake ()
		{
			mMMOUnit = GetComponent<MMOUnit> ();
			mTrans = transform;
		}

		void Start ()
		{
//			InitLoopPositions ();
			mMMOUnit.fsm.AddAction (BattleConst.UnitMachineStatus.STANDBY, new PatrolAction ());
			mMMOUnit.fsm.AddAction (BattleConst.UnitMachineStatus.MOVE_PATROL, new MoveAction ());
			mMMOUnit.fsm.AddAction (BattleConst.UnitMachineStatus.MOVE_PATROL, new ScanAction ());
		}

//		int mPreStatus = -1;
//		float mNextAIMoveTime;
//		const float MIN_IDLE = 3f;
//		 void Update ()
//		{
//			if(mMMOUnit.fsm.CurrentStatus == BattleConst.UnitMachineStatus.STANDBY && mPreStatus!=BattleConst.UnitMachineStatus.STANDBY){
//				//come into standby;
//				mNextAIMoveTime = Time.time + MIN_IDLE;
//			}
//			if (mMMOUnit.fsm.CurrentStatus == BattleConst.UnitMachineStatus.STANDBY && mNextAIMoveTime < Time.time) {
//				AIMove ();
//			}
//			mPreStatus = mMMOUnit.fsm.CurrentStatus;
//		}

//		void InitLoopPositions ()
//		{
//			wayPoints = new List<Vector3> ();
//			Vector3 startPos = mTrans.position;
//			wayPoints.Add (GetCloesNavMeshPosition( startPos + new Vector3 (0, 0, -stepLength)));
//			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (stepLength * Mathf.Cos (60 / 180f), 0, -stepLength * .5f)));
//			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (stepLength * Mathf.Cos (60 / 180f), 0, stepLength * .5f)));
//			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (0, 0, stepLength)));
//			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (-stepLength * Mathf.Cos (60 / 180f), 0, stepLength * .5f)));
//			wayPoints.Add (GetCloesNavMeshPosition(startPos + new Vector3 (-stepLength * Mathf.Cos (60 / 180f), 0, -stepLength * .5f)));
//		}
//
//		Vector3 GetCloesNavMeshPosition(Vector3 pos){
//			NavMeshHit hit;
//			if(NavMesh.SamplePosition(pos, out hit, Mathf.Infinity, NavMesh.AllAreas)){
//				return hit.position;
//			}
//			return pos;
//		}
//
//		void AIMove ()
//		{
//			mMMOUnit.moveTargetPos = GetNextMoveTargetPos ();
//			mMMOUnit.isFollowTarget = false;
//			mMMOUnit.speed = BattleConst.UnitSpeed.NORMAL_PATROL;
//			mMMOUnit.moveClip = AnimationConstant.UNIT_ANIMATION_CLIP_PATROL;
//			mMMOUnit.onMoveDoneStatus = BattleConst.UnitMachineStatus.STANDBY;
//			mMMOUnit.SetZeroStopDistance ();
//			mMMOUnit.fsm.ChangeStatus (BattleConst.UnitMachineStatus.MOVE_PATROL);
//		}
//
//		Vector3 GetNextMoveTargetPos ()
//		{
//			mCurrentWayPointIndex++;
//			mCurrentWayPointIndex = mCurrentWayPointIndex % wayPoints.Count;
//			Vector3 pos = wayPoints [mCurrentWayPointIndex];
//			return pos;
//		}

	}
}
