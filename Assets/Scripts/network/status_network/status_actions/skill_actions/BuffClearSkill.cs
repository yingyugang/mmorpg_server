using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class BuffClearSkill  : BaseSkill
	{

		protected override BaseHit GetBaseHit ()
		{
			BaseHit baseHit = new BuffClearHit ();
			baseHit.Init (this.mmoUnit, this);
			return baseHit;
		}

		//TODO
		public override bool IsSkillable (MMOUnit mmoUnit)
		{
			return true;
		}

	}
}
