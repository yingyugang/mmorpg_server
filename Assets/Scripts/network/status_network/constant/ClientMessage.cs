using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MultipleBattle
{
	public class ClientMessage : MessageBase
	{
		public int frame;
		public bool clientReady;
	}

	public class LostFrameIdsMessage : MessageBase
	{
		public LostFrameIdAToBMessage[] frameAToB;
	}

	//针对丢失连续的帧，为了减少发送数据只发送开始和结束帧
	public class LostFrameIdAToBMessage : MessageBase{
		public int fromFrame;
		public int toFrame;
	}


}
