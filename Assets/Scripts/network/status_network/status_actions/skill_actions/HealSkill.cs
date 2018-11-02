using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class HealSkill : BaseSkill
	{

		protected override BaseHit GetBaseHit ()
		{
			mIsFriends = true;
			BaseHit baseHit = new HealHit ();
			baseHit.Init (this.mmoUnit, this);
			return baseHit;
		}

		public override List<MMOUnit> GetSkillableUnits ()
		{
			if (!IsScaneable ())
				return null;
			List<MMOUnit> targetUnits = new List<MMOUnit> ();
			List<MMOUnit> units = this.GetSkillableUnits (true);
			MMOUnit mmoUnit = this.GetInjuredUnit (units);
			if(mmoUnit!=null)
			targetUnits.Add (mmoUnit);
			return targetUnits;
		}

		public override bool IsSkillable (MMOUnit mmoUnit)
		{
			if (!mmoUnit.IsDeath && mmoUnit.GetHP () < mmoUnit.GetMaxHP()) {
				if (this.mmoUnit.camp == mmoUnit.camp) {
					return true;
				} 
			}
			return false;
		}

	}
}
