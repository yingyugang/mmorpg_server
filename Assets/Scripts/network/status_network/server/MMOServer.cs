using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Text;
using UnityEngine.Events;

namespace MMO
{
	public class MMOServer : NetworkManager
	{
		public bool isBattleBegin;
		public UnityAction<DoublePlayerInfo> onRecievePlayerMessage;
		public List<ChatInfo> chatList;
		public List<VoiceInfo> voiceList;
		float mNextFrameTime;
		//key is playerId
		Dictionary<int,PlayerInfo> mPlayerDic;
		//key is connectId
		Dictionary<int,FullPlayerInfo> mFullPlayerDic;
		//key is connectId
		Dictionary<int,int> mConnectDic;
		int mCurrentMaxId = 0;
		//10回per1秒。mmorpg は 30 FrameRate 以内、FPS は 60 FrameRate 以内.
		public const int FRAME_RATE = 30;

		void Awake ()
		{
			this.networkPort = NetConstant.LISTENE_PORT;
			this.StartServer ();
			mPlayerDic = new Dictionary<int, PlayerInfo> ();
			mFullPlayerDic = new Dictionary<int, FullPlayerInfo> ();
			mConnectDic = new Dictionary<int, int> ();
			chatList = new List<ChatInfo> ();
			connectionConfig.SendDelay = 1;
			NetworkServer.RegisterHandler (MsgType.Connect, OnClientConnect);
			NetworkServer.RegisterHandler (MsgType.Disconnect, OnClientDisconnect);
			NetworkServer.RegisterHandler (MessageConstant.CLIENT_TO_SERVER_MSG, OnRecievePlayerMessage);
			NetworkServer.RegisterHandler (MessageConstant.PLAYER_RESPAWN, OnRespawnPlayer);
			NetworkServer.RegisterHandler (MessageConstant.PLAYER_ACTION, OnRecievePlayerAction);
			//主要な部分に区分する
			NetworkServer.RegisterHandler (MessageConstant.PLAYER_CHAT, OnRecieveChat);
			NetworkServer.RegisterHandler (MessageConstant.PLAYER_VOICE, OnRecieveVoice);
			NetworkServer.RegisterHandler (MessageConstant.PLAYER_CONTROLL, OnRecievePlayerControll);
			NetworkServer.RegisterHandler (MessageConstant.PLAYER_REGISTER, OnRecievePlayerRegister);
			NetworkServer.maxDelay = 0;
//			mFrameInterval = 1f / FRAME_RATE;
		}

		void Update(){
			if(voiceList.Count>0){
				SendVoiceData ();
			}
		}

		#region 1.Send (通信の回数が減ったらがいい、どう影響くらいが分からない). 2.考虑到网络原因，尽量直接发送结果，比如显示什么特效，播放什么动作（要检讨）. 3.减少通信的次数，根据玩家位置来同步需要的数据。
		//TODO 把局部发送和全局发送做一个比较。看一下到底性能有什么样子的改变。
		//1.减少数据量 2.提升性能
		//need create Grid and Node.
		public void SynchronizationByPlayer (UnitInfo[] unitInfos, HitInfo[] hitInfos,StatusInfo[] actions)
		{

			Dictionary<IntVector3, List<UnitInfo>> unitInfoByPositionDic = GridTheUnitInfos(unitInfos);

//			Dictionary<IntVector3, List<HitInfo>> hitInfoByPositionDic = GridTheHitInfos (hitInfos);

//			Dictionary<int, List<ActionInfo>> actionByUnitDic = GridTheActions (actions);

			foreach (PlayerInfo playerInfo in mPlayerDic.Values) {
//				TransferByPositionData transferData = new TransferByPositionData ();
				IntVector3 position = playerInfo.unitInfo.transform.position;
				List<UnitInfo> unitInfoList = new List<UnitInfo> ();
//				List<HitInfo> hitInfoList = new List<HitInfo> ();
//				List<ActionInfo> actionList = new List<ActionInfo> ();
				if(unitInfoByPositionDic.ContainsKey(position)){
					unitInfoList.AddRange (unitInfoByPositionDic [position]);
				}
			}
		}

