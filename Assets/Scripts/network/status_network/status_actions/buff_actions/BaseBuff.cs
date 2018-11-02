using UnityEngine;
using System.Collections;

namespace MMO
{
	public class BaseBuff : UnitBaseAction
	{
		//TODO need change the skill effect csv data to special class.
		public SkillEffectCSVStructure skillEffectCSVStructure;
		public bool isExcuting;
		float mEndTime;

		public virtual void OnDestory(){
			
		}

		public override void OnEnter(){
			mEndTime = Time.time + skillEffectCSVStructure.effect_max_round;
		}

		public bool IsEnd{
			get{ 
				return mEndTime < Time.time;
			}
		}

	}
}