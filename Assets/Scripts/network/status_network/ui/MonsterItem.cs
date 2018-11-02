using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MMO
{
	public class MonsterItem : MonoBehaviour
	{
		public int index;
		public string configName;
		Text txt_config;
		Button btn_config;

		public void Init(int index,string configName){
			this.index = index;
			this.configName = configName;
			btn_config = GetComponent<Button> ();
			txt_config = GetComponentInChildren<Text> (true);
			btn_config.onClick.AddListener (()=>{
				MMOBattleServerManager.Instance.InitMonsters(index);
			});
			txt_config.text = this.configName;
		}
	
	}
}
