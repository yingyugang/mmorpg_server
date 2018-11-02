using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace MultipleBattle
{

	//TODO:保存所有的帧用于回放
	//TODO:重新同步数据。（1.请求同步，2.服务器获取主机数据，3.同步，4.补帧）
	//TODO:同步对服务器和主机的压力可能会比较大，必要的場合同步时可以暂停游戏，等待数据发送过后再继续进行。
	//TODO:考虑到战斗服务器上可能同时运行上百场战斗，为了不影响其他战斗的进行，可以单独一台服务器作为同步服务器。
	//TODO:丢帧过后的重新请求的策略。
	public class BattleClient : SingleMonoBehaviour<BattleClient>
	{
		public string defaultIP;
		public int defaultPort;
		public bool isBattleBegin;
		public UnityAction<ServerMessage> onFrameUpdate;
		UnityAction<NetworkMessage> onConnect;
		public UnityAction<CreatePlayer> onBattleStart;
		NetworkClient client;
		//一時受信したのフラームを保存される処（临时保存收到的帧）
		Dictionary<int,ServerMessage> mCachedMessages;
		//按序储存的帧,这个的作用在于,当网络出现异常,或者机器出现异常，
		//挤压在这里有多帧的话,可以加快客户端游戏逻辑来追上实际进度。
		Dictionary<int,ServerMessage> mRunableMessages;
		List<ServerMessage> mCachedMessageList;
		float mStartTime;
		bool mTestStop = false;
		int mRecievedFrameCount = 0;
		//当前执行中的关键帧
		ServerMessage mCurrentServerMessage;
		//当前执行的关键帧番号
		int mFrame = 0;
		//可执行的最大帧番号
		int mMaxRunableFrame = 0;
		//接收到到最大帧番号
		int mMaxFrame = 0;
		public const float mMaxFrameWaitingTime = 0.5f;
		int mPhysicFrameRemain = 0;
		float mLastFrameTime;

		void Start ()
		{
			mCachedMessages = new Dictionary<int, ServerMessage> ();
			mRunableMessages = new Dictionary<int, ServerMessage> ();
			mCachedMessageList = new List<ServerMessage> ();
			client = new NetworkClient ();
			client.RegisterHandler (MessageConstant.SERVER_TO_CLIENT_MSG, OnFrameMessage);
			client.RegisterHandler (MessageConstant.SERVER_TO_CLIENT_LIST_MSG, OnFrameMessages);
			client.RegisterHandler (MessageConstant.SERVER_CLIENT_STATUS, OnPlayerStatus);
			client.RegisterHandler (MessageConstant.CLIENT_READY, OnBattleStart);
			client.RegisterHandler (MsgType.Connect, OnConnect);
			client.RegisterHandler (MsgType.NotReady, OnServerNotReady);
			client.RegisterHandler (MsgType.Error, OnError);
			client.RegisterHandler (MsgType.Disconnect, OnDisconnect);
			DontDestroyOnLoad (gameObject);
		}

		public void Reset ()
		{
			mCurrentServerMessage = null;
			mFrame = 0;
			mMaxRunableFrame = 0;
			mMaxFrame = 0;
			mRecievedFrameCount = 0;
			mStartTime = 0;
		}

		//send and recieve;
		//1.connect to server;
		//2.get player status from server;
		//3.send ready to server;
		//4.recieve frame message from server;
		//5.send player handle to server;

		#region 1.Send

		//1.Connect to server.
		//when connected,will call onConnect callback.
		public void Connect (string ip, int port, UnityAction<NetworkMessage> onConnect)
		{
			Debug.Log (string.Format ("{0},{1}", ip, port));
			this.onConnect = onConnect;
			client.Connect (ip, port);
		}

		public void Disconnect ()
		{
			client.Disconnect ();
		}

		//2.Send ready to server
		//when all the player send the message,server will send battle begin message to client,
		//(and client can go to load the really battle scene.)
		public void SendReadyToServer ()
		{
			Debug.unityLogger.Log ("SendReadyToServer");
			ClientMessage cm = new ClientMessage ();
			cm.clientReady = true;
			if (client.isConnected)
				client.Send (MessageConstant.CLIENT_READY, cm);
		}

		//3.after get the first frame message from server.
		//can handle at client and send player handle to server.
		public void SendPlayerHandle (HandleMessage ph)
		{
			if (client.isConnected && isBattleBegin) {
				client.Send (MessageConstant.CLIENT_PLAYER_HANDLE, ph);
			}
		}

		//4.send scene prepared message to server.
		//when all the client's scene prepared ,really battle will begin
		//(server will begin to send to frame message to client).
		public void SendResourceReadyToServer ()
		{
			if (client.isConnected) {
				ClientMessage cm = new ClientMessage ();
				cm.clientReady = true;
				client.Send (MessageConstant.CLIENT_RESOURCE_READY, cm);
			}
		}

		#endregion

		#region 2.Recieve

		void OnConnect (NetworkMessage nm)
		{
			Debug.Log ("<color=green>Connect</color>");
			if (onConnect != null)
				onConnect (nm);
		}

		void OnDisconnect (NetworkMessage nm)
		{
			Debug.Log ("<color=red>Disconnect</color>");
			BattleClientUI.Instance.OnDisconnected ();
//			BattleClientController.Instance.Reset ();
		}

		void OnServerNotReady (NetworkMessage nm)
		{
			Debug.Log ("<color=red>OnServerNotReady</color>");
		}

		void OnError (NetworkMessage nm)
		{
			Debug.Log ("<color=red>OnError</color>");
		}

		//when player connect to server or player send ready to server etc.
		//server will send player status change message to client.
		void OnPlayerStatus (NetworkMessage nm)
		{
			Debug.Log ("<color=green>OnPlayerStatus</color>");
			PlayerStatusArray psa = nm.ReadMessage<PlayerStatusArray> ();
			BattleClientUI.Instance.OnPlayerStatus (psa);
		}

		//when battle started,server will send message to client.
		//client will prepare/load battle scene.
		void OnBattleStart (NetworkMessage nm)
		{
			Debug.Log ("OnBattleStart");
			CreatePlayer cp = nm.ReadMessage<CreatePlayer> ();
			if (onBattleStart != null)
				onBattleStart (cp);
			isBattleBegin = true;
		}

		//服务端30FPS的发送频率。客户端60FPS的执行频率。
		//也就是说服务器1帧客户端2帧
		void FixedUpdate ()
		{
			#region 模拟网络阻塞
			if (Input.GetKeyDown (KeyCode.N)) {
				mTestStop = true;
			}
			if (Input.GetKeyDown (KeyCode.M)) {
				mTestStop = false;
			}
			if (mTestStop) {
				return;
			}
			#endregion
			if (!isBattleBegin)
				return;
			//論理が実行する(执行逻辑)
			if (mPhysicFrameRemain <= 0) {
				UpdateFrame ();
			}
			//物理フレームを実行する(执行物理帧)
			mPhysicFrameRemain--;
			UpdateFixedFrame ();
		}

		void UpdateFrame ()
		{
			if (mRunableMessages.ContainsKey (mFrame)) {
				mCurrentServerMessage = mRunableMessages [mFrame];
				mRunableMessages.Remove (mFrame);
				mFrame++;
				mPhysicFrameRemain = 3;
				Time.timeScale = 1;
			} else {
				Time.timeScale = 0;
			}
		}

		void UpdateFixedFrame ()
		{
//			BattleClientController.Instance.UpdateFixedFrame ();
			if (onFrameUpdate != null)
				onFrameUpdate (mCurrentServerMessage);
		}

		void OnFrameMessage (NetworkMessage mb)
		{
			if (mStartTime == 0)
				mStartTime = Time.realtimeSinceStartup;
			mRecievedFrameCount++;
			ServerMessage sm = mb.ReadMessage<ServerMessage> ();
			AddServerMessage (sm);
			while (mCachedMessages.Count > 0 && mCachedMessages.ContainsKey (mMaxRunableFrame)) {
				mRunableMessages.Add (mMaxRunableFrame, mCachedMessages [mMaxRunableFrame]);
				mCachedMessages.Remove (mMaxRunableFrame);
				mMaxRunableFrame++;
				Time.timeScale = 1;
			}
			Debug.Log (mb + "||" + mCachedMessages.Count);
		}

		void OnFrameMessages (NetworkMessage mb)
		{
			if (mStartTime == 0)
				mStartTime = Time.realtimeSinceStartup;
			mRecievedFrameCount++;
			Debug.Log (((mRecievedFrameCount - 1) / (Time.realtimeSinceStartup - mStartTime)).ToString ());
			CachedServerMessage cachedServerMessage = mb.ReadMessage<CachedServerMessage> ();
			for (int i = 0; i < cachedServerMessage.serverMessages.Length; i++) {
				AddServerMessage (cachedServerMessage.serverMessages [i]);
			}
		}

		public void AddServerMessage (ServerMessage tm)
		{
			//丢弃重复帧（防止受到数据被截获后的重复发送攻击）
			if (!mCachedMessages.ContainsKey (tm.frame)) {
				mCachedMessages.Add (tm.frame, tm);
				if (mMaxFrame < tm.frame) {
					mMaxFrame = tm.frame;
				}
				//TODO 应该分段处理，否则的话这个会比较长
				//Ordered list
				for (int i = mCachedMessageList.Count - 1; i > 0; i--) {
					if (mCachedMessageList [i].frame < tm.frame) {
						mCachedMessageList.Insert (i, tm);
						break;
					}
				}
			}
		}

		public int Frame {
			get { 
				return mFrame;
			}
		}

		#endregion
	}

	[Serializable]
	public class RecordMessage
	{
		public int[] playerIds = new int[0];
		public List<ServerMessage> records = new List<ServerMessage> ();
	}


	[System.Serializable]
	public class PlayerKeys
	{
		public bool KeyA;
		public bool KeyS;
		public bool KeyD;
		public bool KeyW;
	}

}
