using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.EventSystems;

namespace MultipleBattle
{
	//Debug用Text.
	public class BattleServerUI : SingleMonoBehaviour<BattleServerUI>
	{
		public BattleServer battleServer;
		public Text txt_ip;
		public Text txt_port;
		public Text txt_maxPlayer;
		public Button btn_reset;

		public bool isDebug;
		public Text txt_debug;
		public Scrollbar scroll_debug;

		protected override void Awake ()
		{
			base.Awake ();
			if(battleServer==null){
				battleServer = GetComponent<BattleServer> ();
			}
			if(txt_port!=null)
				txt_port.text = " Port:" + battleServer.networkPort.ToString ();
			if(txt_ip!=null)
				txt_ip.text =" IP:" + Network.player.ipAddress;
			if (btn_reset != null) {
				btn_reset.onClick.AddListener (()=>{
					battleServer.Reset();
				});
			}
			SetPlayer ();
			mStringBuilder = new StringBuilder ();
			mCachedLines = new List<CachedLine> ();
			Application.logMessageReceived += HandleLog;
		}

		int mCurrentConnetCount;
		void SetPlayer(){
			txt_maxPlayer.text = string.Format (" Player: {0}/{1}",battleServer.ConnectionCount,NetConstant.max_player_count);
		}

		void Update(){
			if(mCurrentConnetCount != NetworkServer.connections.Count){
				mCurrentConnetCount = NetworkServer.connections.Count;
				SetPlayer ();
			}
		}

		List<CachedLine> mCachedLines;
		StringBuilder mStringBuilder;
		int mMaxStringLength = 100000;
		public string Message{
			get{ 
				return mStringBuilder.ToString ();
			}
		}

		struct CachedLine{
			public int startIndex;
			public int count;
		}

		void AddDebugMessage(string msg){
			CachedLine newLine = new CachedLine ();
			newLine.startIndex = mStringBuilder.Length;
			newLine.count = msg.Length + 1;
			mCachedLines.Add (newLine);
			mStringBuilder.AppendLine (msg);
			if(mStringBuilder.Length > mMaxStringLength){
				mStringBuilder.Remove (0,mStringBuilder.Length - mMaxStringLength / 2);
			}
			if (isDebug) {
				txt_debug.text = Message;
				scroll_debug.value = 0;
			}
		}

		void OnDisable () {
			Application.logMessageReceived -= HandleLog;
		}

		void HandleLog(string logString, string stackTrace, LogType type){
			AddDebugMessage (logString);
		}
	}
}
