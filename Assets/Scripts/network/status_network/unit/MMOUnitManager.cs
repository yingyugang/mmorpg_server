using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	//TODO need move thing about the spawn unit into this script.
	public class MMOUnitManager : SingleMonoBehaviour<MMOUnitManager>
	{
		List<MMOUnit> units;

		protected override void Awake ()
		{
			base.Awake ();
			units = new List<MMOUnit> ();
		}

		void Update(){
			for(int i=0;i<units.Count;i++){
				if(units [i]!=null)
					units [i].OnUpdate ();
			}
		}

		public void AddUnit(MMOUnit unit){
			units.Add (unit);
		}

	}
}