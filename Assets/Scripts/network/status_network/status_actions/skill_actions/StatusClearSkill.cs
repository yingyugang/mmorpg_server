using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class StatusClearSkill : BaseSkill {

		protected override BaseHit GetBaseHit ()
		{
			BaseHit baseHit = new StatusClearHit ();
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
