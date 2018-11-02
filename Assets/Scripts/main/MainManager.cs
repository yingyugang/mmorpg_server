using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MultipleBattle;

namespace MultipleBattle
{
	public class MainManager:SingleMonoBehaviour<MainManager>
	{

		public GameObject container_mian;
		public Button btn_server;
		public Button btn_client;
		public Button btn_host;
		public GameObject prefab_server;

		public GameObject container_server_config;
		public Text txt_max_desc;
		public Text txt_max_num;
		public Button btn_add;
		public Button btn_remove;
		public Button btn_confirm;

		bool mIsHost;

		protected override void Awake ()
		{
			base.Awake ();
			#if UNITY_STANDALONE
			Screen.SetResolution(540,960,false);
			#endif
			txt_max_num.text = NetConstant.MaxNum.ToString ();
			if (NetConstant.MaxNum == 1)
				txt_max_desc.text = "Single";
			else
				txt_max_desc.text = "Multiple";
			btn_server.onClick.AddListener (() => {
				mIsHost = false;
				container_mian.SetActive (false);
				container_server_config.SetActive (true);
			});
			btn_client.onClick.AddListener (() => {
				UnityEngine.SceneManagement.SceneManager.LoadScene ("Client");
			});
			btn_host.onClick.AddListener (() => {
				mIsHost = true;
				container_mian.SetActive (false);
				container_server_config.SetActive (true);
			});
			btn_add.onClick.AddListener (() => {
				int num = NetConstant.MaxNum;
				num++;
				num = Mathf.Clamp (num, 1, 2);
				NetConstant.max_player_count = num;
				txt_max_num.text = num.ToString ();
				txt_max_desc.text = "Multiple";
			});
			btn_remove.onClick.AddListener (() => {
				int num = NetConstant.MaxNum;
				num--;
				num = Mathf.Clamp (num, 1, 2);
				NetConstant.max_player_count = num;
				txt_max_num.text = num.ToString ();
				txt_max_desc.text = "Single";
			});
			btn_confirm.onClick.AddListener (() => {
				if (!mIsHost) {
					UnityEngine.SceneManagement.SceneManager.LoadScene ("Server");
				} else {
					GameObject go = Instantiate (prefab_server);
					go.GetComponentInChildren<Camera> ().clearFlags = CameraClearFlags.Depth;
					DontDestroyOnLoad (go);
					UnityEngine.SceneManagement.SceneManager.LoadScene ("Client");
				}
			});
		}
	}
}
