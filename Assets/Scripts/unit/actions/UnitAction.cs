using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class UnitAction : StatusAction
	{

		protected Unit mUnit;

		public override void OnAwake ()
		{
			base.OnAwake ();
			mUnit = GO.GetComponent<Unit> ();
		}

	}
}
