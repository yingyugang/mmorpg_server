using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MMO
{
	public class HealHit : BaseHit
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
			hitInfo.hitPositions = new IntVector3[hittedUnits.Count];
			if(hittedUnits.Count>0)
				hitInfo.hitPositions [0] = IntVector3.ToIntVector3 (mHitPos);

			for(int i=0;i< hittedUnits.Count;i++){
				hitInfo.hitIds [i] = hittedUnits [i].unitInfo.attribute.unitId;
				hitInfo.nums [i] = PlayerActionManager.Instance.GetHealth (mmoUnit,hittedUnits[i],mBaseSkill.skillCSVStructure);
				PlayerActionManager.Instance.Health (hittedUnits[i],hitInfo.nums [i]);
			}
			return hitInfo;
		}

	}
}

