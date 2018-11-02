using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BattleFramework;

namespace MMO
{
	public enum PlayType{RPG=0,TPS=1}
	public class MMOBattleServerManager : SingleMonoBehaviour<MMOBattleServerManager>
	{
		public MMOServer server;
		public List<BattleTerrain> terrainPrefabs;
		public int terrainIndex = 0;
		public Terrain mTerrain;
		public List<HitInfo> hitInfos;
		public List<StatusInfo> actionInfos;
		public List<ShootInfo> shootInfos;
		public PlayType playType;
		public bool isPvP;
		int mBasePlayerUnitId;

		BattleTerrain mCurrentBattleTerrain;
		Dictionary<int,MMOUnit> mUnitDic;
		//all of the units in this map;
		List<MMOUnit> mUnitList;
		//all of the monster in this map;
		List<MMOUnit> mMonsterList;
		Dictionary<int,MMOUnit> mMonsterDic;
		//all of the palyer in this map;
		List<MMOUnit> mPlayerUnitList;
		List<UnitInfo> mPlayerInfoList;
		Dictionary<int,MMOUnit> mPlayerUnitDic;

		//プーレアが追加された時とか、削除された時とか、このアレイを変更される（こうやってはネット通信するために）
		UnitInfo[] mPlayerInfos;
		UnitInfo[] mMonsterInfos;
		public bool debug;

		int mUnitIndex = 0;

		List<float> mCreateMonsterTime;
		List<int> mCreateMonsterId;

		protected override void Awake ()
		{
			base.Awake ();
			InitTerrain (terrainIndex);
			switch(playType){
			case PlayType.RPG:
				mBasePlayerUnitId = 1;
				break;
			case PlayType.TPS:
				mBasePlayerUnitId = 2;
				break;
			default:
				mBasePlayerUnitId = 1;
				break;
			}
			mMonsterList = new List<MMOUnit> ();
			mMonsterDic = new Dictionary<int,MMOUnit> ();
			mUnitDic = new Dictionary<int, MMOUnit> ();
			mUnitList = new List<MMOUnit> ();
			mPlayerUnitList = new List<MMOUnit> ();
			mPlayerInfoList = new List<UnitInfo> ();
			mPlayerUnitDic = new Dictionary<int, MMOUnit> ();
			ResetMonsterCreates ();
			RandomUtility.SetRandomSeed (999);
			//TODO
			server.onRecievePlayerMessage = OnRecievePlayerMessage;
		}

		void Start ()
		{
			InitMonsters (0);
		}

		void Update ()
		{
			mMonsterInfos = new UnitInfo[mMonsterList.Count];
			for (int i = 0; i < mMonsterList.Count; i++) {
				mMonsterInfos [i] = mMonsterList [i].unitInfo;
			}
			server.Synchronization (mMonsterInfos, hitInfos.ToArray (), actionInfos.ToArray (),shootInfos.ToArray());
			server.SendPlayerData ();
			hitInfos.Clear ();
			actionInfos.Clear ();
			shootInfos.Clear ();
			UpdateMonsterCreates ();
		}

		void InitTerrain (int terrainIndex)
		{
			terrainIndex = Mathf.Clamp (terrainIndex, 0, terrainPrefabs.Count - 1);
			mCurrentBattleTerrain = terrainPrefabs [terrainIndex];
			GameObject terrainGo = Instantiate (mCurrentBattleTerrain.terrainPrefab) as GameObject;
			terrainGo.transform.position = Vector3.zero;
			mTerrain = terrainGo.GetComponent<Terrain> ();
		}