		int mNodeSize = 100;
		//把unit划分区域
		Dictionary<IntVector3, List<UnitInfo>> GridTheUnitInfos (UnitInfo[] unitInfos)
		{
			Dictionary <IntVector3,List<UnitInfo>> unitInfoByPositionDic = new Dictionary<IntVector3, List<UnitInfo>> ();
			for (int i = 0; i < unitInfos.Length; i++) {
				IntVector3 pos = unitInfos [i].transform.position;
				pos = pos / mNodeSize;
				if (!unitInfoByPositionDic.ContainsKey (pos)) {
					unitInfoByPositionDic.Add (pos,new List<UnitInfo>());
				}
				unitInfoByPositionDic [pos].Add (unitInfos[i]);
			}
			return unitInfoByPositionDic;
		}

		Dictionary<IntVector3, List<HitInfo>> GridTheHitInfos(HitInfo[] hitInfos){
			Dictionary<IntVector3,List<HitInfo>> hitInfoByPositionDic = new Dictionary<IntVector3, List<HitInfo>> ();
			for (int i = 0; i < hitInfos.Length; i++) {
				IntVector3 pos = hitInfos [i].hitOriginPosition / mNodeSize;
				if (!hitInfoByPositionDic.ContainsKey (pos)) {
					hitInfoByPositionDic.Add (pos,new List<HitInfo>());
				}
				hitInfoByPositionDic [pos].Add (hitInfos[i]);
			}
			return hitInfoByPositionDic;
		}

		Dictionary<int, List<StatusInfo>> GridTheActions(StatusInfo[] actions){
			Dictionary<int,List<StatusInfo>> actionByUnitDic = new Dictionary<int, List<StatusInfo>> ();
			for (int i = 0; i < actions.Length; i++) {
				if (!actionByUnitDic.ContainsKey (actions [i].casterId)) {
					actionByUnitDic.Add (actions [i].casterId,new List<StatusInfo>());
				}
			}
			return actionByUnitDic;
		}

		public void Synchronization (UnitInfo[] monsters, HitInfo[] hitInfos,StatusInfo[] actions,ShootInfo[] shootInfos)
		{
			TransferData data = new TransferData ();
			data.monsterDatas = monsters;
			data.hitDatas = hitInfos;
			data.actions = actions;
			data.shoots = shootInfos;
			foreach(int key in mConnectDic.Keys){
				NetworkServer.SendToClient (key,MessageConstant.SERVER_TO_CLIENT_MONSTER_INFO, data);
			}
			for (int i = 0; i < monsters.Length; i++) {
				monsters [i].action.actionId = -1;
				monsters [i].action.position = IntVector3.ToIntVector3 (Vector3.zero);
			}
		}
		//TODO only the units that moved need to send.
		public void SendUnitTransforms(List<MMOUnit> units){
			MMOTransform[] transArray =  new MMOTransform[units.Count];
			MMOTransformArray mmoTransforms = new MMOTransformArray ();
			for(int i=0;i<units.Count;i++){
				transArray [i] = units [i].unitInfo.transform;
			}
			mmoTransforms.transforms = transArray;
		}

		public void SendHitInfos(HitInfo[] hitInfos){
		
		}

		public void SendStatusInfos(StatusInfo[] statusInfos){
		
		}

		public void SendShootInfos(ShootInfo[] shootInfos){
		
		}

		void SendToOther(int connectId,short MsgType, MessageBase messageBase){
			for(int i=0;i< NetworkServer.connections.Count;i++){
				NetworkConnection conn = NetworkServer.connections[i];
				if (conn != null && conn.connectionId != connectId) {
					NetworkServer.SendToClient (conn.connectionId, MsgType, messageBase);
				}
			}
		}

