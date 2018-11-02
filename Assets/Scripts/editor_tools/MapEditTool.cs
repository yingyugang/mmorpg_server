using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditTool : SingleMonoBehaviour<MapEditTool> {

	public int ground_layer_index = 10;
	public Camera currentCamera;

	public int currentPrefabIndex = 0;

	public List<GameObject> prefabs;
	public List<GameObject> objs;
	public GroundPrefabGrid gridGroundPrefab;
	public GameObject ground;


	protected override void Awake(){
		base.Awake ();
	}

	void Update () {
		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			if(Physics.Raycast(currentCamera.ScreenPointToRay (Input.mousePosition),out hit,Mathf.Infinity,1 << ground_layer_index)){
				Debug.Log (hit.point);
				PlaceWall (hit.point);
			}

		}
	}

	void PlaceWall(Vector3 pos){
		GameObject prefab = gridGroundPrefab.GetSelectPrefab ();
		int x = Mathf.RoundToInt (pos.x);
		int z = Mathf.RoundToInt (pos.z);
		GameObject go = Instantiate (prefab, new Vector3(x,pos.y,z), Quaternion.identity);
		go.transform.SetParent (ground.transform);
	}


}
