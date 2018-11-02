using UnityEngine;
using System.Collections;

namespace MMO
{
	public class DeathAction : UnitBaseAction
	{

		public float unspawnDelay = 10;
		float mUnspawnTime = 0;

		public override void OnEnter ()
		{
			base.OnEnter ();
			mUnspawnTime = Time.time + unspawnDelay;
			mmoUnit.UnCollider ();
			SendAction ();
		}

		protected override void SendAction ()
		{
			base.SendAction ();
			StatusInfo action = new StatusInfo ();
			action.casterId = mmoUnit.unitInfo.attribute.unitId;
			action.status = BattleConst.UnitMachineStatus.DEATH;
			MMOBattleServerManager.Instance.AddAction (action);
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			if (mUnspawnTime < Time.time) {
				MMOBattleServerManager.Instance.OnUnitDeath (mmoUnit);
			}
		}

	}
}
