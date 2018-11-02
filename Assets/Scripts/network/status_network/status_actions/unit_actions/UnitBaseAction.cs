using UnityEngine;
using System.Collections;

namespace MMO
{
	public class UnitBaseAction : StatusAction
	{

		//	public Hero hero;
		public MMOUnit mmoUnit;

		public override void OnAwake ()
		{
			base.OnAwake ();
			mmoUnit = GO.GetComponent<MMOUnit> ();
		}

		public override void OnEnter(){
			base.OnEnter ();
		}


		//TODO need set the ofter enter.
		protected virtual void SendAction(){
		
		}
	}
}
