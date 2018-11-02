using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleBattle
{
	public static class NetConstant
	{

		public static int listene_port = 8080;
		public static int max_player_count = 2;

		public const int FRAME_RATE = 30;
		public const string MAX_PLAYER_CONSTANT = "MAX_PLAYER";

		public static int MaxNum {

			get {
				int max = 1;
				if (PlayerPrefs.HasKey (MAX_PLAYER_CONSTANT)) {
					return Mathf.Min (max, PlayerPrefs.GetInt (MAX_PLAYER_CONSTANT));
				}
				return max;
			}
			set {

				PlayerPrefs.SetInt (MAX_PLAYER_CONSTANT, value);
				PlayerPrefs.Save ();
				Debug.Log (PlayerPrefs.GetInt (MAX_PLAYER_CONSTANT));
			}
		}
	}
}