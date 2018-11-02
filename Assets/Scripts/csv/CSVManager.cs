using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BattleFramework.Data;
using CSV;
using System.IO;

namespace MMO
{
	public class CSVManager : SingleMonoBehaviour<CSVManager>
	{
		//	public const string CSV_PATH = @"Assets/CSV";//
		private const string CSV_ROOT = "CSV/";
		private const string CSV_MAP_MONSTER_ROOT = "monsters/";
		private const string CSV_UNIT = "m_unit";
		private const string CSV_SKILL = "m_skill";
		private const string CSV_SKILL_EFFECT_BASE = "m_skill_effect_base";
		private const string CSV_UNIT_SKILL = "m_unit_skill";
		private const string CSV_ELEMENT = "m_battle_element_multi";
		private const string CSV_MAP_MONSTER = "m_map_monster";
		private const string CSV_SKILL_SHOOT = "m_skill_shoot";

		private CsvContext mCsvContext;
		List<MUnit> mUnitList;
		public Dictionary<int,MUnit> mUnitDic;
		List<MSkillEffectBaseCSVStructure> mSkillEffectBaseList;
		public Dictionary<int,MSkillEffectBaseCSVStructure> skillEffectBaseDic;
		List<MSkill> mSkillList;
		public Dictionary<int,MSkill> skillDic;
		List<MUnitSkill> mUnitSkillList;
		public Dictionary<int,MUnitSkill> unitSkillDic;
		public Dictionary<string, Dictionary<string, int>> elementTable;
		public List<MMapMonster> monsterList;
		public Dictionary<int,MMapMonster> monsterDic;
		List<MSkillShoot> mSkillShootList;
		Dictionary<int,MSkillShoot> mSkillShootDic;

		protected override void Awake ()
		{
			base.Awake ();
			StartLoading ();
		}

		public List<TextAsset> monsterTextAssetList;

		void LoadMonsterCSVs ()
		{
			monsterTextAssetList = new List<TextAsset> ();
			TextAsset[] textAssets = Resources.LoadAll<TextAsset> (CSV_ROOT + CSV_MAP_MONSTER_ROOT);
			for (int i = 0; i < textAssets.Length; i++) {
				monsterTextAssetList.Add (textAssets [i]);
			}
		}

		public void SetMonsterCSV(int index){
			if(index>=0 && monsterTextAssetList.Count > index){
				monsterList = CreateCSVList<MMapMonster> (monsterTextAssetList[index].bytes);
				monsterDic = GetDictionary<MMapMonster> (monsterList);
			}
		}

		byte[] GetCSV (string fileName)
		{
			//#if UNITY_EDITOR
			//TODO 因为时间关系暂时用Resources，放到固定的文件夹下面，可以编辑最佳。
			return Resources.Load<TextAsset> (CSV_ROOT + fileName).bytes;
			//#else
			//return ResourcesManager.Ins.GetCSV (fileName);
			//#endif
		}

		void StartLoading ()
		{
			mCsvContext = new CsvContext ();
			LoadUnitTable ();
			LoadSkillShoot ();
			LoadSkillEffectBaseTable ();
			LoadSkillTable ();
			LoadUnitSkillTable ();
//			LoadMapMonsterTable ();
			LoadMonsterCSVs ();
			SetMonsterCSV (0);
		}

		public MUnit GetUnit (int unitId)
		{
			if (mUnitDic.ContainsKey (unitId))
				return mUnitDic [unitId];
			else {
				return mUnitList [0];
			}
		}

		void LoadSkillShoot(){
			mSkillShootList = CreateCSVList<MSkillShoot> (CSV_SKILL_SHOOT);
			mSkillShootDic = GetDictionary<MSkillShoot> (mSkillShootList);
		}

		public MSkillShoot GetSkillShoot(int shootId){
			if(shootId<=0){
				return null;
			}
			if (mSkillShootDic.ContainsKey (shootId)) {
				return mSkillShootDic [shootId];
			} else {
				if (mSkillShootDic.ContainsKey (BattleConst.DEFAULT_SHOOT_ID)) {
					Debug.LogError (string.Format("shoot id: {0} is not exiting.",shootId));
					return mSkillShootDic [BattleConst.DEFAULT_SHOOT_ID];
				}
			}
			Debug.LogError (string.Format("default shoot id: {0} is not exiting.",BattleConst.DEFAULT_SHOOT_ID));
			return null;
		}

		void LoadUnitTable ()
		{
			mUnitList = CreateCSVList<MUnit> (CSV_UNIT);
			mUnitDic = GetDictionary<MUnit> (mUnitList);
		}

