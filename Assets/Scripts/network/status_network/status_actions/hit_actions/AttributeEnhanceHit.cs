using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	public class AttributeEnhanceHit : BaseHit
	{

		public override HitInfo GetHitInfo ()
		{
			HitInfo hitInfo = new HitInfo ();
			hitInfo.hitOriginPosition = IntVector3.ToIntVector3 (mHitPos);
			hitInfo.casterId = this.mmoUnit.unitInfo.attribute.unitId;
			List<MMOUnit> hittedUnits = GetHittedUnits();
			hitInfo.unitSkillId = mBaseSkill.unitSkillCSVStructure.id;
			hitInfo.hitIds = new int[hittedUnits.Count];
			hitInfo.nums = new int[hittedUnits.Count];
			//TODO コードでセットしなければいけない。
			hitInfo.hitObjectIds = new int[1];
			hitInfo.hitObjectIds [0] = 0;
			hitInfo.hitPositions = new IntVector3[1];
			hitInfo.hitPositions [0] = IntVector3.ToIntVector3 (mHitPos);

			for(int i=0;i< hittedUnits.Count;i++){
				hitInfo.hitIds [i] = hittedUnits [i].unitInfo.attribute.unitId;
				int skillValue = PlayerActionManager.Instance.GetSkillValue (this.mBaseSkill.skillCSVStructure);
				hitInfo.nums [i] = skillValue;
				//buff debuff status are all buffs;
				SkillEffectCSVStructure buff = new SkillEffectCSVStructure ();
				buff.effect_max_round = this.mBaseSkill.skillCSVStructure.effect_continue_round;
				buff.effect_type = this.mBaseSkill.skillCSVStructure.effect_type;
				buff.effect_value = skillValue;
				buff.skillCSVStructure = mBaseSkill.skillCSVStructure;
				AttributeEnhanceBuff buffAction = new AttributeEnhanceBuff ();
				buffAction.skillEffectCSVStructure = buff;
				buffAction.mmoUnit = mmoUnit;
				//need dic to check whether the buff exiting on the unit.
				hittedUnits[i].AddBuff (buffAction);
			}
			return hitInfo;
		}
	}
}
