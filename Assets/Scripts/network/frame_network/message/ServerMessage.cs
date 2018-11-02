using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace MultipleBattle
{

	public class CreatePlayer : MessageBase{
		public int[] playerIds;
	}

	[Serializable]
	public class CachedServerMessage : MessageBase{
		public ServerMessage[] serverMessages;
	}

	//通信を簡単になるために、制御するメッセージがstringで使って。
	[Serializable]
	public class HandleMessage : MessageBase{
		public int playerId;
		public string handle;
		public Vector2 targetPos;
	}

	[Serializable]
	public class ServerMessage : MessageBase
	{
		public int frame = 1;
		public string desc = "";
		[SerializeField]
		public PlayerHandle[] playerHandles;//Old
		[SerializeField]
		public HandleMessage[] handleMessages;//New
	}

	[Serializable]
	public class SpawnGameObject : MessageBase
	{
		public int id;
		public int objType;
		public int actionType;
		public Vector3 pos;
		public Quaternion qua;
	}

	//key: 0=A,1=S,2=D,3=W,4=J,
	//keyStatus: false=up,true=down;
	[Serializable]
	public class PlayerHandle : MessageBase
	{
		public int playerId;//发送的时候服务端会根据connectionId设置playerId，所以Send的时候不用设置。
		public Vector2 mousePos;
	}

	public class PlayerStatusArray:MessageBase {
		public PlayerStatus[] playerStatus;
	}

	public class PlayerStatus : MessageBase{
		public int playerId;
		public bool isReady;
		public bool isSceneReady;
	}

}