using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class UnitAttackAction : UnitAction
	{

		public override void OnEnter ()
		{
			base.OnEnter ();
			this.mUnit.unitRes.PlayAttack ();
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
		}

	}

}
