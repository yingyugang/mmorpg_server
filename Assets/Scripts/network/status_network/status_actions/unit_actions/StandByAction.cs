using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	public class StandByAction : UnitBaseAction
	{

		public override void OnAwake ()
		{
			base.OnAwake ();
		}
		float mNextCheckTime;
		public override void OnEnter ()
		{
			base.OnEnter ();
			SendAction ();
		}

		protected override void SendAction ()
		{
			base.SendAction ();
			StatusInfo action = new StatusInfo ();
			action.casterId = mmoUnit.unitInfo.attribute.unitId;
			action.status = BattleConst.UnitMachineStatus.STANDBY;
			MMOBattleServerManager.Instance.AddAction (action);
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
		}
	}
}
