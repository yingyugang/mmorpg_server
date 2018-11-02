using UnityEngine;
using System.Collections;

namespace MMO
{
	public class AttributeEnhanceBuff : BaseBuff
	{
		public override void OnAwake ()
		{
			base.OnAwake ();
			int effectType = this.skillEffectCSVStructure.effect_type;
			mmoUnit.additionUnitAttribute.attributes [effectType] += this.skillEffectCSVStructure.effect_value;
			skillEffectCSVStructure.effect_current_round++;
			if(effectType == BattleConst.BattleSkillEffectTypeConst.UnTurn){
				//TODO
				//To tell 伝え　client do something , change material , stop animation.
			}
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			isExcuting = false;
		}

		public override void OnEnter ()
		{
			base.OnEnter ();
		}

		public override void OnExit ()
		{
			base.OnExit ();
		}

		public override void OnDestory ()
		{
			base.OnDestory ();
			int effectType = this.skillEffectCSVStructure.effect_type;
			mmoUnit.additionUnitAttribute.attributes [effectType] -= this.skillEffectCSVStructure.effect_value;
			//针对UnTurn的perfrom，简单的逻辑没有必要再分出BuffPerform了。
			if(effectType == BattleConst.BattleSkillEffectTypeConst.UnTurn && mmoUnit.GetAttribute(BattleConst.BattleSkillEffectTypeConst.UnTurn) <= 0){
				//TODO
				//To tell 伝え　client do something , change material , stop animation.
			}
		}

	}
}
