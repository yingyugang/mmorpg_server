using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleBattle
{
	public class MessageConstant
	{
		public const short SERVER_TO_CLIENT_MSG = 8001;
		public const short SERVER_CLIENT_STATUS = 8002;
		public const short SERVER_TO_CLIENT_LIST_MSG = 8003;
		public const short SERVER_TO_CLIENT_START = 8004;

		public const short CLIENT_TO_SERVER_MSG = 9001;
		public const short CLIENT_READY = 9002;
		public const short CLIENT_PLAYER_HANDLE = 9003;
		public const short CLIENT_REQUEST_FRAMES = 9004;
		public const short CLIENT_RESOURCE_READY = 9005;
	}
}
