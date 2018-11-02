using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MMO
{
	[RequireComponent (typeof(MMOUnitSkill))]
	[RequireComponent (typeof(MMOUnitBuff))]
	public class MMOUnit : MonoBehaviour
	{
		//主にunitInfoはサーバーとクラアント同期用
		public UnitInfo unitInfo;

		public MUnit unitCSVStructure;
		//if is monster;
		public int monsterId;

		public MMOUnitSkill mmoUnitSkill;

		public MMOUnitBuff mmoUnitBuff;

		public SimpleAI simpleAI;

		//camp 1:player
		public int camp = 1;
		public int playerId;
		public float defaultAttackDuration;
		public List<int> elements;
		public MMOUnitAttribute baseUnitAttribute;
		//自身のパラメーターと装備のパラメーター
		public MMOUnitAttribute additionUnitAttribute;
		//Buffなど
		public FinalStatusMachine fsm;
		Collider mCollider;
		Transform mTrans;
		public Vector3 defaultPos;
		public Vector3 moveTargetPos;
		public StatusInfo targetStatusInfo;
		//TODO 这个不要作为技能目标比较好
		public Transform target;
		public float stopDistance = 2;
		public float speed = 5;
		public float nextScanTime;
		public string moveClip = AnimationConstant.UNIT_ANIMATION_CLIP_RUN;
		public int onMoveDoneStatus;
		public TerrainNode currentTerrainNode;
		CapsuleCollider mCapsuleCollider;
		bool mIsPlayer;

		List<UnitBaseAction> aiActions = new List<UnitBaseAction> ();
		NavMeshAgent mAgent;

		void Awake ()
		{
			mTrans = transform;
			mmoUnitSkill = GetComponent<MMOUnitSkill> ();
			mmoUnitBuff = GetComponent<MMOUnitBuff> ();
			mCollider = GetComponent<Collider> ();
			mAgent = GetComponent<NavMeshAgent> ();
			simpleAI = GetComponent<SimpleAI> ();
			mCapsuleCollider = GetComponent<CapsuleCollider> ();
			additionUnitAttribute = new MMOUnitAttribute ();
			MMOUnitManager.Instance.AddUnit (this);
			InActive ();
			StartCoroutine (_DelayActive());
		}

		public void ResetUnit(){
			if(fsm!=null){
				fsm.Reset ();
			}
			InitStatusMachine ();
			InitUnitAttributes (this.unitCSVStructure);
			additionUnitAttribute = new MMOUnitAttribute ();
			if (mmoUnitBuff != null)
				mmoUnitBuff.Reset ();
			if (mmoUnitSkill != null)
				mmoUnitSkill.Reset ();
			target = null;
			mCapsuleCollider.enabled = true;
		}

		IEnumerator _DelayActive(){
			yield return new WaitForSeconds (BattleConst.UNIT_ACTIVE_DELAY);
			Active ();
		}

		void Active(){
			fsm.enabled = true;
			mCapsuleCollider.enabled = true;
		}

		void InActive(){
			fsm.enabled = false;
			mCapsuleCollider.enabled = false;
		}

		public void UnCollider ()
		{
			mCollider.enabled = false;
		}

		public void EnCollider(){
			mCollider.enabled = true;
		}

		public void InitStatusMachine ()
		{
			fsm = gameObject.GetOrAddComponent<FinalStatusMachine> ();
			fsm.AddAction (BattleConst.UnitMachineStatus.STANDBY, new StandByAction ());
			ScanAction scanAction = new ScanAction ();
			fsm.AddAction (BattleConst.UnitMachineStatus.STANDBY, scanAction);
			BackToDefaultAction backAction = new BackToDefaultAction ();
			fsm.AddAction (BattleConst.UnitMachineStatus.STANDBY,backAction);
			if (IsPlayer) {
				scanAction.IsEnable = false;
				aiActions.Add (scanAction);
				backAction.IsEnable = false;
				aiActions.Add (backAction);
			} 
			fsm.AddAction (BattleConst.UnitMachineStatus.MOVE, new MoveAction ());
			fsm.AddAction (BattleConst.UnitMachineStatus.DEATH, new DeathAction ());
			fsm.AddAction (BattleConst.UnitMachineStatus.CAST, new CastAction ());
			fsm.ChangeStatus (BattleConst.UnitMachineStatus.STANDBY);
		}
		//TODO 基本的な属性はもちろん、具体的な属性も必要。
		public void InitUnitAttributes (MUnit mUnit)
		{
			baseUnitAttribute = new MMOUnitAttribute ();
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.HP] = mUnit.max_hp + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.HP);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.MaxHP] = mUnit.max_hp + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.MaxHP);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.EN] = mUnit.max_mat + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.EN);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.MaxEN] = mUnit.max_mat + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.MaxEN);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.ATK] = mUnit.min_atk + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.ATK);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.DEF] = mUnit.min_def + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.DEF);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.MAT] = mUnit.min_mat + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.MAT);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.MDF] = mUnit.min_mdf + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.MDF);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.HIT] = mUnit.min_hit + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.HIT);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.LUK] = mUnit.min_luk + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.LUK);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.TEC] = mUnit.min_tec + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.TEC);
			baseUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.SPD] = mUnit.min_spd + GetEquipmentAttribute (BattleConst.BattleSkillEffectTypeConst.SPD);
		}
		//TODO 武器システムを待ってる
		int GetEquipmentAttribute (int attributeId)
		{
			return 0;
		}

		public void StartAuto ()
		{
			if (IsPlayer) {
				for (int i = 0; i < aiActions.Count; i++) {
					aiActions [i].IsEnable = true;
				}
			}
		}

		public void EndAuto ()
		{
			if (IsPlayer) {
				for (int i = 0; i < aiActions.Count; i++) {
					aiActions [i].IsEnable = false;
				}
			}
		}

		public void SetStopDistance(float distance){
			stopDistance = distance;
			mAgent.stoppingDistance = distance;
		}

		public void SetDefaultStopDistance(){
			SetStopDistance (BattleConst.MIN_SKILL_RANGE);
		}

		public void OnUpdate ()
		{
			#if NET_SERVER
			unitInfo.transform.position = IntVector3.ToIntVector3 (mTrans.position);
			unitInfo.transform.forward = IntVector3.ToIntVector3 (mTrans.forward);
			#endif

			unitInfo.attribute.currentHP = Mathf.Max(0, GetAttribute (BattleConst.BattleSkillEffectTypeConst.HP));
			unitInfo.attribute.maxHP = GetAttribute (BattleConst.BattleSkillEffectTypeConst.MaxHP);
			if(mmoUnitBuff!=null)
				mmoUnitBuff.OnUpdate ();

			if(target!=null){
				if(target.GetComponent<MMOUnit>().IsDeath){
					target = null;
				}
			}
		}

		public void StandBy ()
		{
			fsm.ChangeStatus (BattleConst.UnitMachineStatus.STANDBY);
		}

		public bool isFollowTarget{ set; get; }

		public void Move (Vector3 targetPos, int onMoveDone)
		{
			this.onMoveDoneStatus = onMoveDone;
			moveTargetPos = targetPos;
			fsm.ChangeStatus (BattleConst.UnitMachineStatus.MOVE);
		}

		public void Move (Transform target, int onMoveDone)
		{
			this.onMoveDoneStatus = onMoveDone;
			this.target = target;
			fsm.ChangeStatus (BattleConst.UnitMachineStatus.MOVE);
		}

		public int GetAttribute (int effectType)
		{
			if(baseUnitAttribute==null){
				baseUnitAttribute = new MMOUnitAttribute ();
			}
			if(additionUnitAttribute==null){
				additionUnitAttribute = new MMOUnitAttribute ();
			}
			return baseUnitAttribute.attributes [effectType] + additionUnitAttribute.attributes [effectType];
		}

		public int GetHP(){
			return GetAttribute (BattleConst.BattleSkillEffectTypeConst.HP);
		}

		public int GetMaxHP(){
			return GetAttribute (BattleConst.BattleSkillEffectTypeConst.MaxHP);
		}

		public int GetATK ()
		{
			return Mathf.RoundToInt (GetAttribute (BattleConst.BattleSkillEffectTypeConst.ATK) * (1 + GetAttribute (BattleConst.BattleSkillEffectTypeConst.ATKMulti) / 10000f));
		}

		public int GetDEF ()
		{
			return Mathf.RoundToInt (GetAttribute (BattleConst.BattleSkillEffectTypeConst.DEF) * (1 + GetAttribute (BattleConst.BattleSkillEffectTypeConst.DEFMulti) / 10000f));
		}

		public void Death ()
		{
			fsm.ChangeStatus (BattleConst.UnitMachineStatus.DEATH);
		}

		public bool IsDeath {
			get { 
				return unitInfo.attribute.currentHP > 0 ? false : true;
			}
		}
		//プレーヤーかどうか
		public bool IsPlayer {
			get {
				return mIsPlayer;
			}
			set{ 
				mIsPlayer = value;
			}
		}
		//プレーヤーに対するしかない。
		public bool IsAuto { get; set; }

		public void AddBuff (BaseBuff baseBuff)
		{
			mmoUnitBuff.AddBuff (baseBuff);
		}

		public void RemoveBuff (BaseBuff baseBuff)
		{
			mmoUnitBuff.RemoveBuff (baseBuff);
		}

		public Vector3 BodyPos{
			get{ 
				return new Vector3 (0, this.mCapsuleCollider.center.y, 0) + mTrans.position;
			}
		}

		public Vector3 Forward{
			get{
				return mTrans.forward;
			}
		}

		public void OnDamage (int casterId)
		{
			if (!IsPlayer) {
				if (this.target == null ) {
					//change the attack target .TODO the 仇恨值计算必要。
					MMOUnit caster = MMOBattleServerManager.Instance.GetUnitByUnitId (casterId);
					//Debug.Log (string.Format("{0} Is damaged by {1}",unitInfo.attribute.unitId,caster.unitInfo.attribute.unitId));
					this.isFollowTarget = true;
					this.target = caster.transform;
					//TODO need Status Run 
//					this.speed = BattleConst.UnitSpeed.NORMAL_SPEED;
					this.moveClip = AnimationConstant.UNIT_ANIMATION_CLIP_RUN;
					this.onMoveDoneStatus = BattleConst.UnitMachineStatus.CAST;
				}
			}
		}

	}

	public class MMOUnitAttribute
	{
		public Dictionary<int,int> attributes;

		public MMOUnitAttribute ()
		{
			attributes = new Dictionary<int, int> ();
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.HP, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MaxHP, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.EN, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MaxEN, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.ATK, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.DEF, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MAT, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MDF, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.HIT, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.LUK, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.TEC, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.SPD, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.ATKMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.DEFMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MATMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MDFMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.HITMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.TECMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.SPDMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.ActionPoint, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.CritOdds, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.DODGE, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.SkillOdds, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.DefenceOdds, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.ElementMuilt, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.DamageReduceMuilt, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.SuckBlood, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.Element, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.UnAttack, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.UnSkill, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.UnTurn, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.CounterMulti, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.DEFIgnore, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.MDFIgnore, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.Dead, 0);
			attributes.Add (BattleConst.BattleSkillEffectTypeConst.ActionPointAdd, 0);
		}
	}
}
