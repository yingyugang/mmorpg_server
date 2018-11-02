using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class GMManager : SingleMonoBehaviour<GMManager>
	{

		protected override void Awake ()
		{
			base.Awake ();
		}

		void Update(){
			if(Input.GetKeyDown(KeyCode.F)){
				ToggleMonsterTool ();
			}
		}

		void ToggleMonsterTool(){
			if (MMOServerUI.Instance.container_monster_info.activeInHierarchy) {
				MMOServerUI.Instance.container_monster_info.SetActive (false);
				MMOServerUI.Instance.uiCamera.enabled = false;
			} else {
				MMOServerUI.Instance.container_monster_info.SetActive (true);
				MMOServerUI.Instance.uiCamera.enabled = true;
			}
		}
	}
}
