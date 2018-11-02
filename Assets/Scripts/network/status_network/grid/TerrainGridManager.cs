using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	//用正方形来存放node，便于发送的时候剔除不必要的通行。
	public class TerrainGridManager : SingleMonoBehaviour<TerrainGridManager>
	{
		public int terrainSize = 10000;
		public int nodeSize = 100;
		public TerrainNode[,] nodeArray;
		public int nodeSideCount;

		protected override void Awake ()
		{
			base.Awake ();
			InitTerrainNodes ();
		}

		void InitTerrainNodes ()
		{
			int nodeCountX = terrainSize % nodeSize > 0 ? terrainSize / nodeSize + 1 : terrainSize / nodeSize;
			int nodeCount = nodeCountX * nodeCountX;
			this.nodeSideCount = nodeCountX;
			nodeArray = new TerrainNode[nodeCountX, nodeCountX];
			for (int i = 0; i < nodeCountX; i++) {
				for (int j = 0; j < nodeCountX; j++) {
					TerrainNode node = new TerrainNode ();
					node.x = j * nodeCountX * nodeSize;
					node.z = i * nodeCountX * nodeSize;
					nodeArray [j, i] = node;	
				}
			}
		}

		public void UpdateNodes (List<MMOUnit> players, List<MMOUnit> monsters)
		{
			for (int i = 0; i < players.Count; i++) {
				UpdateNode (players [i]);
			}
			for (int i = 0; i < monsters.Count; i++) {
				UpdateNode (monsters [i]);
			}
		}

		void UpdateNode (MMOUnit mmoUnit)
		{
			int x = (int)mmoUnit.transform.position.x / nodeSize;
			int z = (int)mmoUnit.transform.position.z / nodeSize;

			if (x < 0 || x > nodeSideCount || z < 0 || z > nodeSideCount) {
				Debug.LogError (string.Format ("Unit Position is Error,x:{0},z:{1}", x, z));
				return;
			}
			TerrainNode node = nodeArray [x, z];
			if (mmoUnit.currentTerrainNode != node) {
				if (mmoUnit.currentTerrainNode != null) {
					if (mmoUnit.IsPlayer) {
						mmoUnit.currentTerrainNode.players.Remove (mmoUnit);
					} else {
						mmoUnit.currentTerrainNode.monsters.Remove (mmoUnit);
					}
				}
//				Debug.Log (string.Format ("Set current node,x:{0},z:{1}", x, z));
				mmoUnit.currentTerrainNode = node;
				if (mmoUnit.IsPlayer) {
					mmoUnit.currentTerrainNode.players.Add (mmoUnit);
				} else {
					mmoUnit.currentTerrainNode.monsters.Add (mmoUnit);
				}
			}
		}
	}

	public class TerrainNode
	{
		public int x;
		public int z;
		public List<MMOUnit> players;
		public List<MMOUnit> monsters;

		public TerrainNode ()
		{
			players = new List<MMOUnit> ();
			monsters = new List<MMOUnit> ();
		}

	}
}
