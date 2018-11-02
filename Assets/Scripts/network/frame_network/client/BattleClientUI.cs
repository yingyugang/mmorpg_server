using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace MultipleBattle
{
	public class BattleClientUI : SingleMonoBehaviour<BattleClientUI>
	{
		public GameObject dialog;
		public InputField input_ip;
		public InputField input_port;
		public Button btn_connect;
		public Button btn_ready;
		public Button btn_replay;
		public Button btn_save;
		public Button btn_disconnect;
		public Text txt_frame;
		public Text txt_frame1;
		public GridLayoutGroup grid_players;
		public GameObject grid_players_item_prefab;
		public List<GameObject> grid_players_items;
		public Dictionary<int,GameObject> grid_players_items_dic;

		const string SERVER_IP = "SERVER_IP";
		const string SERVER_PORT = "SERVER_PORT";

		void Start ()
		{
			if (PlayerPrefs.HasKey (SERVER_IP))
				input_ip.text = PlayerPrefs.GetString (SERVER_IP);
			else
				input_ip.text = BattleClient.Instance.defaultIP;
			if (PlayerPrefs.HasKey (SERVER_PORT))
				input_port.text = PlayerPrefs.GetString (SERVER_PORT);
			else
				input_port.text = BattleClient.Instance.defaultPort.ToString ();
			btn_connect.onClick.AddListener (Connect);
			btn_ready.onClick.AddListener (SendReadyToServer);
			btn_save.onClick.AddListener (Save);
			btn_replay.onClick.AddListener (Replay);
			btn_disconnect.onClick.AddListener (Stop);
			grid_players_items = new List<GameObject> ();
			grid_players_items_dic = new Dictionary<int, GameObject> ();
			BattleClient.Instance.onBattleStart = OnBattleBegin;
		}

		void Connect ()
		{
			input_ip.gameObject.SetActive (false);
			input_port.gameObject.SetActive (false);
			btn_connect.gameObject.SetActive (false);
			btn_replay.gameObject.SetActive (false);
			BattleClient.Instance.Connect (input_ip.text, int.Parse (input_port.text),(NetworkMessage netMsg)=>{
				OnConnect();
			});
		}

		void Save(){
//			BattleClientReplayManager.Instance.SaveRecord ();
		}

		void Replay(){
//			BattleClientReplayManager.Instance.Replay ();
			btn_replay.gameObject.SetActive (false);
			input_ip.gameObject.SetActive (false);
			input_port.gameObject.SetActive (false);
			btn_connect.gameObject.SetActive (false);
		}

		void Stop(){
			BattleClientController.Instance.Stop ();
		}

		void SendReadyToServer(){
			BattleClient.Instance.SendReadyToServer ();
			btn_ready.gameObject.SetActive (false);
			dialog.SetActive (false);
		}

		public void OnConnect(){
			PlayerPrefs.SetString (SERVER_IP,input_ip.text);
			PlayerPrefs.SetString (SERVER_PORT,input_port.text);
			PlayerPrefs.Save ();
			btn_ready.gameObject.SetActive (true);
			btn_disconnect.gameObject.SetActive (true);
		}

		public void OnDisconnected(){
			input_ip.gameObject.SetActive (true);
			input_port.gameObject.SetActive (true);
			btn_connect.gameObject.SetActive (true);
			btn_ready.gameObject.SetActive (false);
			btn_replay.gameObject.SetActive (true);
			btn_disconnect.gameObject.SetActive (false);
			btn_save.gameObject.SetActive (false);
		}

		public void OnBattleBegin(CreatePlayer cp){
			btn_save.gameObject.SetActive (true);
			UnityEngine.SceneManagement.SceneManager.LoadScene (SceneConstant.SCENE_BATTLE);
			Debug.Log (JsonUtility.ToJson(cp));
		}

		public void OnPlayerStatus(PlayerStatusArray psa){
			for(int i=0;i<grid_players_items.Count;i++){
				Destroy (grid_players_items[i]);
			}
			grid_players_items.Clear ();
			grid_players_items_dic.Clear ();
			Debug.Log (psa.playerStatus.Length);
			for(int i=0;i<psa.playerStatus.Length;i++){
				GameObject go = Instantiate<GameObject>(grid_players_item_prefab);
				go.transform.Find ("txt_name").GetComponent<Text>().text = psa.playerStatus[i].playerId.ToString();
				go.transform.Find ("txt_isready").GetComponent<Text> ().text = psa.playerStatus [i].isReady.ToString();
				go.transform.SetParent (grid_players.transform);
				go.SetActive (true);
				grid_players_items.Add (go);
				grid_players_items_dic.Add (psa.playerStatus[i].playerId,go);
			}
		}

		public void SetPlayerScore(PlayerInfo playerInfo){
			GameObject item;
			if (grid_players_items_dic.TryGetValue (playerInfo.playerId, out item)) {
				Text txt_score = item.transform.Find ("txt_score").GetComponent<Text> ();
				txt_score.text = playerInfo.score.ToString ();
			}
		}

	}
}