		MMOUnit InstantiateUnit (int unitType, Vector3 targetPos)
		{
			MUnit mUnit = CSVManager.Instance.GetUnit (unitType);
			GameObject unitPrebfab = Resources.Load<GameObject> (BattleConst.DEFAULT_MONSTER_PREFAB);
			unitPrebfab.SetActive (false);
			GameObject unitGo = Instantiate (unitPrebfab) as GameObject;
			unitGo.name = mUnit.resource_name;
			unitGo.transform.position = targetPos;
			unitGo.transform.localEulerAngles = new Vector3 (0,Random.Range(0,360),0);
			MMOUnit mmoUnit = unitGo.GetComponent<MMOUnit> ();
			CapsuleCollider coll = unitGo.GetOrAddComponent<CapsuleCollider> ();
			coll.height = mUnit.height;
			coll.center = new Vector3 (0,coll.height / 2,0);
			coll.radius = mUnit.width;
			mmoUnit.speed = mUnit.speed;
			mmoUnit.unitInfo = new UnitInfo ();
			mmoUnit.unitInfo.attribute.unitId = mUnitIndex;
			mmoUnit.unitInfo.attribute.unitType = unitType;
			mmoUnit.unitInfo.attribute.unitName = mUnit.name;
			mmoUnit.unitInfo.attribute.currentHP = mUnit.max_hp;
			mmoUnit.unitInfo.attribute.maxHP = mUnit.max_hp;
			mmoUnit.unitInfo.unitSkillIds = mUnit.unitSkillIds;
			mmoUnit.unitCSVStructure = mUnit;
			mmoUnit.camp = 1;
			mUnitList.Add (mmoUnit);
			mUnitDic.Add (mUnitIndex, mmoUnit);
			mUnitIndex++;
			mmoUnit.InitUnitAttributes (mUnit);
			return mmoUnit;
		}
		//when player connect
		public MMOUnit AddPlayer (int playerId)
		{
			MMOUnit mmoUnit = CreatePlayerUnit (playerId);
			mPlayerUnitDic.Add (playerId, mmoUnit);
			return mmoUnit;
		}
		// respawn player unit;
		public MMOUnit RespawnPlayer (int playerId)
		{
			MMOUnit preUnit = mPlayerUnitDic [playerId];
			preUnit.ResetUnit ();
			this.server.SendPlayerRespawn (preUnit.unitInfo.attribute.unitId);
			return preUnit;
		}
		//when player disconnect;
		public int RemovePlayer (int playerId)
		{
			MMOUnit mmoUnit = null;
			if(mPlayerUnitDic.ContainsKey(playerId) && mPlayerUnitDic [playerId]!=null){
				mmoUnit = mPlayerUnitDic [playerId];
				mPlayerUnitDic.Remove (playerId);
				RemoveUnit (mmoUnit);
				return mmoUnit.unitInfo.attribute.unitId;
			}
			return -1;
		}
		//create player unit;
		MMOUnit CreatePlayerUnit (int playerId)
		{
			MMOUnit mmoUnit = InstantiateUnit (mBasePlayerUnitId, mCurrentBattleTerrain.playerSpawnPosition);
			mmoUnit.IsPlayer = true;
			mmoUnit.unitInfo.isPlayer = true;
			if (isPvP) {
				mmoUnit.camp = Random.Range (10000, 99999);
			} else {
				mmoUnit.camp = 1;
			}
			mmoUnit.unitInfo.camp = 1;
			mmoUnit.playerId = playerId;
			mmoUnit.InitStatusMachine ();
			mPlayerUnitList.Add (mmoUnit);
			mPlayerInfoList.Add (mmoUnit.unitInfo);
			mmoUnit.gameObject.SetActive (true);
			return mmoUnit;
		}

		public MMOUnit GetUnitByPlayerId (int playerId)
		{
			if (mPlayerUnitDic.ContainsKey (playerId)) {
				return mPlayerUnitDic [playerId];
			}
			return null;
		}

		public MMOUnit GetUnitByUnitId (int unitId)
		{
			if (mUnitDic.ContainsKey (unitId)) {
				return mUnitDic [unitId];
			}
			return null;
		}

		public void RemoveUnit (MMOUnit mmoUnit)
		{
			if (mmoUnit.IsPlayer) {
				RemovePlayerUnit (mmoUnit);
			} else {
				RemoveMonster (mmoUnit);
			}
			Destroy (mmoUnit.gameObject);
		}

		void RemovePlayerUnit (MMOUnit mmoUnit)
		{
			mPlayerUnitList.Remove (mmoUnit);
			mPlayerInfoList.Remove (mmoUnit.unitInfo);
		}

		void RemoveMonster (MMOUnit mmoUnit)
		{
			mMonsterDic.Remove (mmoUnit.unitInfo.attribute.unitId);
			mMonsterList.Remove (mmoUnit);
		}

