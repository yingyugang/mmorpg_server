using System;
using UnityEngine;
using System.Collections;
using BattleFramework;
using System.Collections.Generic;

namespace MMO
{
	public class PlayerActionManager : SingleMonoBehaviour<PlayerActionManager>
	{
		protected override void Awake ()
		{
			base.Awake ();
		}
		//TODO need real action logic.
		//TODO need player attack logic.
		//the target will  always be the client selected target.
		//これはプレーヤーをスキルを実行する関数。
		//AIと同じロジックになる。
		public void DoSkill (MMOUnit caster, MMOUnit target, StatusInfo action)
		{
			if (caster.mmoUnitSkill.unitSkilDic.ContainsKey (action.actionId)) {
				caster.targetStatusInfo = action;
				BaseSkill baseSkill = caster.mmoUnitSkill.unitSkilDic [action.actionId];
				if (baseSkill.IsInPlayerSkillCooldown) {
					if(MMOBattleServerManager.Instance.debug)
						Debug.LogError (string.Format ("{0} is in cooldown.", caster.unitInfo.attribute.unitId));
					return;
				}
				if (target != null) {
					float sqrRange = baseSkill.skillCSVStructure.range * baseSkill.skillCSVStructure.range;
					if (sqrRange + BattleConst.SKILL_CHECK_DISTANCE_OFFSET < (caster.transform.position - target.transform.position).sqrMagnitude) {
						return;
					} else {
						caster.target = target.transform;
					}
				}
				caster.mmoUnitSkill.currentSkill = baseSkill;
				caster.mmoUnitSkill.Cast (action.actionId);
			}
		}

		public void Damage (MMOUnit target, int damage)
		{
			target.additionUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.HP] -= damage;
			if (target.GetAttribute (BattleConst.BattleSkillEffectTypeConst.HP) <= 0) {
				target.Death ();
			} 
		}

		public void Health (MMOUnit target, int health)
		{
			health = Mathf.Min (target.unitInfo.attribute.maxHP - target.unitInfo.attribute.currentHP,health);
			target.additionUnitAttribute.attributes [BattleConst.BattleSkillEffectTypeConst.HP] += health;
		}

		public bool IsBuff (SkillEffectCSVStructure skillEffectCSVStructure)
		{
			bool isBuff = false;
			if (skillEffectCSVStructure.effect_type < 3000 && skillEffectCSVStructure.effect_value > 0) {
				isBuff = true;
			}
			return isBuff;
		}

		public bool IsDebuff (SkillEffectCSVStructure skillEffectCSVStructure)
		{
			bool isDebuff = false;
			if (skillEffectCSVStructure.effect_type < 3000 && skillEffectCSVStructure.effect_value < 0) {
				isDebuff = true;
			}
			return isDebuff;
		}

