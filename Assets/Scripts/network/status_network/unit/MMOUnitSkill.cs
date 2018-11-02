using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MMO
{
	[RequireComponent(typeof(MMOUnit))]
	public class MMOUnitSkill : MonoBehaviour
	{

		public List<BaseSkill> activeSkillLists;

		public List<BaseSkill> controllableSkills;
		//Note: key is unitskillstructure.id;
		public Dictionary<int,BaseSkill> unitSkilDic;
		//TODO is the current skill loop.
		public bool isLoopCurrentSkill = false;
		//目前のスキル,TODO need AI to set the current skill。
		public BaseSkill currentSkill;
		//常に普通な攻撃、近距離は１８に固定する、遠距離は２１に固定する。
		//自由にため、unit_skill.csvで第一スキルに設定する。
		public BaseSkill defaultSkill;

		MMOUnit mUnit;

		void Awake(){
			mUnit = GetComponent<MMOUnit> ();
			InitUnitSkills ();
		}

		void AddSkill(BaseSkill baseSkill){
			activeSkillLists.Add (baseSkill);
			//TODO need to check the skill controllable.
			controllableSkills.Add (baseSkill);
			if(!unitSkilDic.ContainsKey(baseSkill.unitSkillCSVStructure.id))
				unitSkilDic.Add (baseSkill.unitSkillCSVStructure.id,baseSkill);
   		}

		public void Reset(){
			InitUnitSkills ();
		}

		public void InitUnitSkills ()
		{
			int[] unitSkillIds = mUnit.unitInfo.unitSkillIds;
			activeSkillLists = new List<BaseSkill> ();
			controllableSkills = new List<BaseSkill> ();
			unitSkilDic = new Dictionary<int, BaseSkill> ();
			for (int i = 0; i < unitSkillIds.Length; i++) {
				MUnitSkill unitSkillCSVStructure = CSVManager.Instance.unitSkillDic[unitSkillIds[i]];
				MSkill skillCSVStructure;
				if (CSVManager.Instance.skillDic.TryGetValue (unitSkillCSVStructure.skill_id, out skillCSVStructure)) {
					BaseSkill baseSkill = LoadSkill (skillCSVStructure,unitSkillCSVStructure);
					AddSkill (baseSkill);
				}
			}
			defaultSkill = controllableSkills [0];
			currentSkill = controllableSkills [0];
		}

		BaseSkill LoadSkill (MSkill skillCSVStructure,MUnitSkill unitSkillCSVStructure)
		{
			BaseSkill baseSkill = InitBaseSkill (skillCSVStructure,unitSkillCSVStructure, mUnit);
			if (skillCSVStructure.subSkills != null && skillCSVStructure.subSkills.Count > 0) {
				for (int j = 0; j < skillCSVStructure.subSkills.Count; j++) {
					if (baseSkill.subSkills == null) {
						baseSkill.subSkills = new List<BaseSkill> ();
					}
					MSkill subSkillCSVStructure = null;
					if (CSVManager.Instance.skillDic.TryGetValue (skillCSVStructure.subSkills [j].subSkillId, out subSkillCSVStructure)) {
						BaseSkill subSkill = InitBaseSkill (subSkillCSVStructure,unitSkillCSVStructure, mUnit);
						baseSkill.subSkills.Add (subSkill);
						subSkill.superSkill = baseSkill;
					}
				}
			}
			return baseSkill;
		}
		//TODO
		BaseSkill InitBaseSkill (MSkill skillCSVStructure,MUnitSkill unitSkillCSVStructure, MMOUnit unit)
		{
			BaseSkill baseSkill = null;
			switch(skillCSVStructure.effect_type){
			case BattleConst.BattleSkillEffectTypeConst.HP:
				baseSkill = new DamageSkill();
				break;
			case BattleConst.BattleSkillEffectTypeConst.Heal:
				baseSkill = new HealSkill ();
				break;
			case BattleConst.BattleSkillEffectTypeConst.DebuffClear:
				baseSkill = new DebuffClearSkill ();
				break;
			case BattleConst.BattleSkillEffectTypeConst.BuffClear:
				baseSkill = new BuffClearSkill ();
				break;
			case BattleConst.BattleSkillEffectTypeConst.StatusClear:
				baseSkill = new StatusClearSkill ();
				break;
			default:
				baseSkill = new AttributeEnhanceSkill ();
				break;
			}
			baseSkill.Init (unit,skillCSVStructure,unitSkillCSVStructure);
			baseSkill.OnAwake ();
			return baseSkill;
		}
		//Cast the skill.Handled excute the castaction.
		public void Cast(int unitSkillId){
			CastAction castAction = new CastAction ();
			castAction.mmoUnit = mUnit;
			castAction.GO = gameObject;
			if (CSVManager.Instance.unitSkillDic.ContainsKey (unitSkillId)) {
				int skillId = CSVManager.Instance.unitSkillDic [unitSkillId].id;
				currentSkill = this.unitSkilDic [skillId];
				//To get the current skill;
				castAction.OnEnter ();
				StartCoroutine (_Cast(castAction));
			}
		}

		IEnumerator _Cast(CastAction castAction){
			while(!currentSkill.IsEnd || !currentSkill.IsActive){
				castAction.OnUpdate ();
				yield return null;
			}
			castAction.OnExit ();
		}

	}
}