		void LoadSkillEffectBaseTable ()
		{
			mSkillEffectBaseList = CreateCSVList<MSkillEffectBaseCSVStructure> (CSV_SKILL_EFFECT_BASE);
			skillEffectBaseDic = GetDictionary<MSkillEffectBaseCSVStructure> (mSkillEffectBaseList);
		}

		void LoadSkillTable ()
		{
			mSkillList = CreateCSVList<MSkill> (CSV_SKILL);
			skillDic = GetDictionary<MSkill> (mSkillList);
		}

		void LoadUnitSkillTable ()
		{
			mUnitSkillList = CreateCSVList<MUnitSkill> (CSV_UNIT_SKILL);
			unitSkillDic = GetDictionary<MUnitSkill> (mUnitSkillList);
			for (int i = 0; i < mUnitSkillList.Count; i++) {
				MUnitSkill mUnitSkill = mUnitSkillList [i];
				if (mUnitDic.ContainsKey (mUnitSkill.unit_id)) {
					MUnit mUnit = mUnitDic [mUnitSkill.unit_id];
					if (mUnit.unitSkillList == null) {
						mUnit.unitSkillList = new List<MUnitSkill> ();
					}
					if (mUnit.skillIdList == null) {
						mUnit.skillIdList = new List<int> ();
					}
					mUnit.skillIdList.Add (mUnitSkill.skill_id);
					mUnit.unitSkillList.Add (mUnitSkill);

					if (mUnit.unitSkillIdList == null) {
						mUnit.unitSkillIdList = new List<int> ();
					}
					mUnit.unitSkillIdList.Add (mUnitSkill.id);
					mUnit.unitSkillIds = mUnit.unitSkillIdList.ToArray ();
				}
			}
			for (int i = 0; i < mUnitList.Count; i++) {
				if (mUnitList [i].skillIdList != null)
					mUnitList [i].skillIds = mUnitList [i].skillIdList.ToArray ();
				else
					mUnitList [i].skillIds = new int[0];
			}
		}


		void LoadElementTable ()
		{
			List<MElementMuilt> mElementMuiltList = CreateCSVList<MElementMuilt> (CSV_ELEMENT);
			InitElements (mElementMuiltList, out elementTable);
		}

		void InitElements (List<MElementMuilt> elementCSVStructures, out Dictionary<string, Dictionary<string, int>> elements)
		{
			elements = new Dictionary<string, Dictionary<string, int>> ();
			//第一列是攻击方，第一行是受击方
			for (int i = 0; i < elementCSVStructures.Count; i++) {
				MElementMuilt elementCSVStructure = elementCSVStructures [i];
				if (!elements.ContainsKey (elementCSVStructure.element))
					elements.Add (elementCSVStructure.element, new Dictionary<string, int> ());
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.red.ToString (), elementCSVStructure.red);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.green.ToString (), elementCSVStructure.green);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.blue.ToString (), elementCSVStructure.blue);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.white.ToString (), elementCSVStructure.white);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.black.ToString (), elementCSVStructure.black);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.none.ToString (), elementCSVStructure.none);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.cut.ToString (), elementCSVStructure.cut);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.destroy.ToString (), elementCSVStructure.destroy);
				elements [elementCSVStructure.element].Add (BattleConst.ElementType.bulge.ToString (), elementCSVStructure.bulge);
			}
		}

		void LoadMapMonsterTable ()
		{
			monsterList = CreateCSVList<MMapMonster> (CSV_MAP_MONSTER);
			monsterDic = GetDictionary<MMapMonster> (monsterList);
		}

		List<T> CreateCSVList<T> (byte[] csvBytes) where T:BaseCSVStructure, new()
		{
			var stream = new MemoryStream (csvBytes);
			var reader = new StreamReader (stream);
			IEnumerable<T> list = mCsvContext.Read<T> (reader);
			return new List<T> (list);
		}

		List<T> CreateCSVList<T> (string csvname) where T:BaseCSVStructure, new()
		{
			var stream = new MemoryStream (GetCSV (csvname));
			var reader = new StreamReader (stream);
			IEnumerable<T> list = mCsvContext.Read<T> (reader);
			return new List<T> (list);
		}

		Dictionary<int,T> GetDictionary<T> (List<T> list) where T : BaseCSVStructure
		{
			Dictionary<int,T> dic = new Dictionary<int, T> ();
			foreach (T t in list) {
				if (!dic.ContainsKey (t.id))
					dic.Add (t.id, t);
			}
			return dic;
		}
	}
}
