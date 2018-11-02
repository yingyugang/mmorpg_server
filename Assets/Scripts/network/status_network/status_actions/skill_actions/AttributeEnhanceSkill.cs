using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class AttributeEnhanceSkill : BaseSkill
	{
		protected override BaseHit GetBaseHit ()
		{
			mIsFriends = true;
			BaseHit baseHit = new AttributeEnhanceHit ();
			baseHit.Init (this.mmoUnit, this);
			return baseHit;
		}

		public override List<MMOUnit> GetSkillableUnits ()
		{
			if (!IsScaneable ())
				return null;
			List<MMOUnit> targetUnits = new List<MMOUnit> ();
			List<MMOUnit> units = this.GetSkillableUnits (true);
			MMOUnit mmoUnit = this.GetCloseUnit (units);
			targetUnits.Add (mmoUnit);
			return targetUnits;
		}

		//TODO
		public override bool IsSkillable (MMOUnit mmoUnit)
		{
			return true;
		}

	}
}
