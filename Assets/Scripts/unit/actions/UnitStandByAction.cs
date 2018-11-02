using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class UnitStandByAction : UnitAction
	{

		public override void OnEnter ()
		{
			base.OnEnter ();
			mUnit.unitRes.PlayStandBy ();
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
		}

	}

}
