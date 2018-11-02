using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class UnitMoveAction : UnitAction
	{

		public override void OnEnter ()
		{
			base.OnEnter ();
			mUnit.unitRes.PlayMove ();
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
		}

	}
}
