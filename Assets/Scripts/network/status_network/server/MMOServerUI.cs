using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MMO
{
	public class MMOServerUI : SingleMonoBehaviour<MMOServerUI>
	{
		public GameObject container_base_info;
		public GameObject container_monster_info;

		public Text txt_ip;
		public Text txt_port;

		public Camera uiCamera;

		protected override void Awake ()
		{
			base.Awake ();
		}

		void Start ()
		{
			txt_ip.text = string.Format(" IP : {0}",Network.player.ipAddress);
			txt_port.text = string.Format(" Port : {0}",NetConstant.LISTENE_PORT.ToString ());
		}
	}
}