		public void SendPlayerStatusToOther(int connId,StatusInfo statusInfo){
			SendToOther (connId,MessageConstant.PLAYER_ACTION,statusInfo);
		}

		public void SendPlayerData ()
		{
			TransferData data = GetTransferData ();
			NetworkServer.SendUnreliableToAll (MessageConstant.SERVER_TO_CLIENT_MSG, data);
		}

		public void SendPlayerRespawn(int unitId){
			SimplePlayerInfo respawnInfo = new SimplePlayerInfo ();
			respawnInfo.playerId = -1;
			respawnInfo.unitId = unitId;
			NetworkServer.SendUnreliableToAll (MessageConstant.PLAYER_RESPAWN, respawnInfo);
		}

		public void SendPlayerEnter(int connId,FullPlayerInfo fullPlayerInfo){
			SendToOther (connId,MessageConstant.PLAYER_ENTER,fullPlayerInfo);
		}

		public void SendPlayerLeave(int playerId,int unitId){
			SimplePlayerInfo playerInfo = new SimplePlayerInfo ();
			playerInfo.playerId = playerId;
			playerInfo.unitId = unitId;
			NetworkServer.SendUnreliableToAll (MessageConstant.PLAYER_LEAVE, playerInfo);
		}

		//TODO update the data by www.
		public void SendVoiceData(){
			VoiceInfos voices = new VoiceInfos ();
			voices.voices = voiceList.ToArray ();
			for(int i=0;i< NetworkServer.connections.Count;i++){
				NetworkConnection conn = NetworkServer.connections[i];
				if(conn!=null)
					NetworkServer.SendToClient (conn.connectionId, MessageConstant.PLAYER_VOICE, voices);
			}
			voiceList.Clear ();
		}

		#endregion

		#region 2.Recieve

		void OnClientConnect (NetworkMessage nm)
		{
			Debug.unityLogger.Log ("OnClientConnect");
			if (nm.channelId == 99)
				return;
			
		}

		void OnClientDisconnect (NetworkMessage nm)
		{
			Debug.unityLogger.Log ("OnClientDisconnect");
			//mConnectDic 中に入ってconnIdが登録したの意味だ。
			if(mConnectDic.ContainsKey(nm.conn.connectionId)){
				int playerId = mConnectDic [nm.conn.connectionId];
				int unitId = MMOBattleServerManager.Instance.RemovePlayer (playerId);
				RemovePlayerInfo (nm.conn.connectionId);
				mConnectDic.Remove (nm.conn.connectionId);
				TransferData data = GetTransferData ();
				SendPlayerLeave (playerId,unitId);
			}
//			NetworkServer.SendUnreliableToAll (MessageConstant.SERVER_TO_CLIENT_MSG, data);
		}

		void RemovePlayerInfo(int connId){
			int playerId = mConnectDic [connId];
			mPlayerDic.Remove (playerId);
			mFullPlayerDic.Remove (connId);
		}

		//收到用户准备准备完毕
		//ユーザーを準備できたメセージを受信する
		void OnRecieveClientReady (NetworkMessage msg)
		{
			Debug.unityLogger.Log ("OnRecieveClientReady");
		}

		//分成三个方法，分别更新transform，animation，attribute
		//TODO need to recieve player pos only .
		void OnRecievePlayerMessage (NetworkMessage msg)
		{
			PlayerInfo playerHandle = msg.ReadMessage<PlayerInfo> ();
			PlayerInfo serverPlayerInfo = mPlayerDic [playerHandle.playerId];
			if (onRecievePlayerMessage != null) {
				DoublePlayerInfo doublePlayerInfo = new DoublePlayerInfo ();
				doublePlayerInfo.clientPlayerInfo = playerHandle;
				doublePlayerInfo.serverPlayerInfo = serverPlayerInfo;
				onRecievePlayerMessage (doublePlayerInfo);
			}
//			playerHandle.chat = "";
		}

