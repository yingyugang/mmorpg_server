using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleBattle
{
	public class BattleClientResourceManager : SingleMonoBehaviour<BattleClientResourceManager>
	{
	
		//TODO ,暂时这样做，绑定到Client场景，而不是把美术资源放到resource下面，这样打Server场景的时候就不会把这些资源打进去
		//TODO ,以后用ab的方式加载
		public List<GameObject> prefabs;

		public GameObject GetPlantPrefab (int playerId)
		{
			playerId = playerId - 1;
			if (prefabs.Count > playerId) {
				return prefabs [playerId];
			}
			return prefabs [0];
		}

	}
}
