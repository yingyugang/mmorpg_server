using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMO;

namespace MultipleBattle
{
	//基于send message的控制机制
	public class BattleController : SingleMonoBehaviour<BattleController>
	{
		public Camera battleCamera;
		public SampleUnit unit;
		BattleClient mBattleClient;

		protected override void Awake ()
		{
			base.Awake ();
			mBattleClient = BattleClient.Instance;
		}

		void Start ()
		{
			if (mBattleClient != null)
				mBattleClient.onFrameUpdate = FrameUpdate;
		}

		void Update ()
		{
			if (Input.GetKeyDown (KeyCode.G)) {
				mBattleClient.SendResourceReadyToServer ();
			}
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				if (Physics.Raycast (battleCamera.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, 1 << LayerConstant.LAYER_GROUND)) {
//					Debug.Log (hit);
//					unit.MoveTo (new Vector2(hit.point.x,hit.point.z));
					SendMessage (hit.point);
				}
			}
		}

		void SendMessage(Vector3 targetPos){
//			unit.MoveTo (new Vector2(targetPos.x,targetPos.z));
			HandleMessage handleMessage = new HandleMessage ();
			handleMessage.targetPos = new Vector2 (Mathf.RoundToInt(targetPos.x * 1000),Mathf.RoundToInt(targetPos.z * 1000));
//			PlayerHandle ph = new PlayerHandle ();
//			ph.mousePos = targetPos;
			mBattleClient.SendPlayerHandle (handleMessage);
		}

		public void FrameUpdate (ServerMessage sm)
		{
			if (sm != null) {
				for (int i = 0; i < sm.handleMessages.Length; i++) {
					unit.MoveTo (sm.handleMessages[i].targetPos);
				}
				sm.playerHandles = null;
			}
			unit.FrameUpdate ();
		}

	}
}