		public bool IsStatus (SkillEffectCSVStructure skillEffectCSVStructure)
		{
			bool isStatus = false;
			if (skillEffectCSVStructure.effect_type >= 3000) {
				isStatus = true;
			}
			return isStatus;
		}
		//ヒットするメッセージを起く
		public HitInfo GetHittedInfos (int casterId, int damage, int hitObjectId, Vector3 pos, float circleRadiu)
		{
			Collider[] colls = Physics.OverlapSphere (pos, circleRadiu, 1 << LayerConstant.LAYER_UNIT);
			HitInfo hitInfo = new HitInfo ();
			hitInfo.hitOriginPosition = IntVector3.ToIntVector3 (pos);
			hitInfo.casterId = casterId;
			hitInfo.hitIds = new int[colls.Length];
			hitInfo.nums = new int[colls.Length];
			hitInfo.hitObjectIds = new int[1];
			hitInfo.hitObjectIds [0] = hitObjectId;
			hitInfo.hitPositions = new IntVector3[1];
			hitInfo.hitPositions [0] = IntVector3.ToIntVector3 (pos);
			for (int i = 0; i < colls.Length; i++) {
				MMOUnit mmoUnit = colls [i].GetComponent<MMOUnit> ();
				if (mmoUnit.unitInfo.attribute.unitId != casterId) {
					hitInfo.nums [i] = damage;
					hitInfo.hitIds [i] = mmoUnit.unitInfo.attribute.unitId;
					Damage (mmoUnit, damage);
					mmoUnit.OnDamage (casterId);
				}
			}
			Debug.Log ("colls.Length:" + colls.Length);
			return hitInfo;
		}
		//TODO
		public int GetDamage (MMOUnit attacker, MMOUnit target, BaseSkill baseSkill)
		{
			return CalculateDamage (attacker, target, baseSkill);
		}
		//calculate damages.
		int CalculateDamage (MMOUnit mmoUnit, MMOUnit target, BaseSkill baseSkill)
		{
			//1.whether crit.
			bool isCrit = GetCrit (target, baseSkill);
			//2.relation damage multi.
			bool isRelation = false;
			int relationAll = GetRelation (mmoUnit, target);
			if (relationAll > 0)
				isRelation = true;
			//3.not have.
			bool isDefence = false;
			//4.whether miss.
			bool isMiss = GetMiss (target);
			//5.ignore defense.
			bool isIgnoreDef = RandomUtility.DefaultRange () < baseSkill.GetUnitAndSkillAttribute (BattleConst.BattleSkillEffectTypeConst.DEFIgnore) ? true : false;
			//6.whether die immediatly.
			bool isDead = RandomUtility.DefaultRange () < baseSkill.GetUnitAndSkillAttribute (BattleConst.BattleSkillEffectTypeConst.Dead) ? true : false;
			//7.real damage.
			int realDamage = GetRealDamage (mmoUnit, target, isRelation, (relationAll + 100) / 100f, isCrit, isDefence, isMiss, GetSkillValue (baseSkill.skillCSVStructure) / 10000f, isIgnoreDef);
			int targetHp = target.GetAttribute (BattleConst.BattleSkillEffectTypeConst.HP);
			//8.if die immediatly
			if (isDead && (targetHp > realDamage)) {
				realDamage = targetHp - 1;
			}
			//9.真实回血
			int suckBloodPercent = baseSkill.GetUnitAndSkillAttribute (BattleConst.BattleSkillEffectTypeConst.SuckBlood);
			int suckBlood = Mathf.RoundToInt (realDamage * suckBloodPercent / 10000f);
			//TDDO 残しておく、高貴なパッジンに実行する。
//			Hashtable param = new Hashtable ();
//			param.Add (BattleConst.CalculationResultType.IsCrit, isCrit);
//			param.Add (BattleConst.CalculationResultType.IsDefence, isDefence);
//			param.Add (BattleConst.CalculationResultType.IsMiss, isMiss);
//			param.Add (BattleConst.CalculationResultType.IsRelation, isRelation);
//			param.Add (BattleConst.CalculationResultType.IsDefIgnore, isIgnoreDef);
//			param.Add (BattleConst.CalculationResultType.IsDead, isDead);
//			param.Add (BattleConst.CalculationResultType.RealDamage, realDamage);
//			param.Add (BattleConst.CalculationResultType.RealHealth, suckBlood);
			return realDamage;
		}
		//default damage calculation.
		int GetRealDamage (MMOUnit attacker, MMOUnit target, bool isRelation, float relationMuilt, bool isCrit, bool isDenfence, bool isMiss, float skillMuilt, bool isDefIgnore)
		{
			if (isMiss)
				return 0;
			float critMulti = isCrit ? 1 : 0;
			float defenceMulti = isDenfence ? 0.5f : 0;
			int atk = attacker.GetATK ();
			int def = target.GetDEF ();
			if (isDefIgnore)
				def = 0;
			int realDamage = Mathf.Max (1, Mathf.RoundToInt ((atk - def) * skillMuilt * (1 + relationMuilt) * (1 + critMulti) * (1 - defenceMulti))); 
			return realDamage;
		}
		//whether cirt.
		bool GetCrit (MMOUnit target, BaseSkill baseSkill)
		{
			bool isCrit;
			int tec = Mathf.RoundToInt (baseSkill.GetUnitAndSkillAttribute (BattleConst.BattleSkillEffectTypeConst.TEC) * (1 + baseSkill.GetUnitAndSkillAttribute (BattleConst.BattleSkillEffectTypeConst.TECMulti) / BattleConst.CRIT_RATE));
			int targetTec = Mathf.RoundToInt (target.GetAttribute (BattleConst.BattleSkillEffectTypeConst.TEC) * (1 + target.GetAttribute (BattleConst.BattleSkillEffectTypeConst.TECMulti) / BattleConst.CRIT_RATE));
			int critPercent = Mathf.RoundToInt (BattleConst.CRIT_DEFAULT * tec / targetTec);
			isCrit = UnityEngine.Random.Range (0, 100) < critPercent ? true : false;
			return isCrit;
		}
		//whether miss.
		bool GetMiss (MMOUnit target)
		{
			bool isMiss;
			int dodge = Mathf.RoundToInt (target.GetAttribute (BattleConst.BattleSkillEffectTypeConst.DODGE));
			isMiss = UnityEngine.Random.Range (0, 100) < dodge;
			return isMiss;
		}
		//get element damage relation.
		int GetRelation (MMOUnit attacker, MMOUnit target)
		{
			int relationAll = 0;
			Dictionary<string, Dictionary<string, int>> elements = CSVManager.Instance.elementTable;
			for (int i = 0; i < attacker.elements.Count; i++) {
				string unitEle = ((BattleConst.ElementType)attacker.elements [i]).ToString ();
				for (int j = 0; j < target.elements.Count; j++) {
					string targetEle = ((BattleConst.ElementType)target.elements [j]).ToString ();
					if (elements.ContainsKey (unitEle)) {
						if (elements [unitEle].ContainsKey (targetEle)) {
							relationAll += elements [unitEle] [targetEle] - 100;
						}
					}
				}
			}
			return relationAll;
		}
		//TODO
		public int GetHealth (MMOUnit attacker, MMOUnit target, MSkill skillStructure)
		{
			int atk = attacker.GetATK ();
			atk = Mathf.RoundToInt( BattleFramework.RandomUtility.Range (skillStructure.effect_value_min, skillStructure.effect_value_max) / 10000f * atk);
			return atk;
		}
		//Get the skill value.
		public int GetSkillValue (MSkill skill)
		{
			return BattleFramework.RandomUtility.Range (skill.effect_value_min, skill.effect_value_max);
		}
			
	}
}

