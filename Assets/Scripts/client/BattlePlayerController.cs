using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleBattle
{
	public class BattlePlayerController : SingleMonoBehaviour<BattlePlayerController>
	{
		float mPreX;
		public Camera gameCamera;
		void Update ()
		{
//			if (Input.GetMouseButton (0)) {
//				PlayerHandle ph = new PlayerHandle ();
//				float x = Mathf.Round(gameCamera.ScreenToWorldPoint(Input.mousePosition).x * 100) / 100f;
//				if (mPreX != x) {
//					mPreX = x;
//					ph.mousePos = new Vector2 (x,0);
//					ph.playerId = BattleClientController.Instance.playerId;
//					BattleClient.Instance.SendPlayerHandle (ph);
//				}
//			}
		}
	}
}
