using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MMO
{
	public class MonsterListPanel : MonoBehaviour
	{
		public GameObject itemPrefab;
		public Transform container_root;
		List<MonsterItem> mItems;

		void Start ()
		{
			itemPrefab.SetActive (false);
			Init ();
		}

		void Init ()
		{
			for (int i = 0; i < CSVManager.Instance.monsterTextAssetList.Count; i++) {
				GameObject item = Instantiate (itemPrefab);
				MonsterItem monsterItem = item.GetOrAddComponent<MonsterItem> ();
				monsterItem.Init (i,CSVManager.Instance.monsterTextAssetList[i].name);
				item.transform.SetParent (container_root);
				item.transform.localPosition = Vector3.zero;
				item.transform.localScale = Vector3.one;
				item.transform.localEulerAngles = Vector3.zero;
				item.SetActive (true);
			}
		}

	}
}
