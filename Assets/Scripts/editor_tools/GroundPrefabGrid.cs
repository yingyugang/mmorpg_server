using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundPrefabGrid : MonoBehaviour {

	public GameObject itemPrefab;
	public List<GameObject> items;
	int mIndex = 0;
	GameObject mCurrentSelectItem;

	void Start () {
		items = new List<GameObject> ();
		List<GameObject> wallPrefabs = MapEditTool.Instance.prefabs;
		for(int i=0;i<wallPrefabs.Count;i++){
			GameObject go = Instantiate<GameObject> (itemPrefab);
			go.GetComponent<Button> ().onClick.AddListener (OnItemClick);
			go.transform.SetParent (transform);
			go.SetActive (true);
			go.GetComponentInChildren<Text> ().text = wallPrefabs [i].name;
			items.Add (go);
		}
		SetSelectItem ();
	}

	void OnItemClick(){
		GameObject go = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
		mIndex = items.IndexOf (go);
		SetSelectItem ();
	}

	void SetSelectItem(){
		GameObject go = items[mIndex];
		if (mCurrentSelectItem != null) {
			mCurrentSelectItem.GetComponentInChildren<Text> ().color = Color.black;
		}
		go.GetComponentInChildren<Text> ().color = Color.green;
		mCurrentSelectItem = go;
	}

	public GameObject GetSelectPrefab(){
		return MapEditTool.Instance.prefabs [mIndex];
	}

}
