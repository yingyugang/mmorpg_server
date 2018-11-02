using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleFramework;


namespace MMO
{
	public class DamageSkill : BaseSkill
	{
		protected override BaseHit GetBaseHit ()
		{
			mIsFriends = false;
			BaseHit baseHit = new DamageHit ();
			baseHit.Init (this.mmoUnit, this);
			return baseHit;
		}

		public override List<MMOUnit> GetSkillableUnits ()
		{
			if (!IsScaneable ())
				return null;
			List<MMOUnit> targetUnits = new List<MMOUnit> ();
			List<MMOUnit> units = this.GetSkillableUnits (false);
			MMOUnit mmoUnit = this.GetCloseUnit (units);
			targetUnits.Add (mmoUnit);
			return targetUnits;
		}

		public override bool IsSkillable (MMOUnit mmoUnit)
		{
			if (!mmoUnit.IsDeath) {
				if (this.mmoUnit.camp != mmoUnit.camp) {
					return true;
				} 
			}
			return false;
		}
	}
}