		void OnRecievePlayerAction (NetworkMessage msg)
		{
			PlayerInfo playerInfo = GetPlayerInfoByConn (msg.conn.connectionId);
			StatusInfo action = msg.ReadMessage<StatusInfo> ();
			MMOBattleServerManager.Instance.OnRecievePlayerAction (msg.conn.connectionId,playerInfo.playerId, action);
		}

		void OnRespawnPlayer (NetworkMessage msg)
		{
			PlayerInfo playerInfo = GetPlayerInfoByConn (msg.conn.connectionId);
			MMOUnit unit = MMOBattleServerManager.Instance.RespawnPlayer (playerInfo.playerId);
			playerInfo.unitInfo = unit.unitInfo;
		}

		void OnRecieveChat(NetworkMessage msg){
			ChatInfo chatInfo = msg.ReadMessage<ChatInfo> ();
			chatList.Add (chatInfo);
		}

		void OnRecieveVoice(NetworkMessage msg){
			VoiceInfo chatInfo = msg.ReadMessage<VoiceInfo> ();
			this.voiceList.Add (chatInfo);
		}

		void OnRecievePlayerControll(NetworkMessage msg){
			PlayerControllInfo playerControll = msg.ReadMessage<PlayerControllInfo> ();
			PlayerInfo playerInfo = GetPlayerInfoByConn (msg.conn.connectionId);
			playerControll.unitId = playerInfo.unitInfo.attribute.unitId;
			int connectId = msg.conn.connectionId;
			SendToOther (connectId,MessageConstant.PLAYER_CONTROLL,playerControll);
		}

		//1. send this player info to all players.
		//2. send all player infos to this player.
		void OnRecievePlayerRegister(NetworkMessage msg){
			FullPlayerInfo fullPlayerInfo = msg.ReadMessage<FullPlayerInfo> ();
			PlayerInfo playerInfo = new PlayerInfo ();
			playerInfo.playerId = mCurrentMaxId;
			MMOUnit unit = MMOBattleServerManager.Instance.AddPlayer (playerInfo.playerId);
			playerInfo.unitInfo = unit.unitInfo;
			fullPlayerInfo.playerInfo = playerInfo;
			mConnectDic.Add (msg.conn.connectionId, mCurrentMaxId);
			mCurrentMaxId++;
			GameInitInfo gameInitInto = new GameInitInfo ();
			gameInitInto.playerInfo = fullPlayerInfo;
			gameInitInto.playType = (int)MMOBattleServerManager.Instance.playType;
			gameInitInto.otherPlayerInfos = new FullPlayerInfo[mFullPlayerDic.Count];
			int index = 0;
			foreach(FullPlayerInfo fpi in this.mFullPlayerDic.Values){
				gameInitInto.otherPlayerInfos [index] = fpi;
			}
			NetworkServer.SendToClient (msg.conn.connectionId, MessageConstant.PLAYER_INIT_INFO, gameInitInto);
			if (!mPlayerDic.ContainsKey (playerInfo.playerId)) {
				mPlayerDic.Add (playerInfo.playerId, playerInfo);
				mFullPlayerDic.Add (msg.conn.connectionId,fullPlayerInfo);
			}
			SendPlayerEnter (msg.conn.connectionId,fullPlayerInfo);
		}

		TransferData GetTransferData ()
		{
			TransferData data = new TransferData ();
			int i = 0;
			data.playerDatas = new PlayerInfo[mPlayerDic.Count];
			foreach (int id in mPlayerDic.Keys) {
				data.playerDatas [i] = mPlayerDic [id];
				i++;
			}
			return data;
		}

		PlayerInfo GetPlayerInfoByConn (int connId)
		{
			int playerId = mConnectDic [connId];
			if (mPlayerDic.ContainsKey (playerId))
				return mPlayerDic [playerId];
			else
				return null;
		}

		#endregion
	}

	public class DoublePlayerInfo
	{
		public PlayerInfo clientPlayerInfo;
		public PlayerInfo serverPlayerInfo;
	}

}

