using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	public class StatusClearHit : BaseHit
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
				List<BaseBuff> buffs = hittedUnits [i].mmoUnitBuff.list_actived_buffs;
				for (int j = 0; j < buffs.Count; j++) {
					if(PlayerActionManager.Instance.IsStatus(buffs [j].skillEffectCSVStructure)){
						hittedUnits [i].RemoveBuff (buffs [j]);
						j--;
					}
				}
				hitInfo.nums [i] = 1;
			}
			return hitInfo;
		}


	}
}

