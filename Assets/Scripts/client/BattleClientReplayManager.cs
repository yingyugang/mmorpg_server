using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleBattle
{
	public class BattleClientReplayManager : SingleMonoBehaviour<BattleClientReplayManager>
	{
		public const string BATTLE_RECORD = "BATTLE_RECORD";
		public RecordMessage record;
		public Dictionary<int,ServerMessage> handles;
		public bool isBegin;
		int mFrame;

		protected override void Awake ()
		{
			base.Awake ();
			isBegin = false;
			mFrame = 0;
			mFrameInterval = 1f / mFrameRate;
		}

		public void SaveRecord ()
		{
			if (record.records.Count == 0) {
				return;
			}
			string str = JsonUtility.ToJson (record);
			Debug.Log (str);
			PlayerPrefs.SetString (BATTLE_RECORD, str);
			PlayerPrefs.Save ();
		}

		public void Replay ()
		{
			isBegin = true;
			mNextFrameTime = Time.realtimeSinceStartup;
			mFrame = 0;
			handles = new Dictionary<int, ServerMessage> ();
			string recordStr = PlayerPrefs.GetString (BATTLE_RECORD);
			Debug.Log (recordStr);
			record = JsonUtility.FromJson<RecordMessage> (recordStr);
			for (int i = 0; i < record.records.Count; i++) {
				handles.Add (record.records [i].frame, record.records [i]);
			}
			BattleClientController.Instance.Reset ();
			CreatePlayer cp = new CreatePlayer ();
			cp.playerIds = record.playerIds;
			BattleClientController.Instance.CreatePlayers (cp);
		}

		void Update ()
		{
			if (isBegin) {
				SendFrame ();
			}
		}

		int mFrameRate = 30;
		float mFrameInterval;
		float mNextFrameTime;
		void SendFrame(){
			while(mNextFrameTime <= Time.realtimeSinceStartup){
				mNextFrameTime += mFrameInterval;
				SendFrameMessage ();
			}
		}

		void SendFrameMessage (){
			ServerMessage sm = new ServerMessage ();
			sm.frame = mFrame;
			sm.playerHandles = new PlayerHandle[0];
			if (handles.ContainsKey (mFrame)) {
				sm = handles [mFrame];
			}
			BattleClient.Instance.AddServerMessage (sm);
			mFrame++;
		}
	}
}