		public void AddHitInfo (HitInfo hitInfo)
		{
			hitInfos.Add (hitInfo);
		}

		public void AddAction (StatusInfo action)
		{
			if(MMOBattleServerManager.Instance.debug){
				Debug.Log (JsonUtility.ToJson(action));
			}
			actionInfos.Add (action);
		}

		public void OnStartAutoPlayerUnit (int unitId)
		{
			if (mPlayerUnitDic.ContainsKey (unitId)) {
				MMOUnit unit = this.mPlayerUnitDic [unitId];
				unit.StartAuto ();
			}
		}

		public void OnEndAutoPlayerUnit (int unitId)
		{
			if (mPlayerUnitDic.ContainsKey (unitId)) {
				MMOUnit unit = this.mPlayerUnitDic [unitId];
				unit.EndAuto ();
			}
		}
		//TODO update skill and position.
		public void OnRecievePlayerMessage (DoublePlayerInfo playerInfo)
		{
			MMOUnit mmoUnit = mUnitDic [playerInfo.clientPlayerInfo.unitInfo.attribute.unitId];
			if (mmoUnit.IsAuto) {
				return;
			}
			mmoUnit.transform.position = IntVector3.ToVector3 (playerInfo.clientPlayerInfo.unitInfo.transform.position);
			mmoUnit.transform.forward = IntVector3.ToVector3 (playerInfo.clientPlayerInfo.unitInfo.transform.forward);
			if (playerInfo.clientPlayerInfo.skillId > -1) {
				//TODO check the skill use able;
				MMOUnit target = null;
				if (playerInfo.clientPlayerInfo.unitInfo.action.targetId != -1) {
					if (mMonsterDic.ContainsKey (playerInfo.clientPlayerInfo.targetId)) {
						target = mMonsterDic [playerInfo.clientPlayerInfo.targetId];
					}
				}
			}
			playerInfo.clientPlayerInfo.unitInfo.action.actionId = -1;
			playerInfo.serverPlayerInfo.unitInfo.action.actionId = -1;
		}

		void ClearMonsters(){
			while(this.mMonsterList.Count>0){
				RemoveUnit (this.mMonsterList[0]);
			}
		}

		public void InitMonsters (int index)
		{
			ClearMonsters ();
			ResetMonsterCreates ();
			CSVManager.Instance.SetMonsterCSV (index);
			List<MMapMonster> monsters = CSVManager.Instance.monsterList;
			for (int i = 0; i < monsters.Count; i++) {
				CreateMonster (monsters [i]);
			}
		}

		public Vector3 GetTerrainPos(Vector3 pos){
			Vector3 terrainPos = new Vector3 (pos.x,mTerrain.SampleHeight (pos),pos.z);
			return terrainPos;
		}

		void CreateMonster (MMapMonster monster)
		{
			Vector3 pos = new Vector3 (monster.pos_x, 0, monster.pos_y);
			pos = GetTerrainPos (pos);
			MMOUnit unit = InstantiateUnit (monster.unit_id, pos);
			//TODO AIManager が必要だ
			if (monster.action == 2)
				unit.gameObject.GetOrAddComponent<SimpleAI> ();
			unit.defaultPos = unit.transform.position;
			mMonsterList.Add (unit);
			mMonsterDic.Add (unit.unitInfo.attribute.unitId, unit);
			unit.camp = monster.alignment;
			unit.unitInfo.camp = monster.alignment;
			unit.isFollowTarget = true;
			unit.InitStatusMachine ();
			unit.monsterId = monster.id;
			unit.IsPlayer = false;
			unit.unitInfo.isPlayer = false;
			unit.gameObject.SetActive (true);
		}

		public void AddShootInfo(int casterId ,int unitSkillId,int targetId,IntVector3 targetPos){
			ShootInfo shootInfo = ShootInfo.Instance (casterId,unitSkillId,targetId,targetPos,IntVector3.Zero());
			shootInfos.Add (shootInfo);
		}

		public void AddShootInfo(ShootInfo shootInfo ){
			shootInfos.Add (shootInfo);
		}

