using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	[RequireComponent (typeof(MMOUnit))]
	public class MMOUnitBuff : MonoBehaviour
	{
		public List<BaseBuff> list_actived_buffs;
		MMOUnit mMMOUnit;

		void Awake(){
			list_actived_buffs = new List<BaseBuff> ();
			mMMOUnit = GetComponent<MMOUnit> ();
		}

		public void Reset(){
			list_actived_buffs = new List<BaseBuff> ();
		}

		public void OnUpdate(){
			UpdateBuffs ();
		}

		void UpdateBuffs(){
			for(int i=0;i<list_actived_buffs.Count;i++){
				BaseBuff baseBuff = list_actived_buffs [i];
				if(baseBuff.IsEnd){
					RemoveBuff (baseBuff);
					i--;
				}
			}
		}

		public void AddBuff (BaseBuff baseBuff)
		{
			baseBuff.mmoUnit = mMMOUnit;
			baseBuff.GO = mMMOUnit.gameObject;
			list_actived_buffs.Add (baseBuff);
			baseBuff.OnAwake ();
			baseBuff.OnEnter ();
		}

		public void RemoveBuff (BaseBuff baseBuff)
		{
			//TODO List.Remove はちょっと遅いね。
			baseBuff.OnDestory ();
			list_actived_buffs.Remove (baseBuff);
		}

	}
}

