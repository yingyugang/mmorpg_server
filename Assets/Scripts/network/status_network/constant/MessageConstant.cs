using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public static class MessageConstant
	{
		public const short SERVER_TO_CLIENT_MSG = 8001;
		public const short PLAYER_INIT_INFO = 8002;
		public const short SERVER_TO_CLIENT_MONSTER_INFO = 8003;
		public const short SERVER_TO_CLIENT_SKILL = 8004;
		public const short SERVER_TO_CLIENT_HIT_INFO = 8005;
		public const short SERVER_TO_CLIENT_ACTION_INFO = 8006;
		public const short SERVER_TO_CLIENT_TRANSFORM_INFO = 8007;

		public const short UNIT_TRANSFORMS = 8010;
		public const short UNIT_ACTIONS = 8011;
		public const short UNIT_HITS = 8012;
		public const short UNIT_SHOOTS = 8013;

		public const short PLAYER_LEAVE = 8008;
		public const short PLAYER_ENTER = 8009;

		public const short CLIENT_TO_SERVER_MSG = 9001;
		public const short CLIENT_TO_SERVER_PLAYER_INFO = 9002;
		public const short PLAYER_RESPAWN = 9003;
		public const short PLAYER_REGISTER = 9004;

		public const short PLAYER_ACTION = 10001;
		public const short PLAYER_CHAT = 10002;
		public const short PLAYER_VOICE = 10003;

		public const short PLAYER_SHOOT = 10004;
		public const short PLAYER_CONTROLL = 10005;
	}
}