		public void OnUnitDeath (MMOUnit mmoUnit)
		{
			if (!mmoUnit.IsPlayer) {

				float delay = Random.Range (BattleConst.MONSTER_RESPAWN_DELAY * 0.8f, BattleConst.MONSTER_RESPAWN_DELAY * 1.2f);
				int monsterId = mmoUnit.monsterId;
				AddMonsterCreate (Time.time + delay,monsterId);
				RemoveMonster (mmoUnit);
				Destroy (mmoUnit.gameObject);
			}
		}
		//TODO 需要和单位的状态机整合到一起。工程量比较大，需要考虑。目前做法参照客户端ActionManager
		//1:unit skill,2:start auto,3:end auto(must),4,change status(actionId:1=idle,2=move,3=run,4=death)。
		public void OnRecievePlayerAction (int connId,int playerId, StatusInfo action)
		{
			MMOUnit playerUnit = GetUnitByPlayerId (playerId);
			MMOUnit targetUnit = GetUnitByUnitId (action.targetId);
			StatusInfo backAction = new StatusInfo ();
			backAction.casterId = playerUnit.unitInfo.attribute.unitId;
			//TODO 単純な动作通行，最好放到单独的方法处理比较好。
			switch (action.status) {
			case BattleConst.UnitMachineStatus.STANDBY:
				backAction.status = BattleConst.UnitMachineStatus.STANDBY;
				break;
			case BattleConst.UnitMachineStatus.MOVE:
				backAction.status = BattleConst.UnitMachineStatus.MOVE;
				break;
			case BattleConst.UnitMachineStatus.FIRE:
				backAction.status = BattleConst.UnitMachineStatus.FIRE;
				break;
			case BattleConst.UnitMachineStatus.UNFIRE:
				backAction.status = BattleConst.UnitMachineStatus.UNFIRE;
				break;
			case BattleConst.UnitMachineStatus.JUMP:
				backAction.status = BattleConst.UnitMachineStatus.JUMP;
				break;
			case BattleConst.UnitMachineStatus.LYING:
				backAction.status = BattleConst.UnitMachineStatus.LYING;
				break;
			case BattleConst.UnitMachineStatus.UNLYING:
				backAction.status = BattleConst.UnitMachineStatus.UNLYING;
				break;
			case BattleConst.UnitMachineStatus.RELOAD:
				backAction.status = BattleConst.UnitMachineStatus.RELOAD;
				break;
			case BattleConst.UnitMachineStatus.SQUAT:
				backAction.status = BattleConst.UnitMachineStatus.SQUAT;
				break;
			case BattleConst.UnitMachineStatus.UNSQUAT:
				backAction.status = BattleConst.UnitMachineStatus.UNSQUAT;
				break;
			case BattleConst.UnitMachineStatus.CAST:
				PlayerActionManager.Instance.DoSkill (playerUnit, targetUnit, action);
				break;
			default:
				break;
			}
			if (backAction != null)
				server.SendPlayerStatusToOther (connId,backAction);
		}

		IEnumerator _RespawnMonsterDelay (int monsterId, float delay)
		{
			delay = Random.Range (delay * 0.8f, delay * 1.2f);
			yield return new WaitForSeconds (delay);
			MMapMonster monster = CSVManager.Instance.monsterDic [monsterId];
			CreateMonster (monster);
		}

		void AddMonsterCreate(float time,int id){
			this.mCreateMonsterId.Add (id);
			this.mCreateMonsterTime.Add (time);
		}

		void ResetMonsterCreates(){
			this.mCreateMonsterId = new List<int> ();
			this.mCreateMonsterTime = new List<float> ();
		}
		//这样做比用协程高效并且容易控制。
		void UpdateMonsterCreates(){
			if(mCreateMonsterTime.Count>0){
				if(mCreateMonsterTime[0]<Time.time){
					if(CSVManager.Instance.monsterDic.ContainsKey(mCreateMonsterId[0])){
						MMapMonster monsterCSV = CSVManager.Instance.monsterDic[mCreateMonsterId[0]];
						CreateMonster (monsterCSV);
					}
					mCreateMonsterTime.RemoveAt (0);
					mCreateMonsterId.RemoveAt (0);
				}
			}
		}
	}

	[System.Serializable]
	public class BattleTerrain
	{
		public Vector3 playerSpawnPosition;
		public GameObject terrainPrefab;
	}

}