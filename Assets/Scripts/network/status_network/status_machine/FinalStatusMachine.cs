using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	//策略模式だけ
	public class FinalStatusMachine : MonoBehaviour
	{
		int mPreStatus;
		int mCurrentStatus;
	 	public List<int> statusList;
		List<StatusAction> mCurrentStatusActions;

		Dictionary<int,List<StatusAction>> mStatusDic;
		Dictionary<int,List<StatusAction>> StatusDic {
			get { 
				if (mStatusDic == null) {
					mStatusDic = new Dictionary<int, List<StatusAction>> ();
					statusList = new List<int> ();
				}
				return mStatusDic;
			}
		}

		public void Reset(){
			mPreStatus = 0;
			mCurrentStatus = 0;
			statusList = new List<int> ();
			mCurrentStatusActions = new List<StatusAction> ();
			mStatusDic = new Dictionary<int, List<StatusAction>> ();
		}

		public void AddStatus (int statusName)
		{
			if (StatusDic.ContainsKey (statusName))
				return;
			StatusDic.Add (statusName, new List<StatusAction> ());
			statusList.Add (statusName);
		}

		public void AddAction (int statusName, StatusAction action)
		{
			if (action == null)
				return;
			AddStatus (statusName);
			StatusDic [statusName].Add (action);
			action.GO = gameObject;
			action.statusMachine = this;
			action.OnAwake ();
		}

		public void AddActions (int statusId, List<StatusAction> actions)
		{
			if (actions == null)
				return;
			AddStatus (statusId);
			for (int i = 0; i < actions.Count; i++) {
				AddAction (statusId, actions [i]);
			}
		}

		public void ChangeStatus (int status)
		{
			//status あるし、そして今のstatusはparameter-statusじゃない
			if (StatusDic.ContainsKey (status) && mCurrentStatus != status) {
				if(MMOBattleServerManager.Instance.debug)
					Debug.Log (string.Format("Time:{0},From:{1},To:{2}",Time.time,mCurrentStatus,status));
				mPreStatus = mCurrentStatus;
				mCurrentStatus = status;
				mCurrentStatusActions = StatusDic [mCurrentStatus];
				//OnExit
				if (StatusDic.ContainsKey (mPreStatus)) {
					List<StatusAction> exitActions = StatusDic [mPreStatus];
					for (int i = 0; i < exitActions.Count; i++) {
						if(exitActions [i].IsEnable)
							exitActions [i].OnExit ();
					}
				}
				//OnEnter
				if ( StatusDic.ContainsKey (mCurrentStatus)) {
					List<StatusAction> enterActions = StatusDic [mCurrentStatus];
					for (int i = 0; i < enterActions.Count; i++) {
						if(enterActions [i].IsEnable)
							enterActions [i].OnEnter ();
					}
				}
			}
		}

		public int GetPreStatus ()
		{
			return mPreStatus;
		}

		public int CurrentStatus
		{
			get{ 
				return mCurrentStatus;
			}
		}

		 void Update ()
		{
			if (mCurrentStatusActions != null) {
				for (int i = 0; i < mCurrentStatusActions.Count; i++) {
					if (!mCurrentStatusActions [i].IsEnable && mCurrentStatusActions [i].IsExcute) {
						mCurrentStatusActions [i].OnExit ();
					}
					if(mCurrentStatusActions [i].IsEnable)
						mCurrentStatusActions [i].OnUpdate ();
				}
			}
		}
	}
}