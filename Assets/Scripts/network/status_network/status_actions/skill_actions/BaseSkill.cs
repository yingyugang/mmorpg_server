using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	//TODO 召唤物スキルをしよう。
	public class BaseSkill : UnitBaseAction
	{

		//使えるかどうか
		public bool isUseAble = true;
		//もっと多く使える回数
		public int maxCastRound;
		//the cool down time of the skill.
		public float coolDownTime;
		//it's super skill,if it is null means is a super skill.
		public BaseSkill superSkill;
		//it's sub skills.
		public List<BaseSkill> subSkills;
		//the skill attribute what enhance by this superskill.
		public MMOUnitAttribute skillAttribute;

		public MSkill skillCSVStructure;

		public MUnitSkill unitSkillCSVStructure;

		float mActionTime;

		float mEndTime;

		float mNextCooldownTime;

		float mNextScaleableTime;
		//効率化のために
		Collider[] mColls = new Collider[BattleConst.MAX_SCANE_ABLE_COUNT];
		RaycastHit[] mRayHits = new RaycastHit[BattleConst.MAX_SCANE_ABLE_COUNT];

		protected bool mIsFriends;

		bool mIsActived;

		public override void OnEnter ()
		{
			base.OnEnter ();
			skillAttribute = new MMOUnitAttribute ();
			//TODO エネルギーを削減
			//TODO to tell client to update En.
			if (superSkill == null) {
				mmoUnit.additionUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.EN] -= skillCSVStructure.en_cost;
				if (mmoUnit.GetAttribute (BattleConst.BattleSkillEffectTypeConst.EN) < 0) {
					mmoUnit.additionUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.EN] -= mmoUnit.GetAttribute (BattleConst.BattleSkillEffectTypeConst.EN);
				}
			}
		
			mEndTime = Time.time + unitSkillCSVStructure.anim_length;
			mActionTime = Time.time + (unitSkillCSVStructure.anim_action_point / 100f) * unitSkillCSVStructure.anim_length;
			mNextCooldownTime = Time.time + skillCSVStructure.cooldown + unitSkillCSVStructure.anim_length;
			mIsActived = false;
		}

		public override void OnUpdate ()
		{
			base.OnUpdate ();
			if (mActionTime < Time.time) {
				OnAction ();
				mActionTime = Mathf.Infinity;
			}
		}

		public bool IsEnd {
			get {
				return mEndTime < Time.time;
			}
		}

		public bool IsActive{
			get{ 
				return mIsActived;
			}
		}

		public bool IsInCooldown {
			get {
				return mNextCooldownTime > Time.time;
			}
		}

		//client skill cooldown時間がも少し減す
		public bool IsInPlayerSkillCooldown {
			get {
				return mNextCooldownTime + BattleConst.COOLDOWN_OFFSET > Time.time;
			}
		}

		public virtual void OnAction ()
		{
			if (IsMainSkill ()) {
				//有目标的情况
				//TODO 无目标的情况需要处理
				int impactCount = skillCSVStructure.impact_count;
				if (impactCount == 0) {
					impactCount = 1;
					Debug.LogError (string.Format ("{0} skillCSVStructure.impact_count is 0", skillCSVStructure.id));
				}
				Vector3 forward = this.mmoUnit.transform.forward;
				forward = new Vector3 (forward.x, 0, forward.z).normalized;
				List<MMOUnit> targets = new List<MMOUnit> ();
				MMOUnit mainTarget = null;
				if (mmoUnit.target != null) {
					mainTarget = mmoUnit.target.GetComponent<MMOUnit> ();
					targets.Add (mainTarget);
					impactCount--;
				}
				if (mColls.Length < skillCSVStructure.impact_count)
					mColls = new Collider[skillCSVStructure.impact_count];
				//枪械类，由于子弹飞行太快时间太短，视觉上几乎不能判断，并且如果做碰撞检测的话，性能消耗极高，所以直接造成伤害，客户端只做模拟的弹道演出，和开火特效演出
				//或者把子弹长度改为原来的数十倍的碰撞检测，然后按照原来的频率进行碰撞检测，这样有没有必要，需要检讨。
				if (skillCSVStructure.impact_type == 1) {
					Vector3 checkForward = mmoUnit.Forward;
					Vector3 checkPosition = mmoUnit.BodyPos;
					//由客户端控制的射击
					if (mmoUnit.targetStatusInfo != null) {
						//加上unit的视角。
						checkForward = IntVector3.ToVector3 (mmoUnit.targetStatusInfo.forward);
						checkPosition = IntVector3.ToVector3 (mmoUnit.targetStatusInfo.position);
					}
					mmoUnit.UnCollider ();
					int count = Physics.RaycastNonAlloc (checkPosition, checkForward, mRayHits, skillCSVStructure.range, 1 << LayerConstant.LAYER_UNIT);
					mmoUnit.EnCollider ();
					for (int i = 0; i < count; i++) {
						MMOUnit unit = mRayHits [i].collider.GetComponent<MMOUnit> ();
						if (!unit.IsDeath) {
							if (!mIsFriends && this.mmoUnit.camp != unit.camp) {
								targets.Add (unit);
							} else if (mIsFriends && this.mmoUnit.camp == unit.camp) {
								targets.Add (unit);
							}
						}
					}
					if (skillCSVStructure.impact_type == 1) {
						if (mmoUnit.targetStatusInfo != null) {
							ShootInfo shootInfo = new ShootInfo ();
							shootInfo.unitSkillId = -1;
							shootInfo.position = mmoUnit.targetStatusInfo.position;
							shootInfo.forward = mmoUnit.targetStatusInfo.forward;
							shootInfo.casterId = mmoUnit.unitInfo.attribute.unitId;
							MMOBattleServerManager.Instance.AddShootInfo (shootInfo);
						}
					}
				} else {
					int hitCount = Physics.OverlapSphereNonAlloc (mmoUnit.BodyPos, mmoUnit.unitCSVStructure.width + skillCSVStructure.range, mColls, 1 << LayerConstant.LAYER_UNIT);
					for (int i = 0; i < hitCount; i++) {
						if (targets.Count >= impactCount) {
							break;
						}
						MMOUnit unit = mColls [i].GetComponent<MMOUnit> ();
						if (mainTarget != null && mainTarget == unit) {
							continue;
						}
						if (unit.IsDeath)
							continue;
						if (mIsFriends && unit.camp != this.mmoUnit.camp) {
							continue;
						}
						if (!mIsFriends && unit.camp == this.mmoUnit.camp) {
							continue;
						}
						//攻撃者前の数度以内。
						if (skillCSVStructure.impact_check_radiu < 360 && skillCSVStructure.impact_check_radiu > 0) {
							//コストがある
							Vector3 direct = new Vector3 (unit.transform.position.x - this.mmoUnit.transform.position.x, 0, unit.transform.position.z - this.mmoUnit.transform.position.z);
							direct = direct.normalized;
							float acos = Vector3.Dot (direct, forward);
							if (acos > 1.0f)
								acos = 1.0f;
							else if (acos < -1.0f)
								acos = -1.0f;
							float angle = Mathf.Acos (acos);
							if (acos == 1 || acos == -1) {
								angle = 0;
							}
							angle = angle * 180 / Mathf.PI;
							if (Mathf.Abs (angle) < skillCSVStructure.impact_check_radiu / 2f) {
								targets.Add (unit);
							}
						} else {
							targets.Add (unit);
						}
					}
				}
				for (int j = 0; j < targets.Count; j++) {
					MMOUnit target = targets [j];
					if (skillCSVStructure.skillShoot != null) {
						ShootObject shootObj = null;
						switch (skillCSVStructure.skillShoot.shoot_type) {
						case BattleConst.BattleShoot.LINE:
							shootObj = GameObject.CreatePrimitive (PrimitiveType.Sphere).AddComponent<ShootLineObject> ();
							break;
						default:
							shootObj = GameObject.CreatePrimitive (PrimitiveType.Sphere).AddComponent<ShootProjectileObject> ();
							break;
						}
						shootObj.gameObject.hideFlags = HideFlags.HideInHierarchy;
						shootObj.speed = skillCSVStructure.skillShoot.shoot_move_speed;
						shootObj.baseHits = GetBaseHits ();
						shootObj.transform.position = mmoUnit.transform.position;
						shootObj.isFriends = mIsFriends;
						shootObj.Shoot (this.mmoUnit, target, Vector3.zero);
						MMOBattleServerManager.Instance.AddShootInfo (mmoUnit.unitInfo.attribute.unitId, this.unitSkillCSVStructure.id, target.unitInfo.attribute.unitId, IntVector3.Zero ());
//						Vector3 targetPos = MMOBattleServerManager.Instance.GetTerrainPos (mmoUnit.transform.position + mmoUnit.transform.forward *  skillCSVStructure.range);
//						shootObj.Shoot (this.mmoUnit, targetPos, Vector3.zero);
					} else {
						List<BaseHit> baseHits = GetBaseHits ();
						for (int i = 0; i < baseHits.Count; i++) {
							baseHits [i].OnHit (mmoUnit.transform.position, mIsFriends, target);
						}
					}
				}
				mIsActived = true;
			}
		}

		protected MMOUnit GetCloseUnit (List<MMOUnit> mmoUnits)
		{
			float distance = Mathf.Infinity;
			MMOUnit targetUnit = null;
			for (int i = 0; i < mmoUnits.Count; i++) {
				float sqrDistance = Vector3.SqrMagnitude (mmoUnits [i].transform.position - GO.transform.position);
				if (sqrDistance < distance) {
					targetUnit = mmoUnits [i];
					distance = sqrDistance;
				}
			}
			return targetUnit;
		}

		protected MMOUnit GetInjuredUnit (List<MMOUnit> mmoUnits)
		{
			if (mmoUnits == null)
				return null;
			float minHealth = Mathf.Infinity;
			MMOUnit targetUnit = null;
			for (int i = 0; i < mmoUnits.Count; i++) {
				int health = mmoUnits [i].unitInfo.attribute.currentHP;
				int maxHealth = mmoUnits [i].unitInfo.attribute.maxHP;
				if (health < maxHealth && health < minHealth) {
					targetUnit = mmoUnits [i];
					minHealth = health;
				}
			}
			return targetUnit;
		}

		public virtual bool IsSkillable (MMOUnit mmoUnit)
		{
			return true;
		}

		public virtual List<MMOUnit> GetSkillableUnits ()
		{
			return null;
		}

		protected bool IsScaneable ()
		{
			return mNextScaleableTime < Time.time;
		}

		protected List<MMOUnit> GetLineSkillableUnits (bool isFriend)
		{
			if (mNextScaleableTime > Time.time)
				return null;
			mNextScaleableTime = Time.time + BattleConst.DEFAULT_AI_SKILL_SCANE_INTERVAL;
			List<MMOUnit> targetUnits = new List<MMOUnit> ();
			int count = Physics.RaycastNonAlloc (mmoUnit.BodyPos, mmoUnit.Forward, mRayHits, skillCSVStructure.range, 1 << LayerConstant.LAYER_UNIT);
			for (int i = 0; i < count; i++) {
				MMOUnit unit = mRayHits [i].collider.GetComponent<MMOUnit> ();
				if (!unit.IsDeath) {
					if (!isFriend && this.mmoUnit.camp != unit.camp) {
						targetUnits.Add (unit);
					} else if (isFriend && this.mmoUnit.camp == unit.camp) {
						targetUnits.Add (unit);
					}
				}
			}
			return targetUnits;
		}

		protected List<MMOUnit> GetSkillableUnits (bool isFriend)
		{
			if (mNextScaleableTime > Time.time)
				return null;
			mNextScaleableTime = Time.time + BattleConst.DEFAULT_AI_SKILL_SCANE_INTERVAL;
			List<MMOUnit> targetUnits = new List<MMOUnit> ();
			int count = Physics.OverlapSphereNonAlloc (GO.transform.position, mmoUnit.unitCSVStructure.view + mmoUnit.unitCSVStructure.width, mColls, 1 << LayerConstant.LAYER_UNIT);
			count = Mathf.Min (count, mColls.Length);
			for (int i = 0; i < count; i++) {
				MMOUnit unit = mColls [i].GetComponent<MMOUnit> ();
				if (!unit.IsDeath) {
					if (!isFriend && this.mmoUnit.camp != unit.camp) {
						targetUnits.Add (unit);
					} else if (isFriend && this.mmoUnit.camp == unit.camp) {
						targetUnits.Add (unit);
					}
				}
			}
			return targetUnits;
		}

		public virtual void Init (MMOUnit unit, MSkill skillCSVStructure, MUnitSkill unitSkillCSVStructure)
		{
			this.mmoUnit = unit;
			this.GO = unit.gameObject;
			this.skillCSVStructure = skillCSVStructure;
			this.unitSkillCSVStructure = unitSkillCSVStructure;
		}

		public int GetUnitAndSkillAttribute (int effectType)
		{
			return mmoUnit.GetAttribute (effectType) + this.GetSuperSkill ().skillAttribute.attributes [effectType];
		}

		protected virtual int GetSkillValue ()
		{
			return BattleFramework.RandomUtility.Range (skillCSVStructure.effect_value_min, skillCSVStructure.effect_value_max);
		}

		public bool IsMainSkill ()
		{
			return superSkill == null ? true : false;
		}

		public BaseSkill GetSuperSkill ()
		{
			return superSkill == null ? this : superSkill;
		}

		protected List<BaseHit> GetBaseHits ()
		{
			List<BaseHit> baseHits = new List<BaseHit> ();
			BaseHit baseHit = GetBaseHit ();
			baseHits.Add (baseHit);
			for (int i = 0; i < subSkills.Count; i++) {
				baseHits.Add (subSkills [i].GetBaseHit ());
			}
			return baseHits;
		}

		protected virtual BaseHit GetBaseHit ()
		{
			BaseHit baseHit = new DamageHit ();
			baseHit.Init (this.mmoUnit, this);
			return baseHit;
		}

	}
}
