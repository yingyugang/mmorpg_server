using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BattleFramework
{
	public static class RandomUtility
	{

		static List<string> vList;
		static System.Random mBattleRandom;

		static System.Random mBattleScanRandom;

		public static float Range (float min, float max)
		{
			double d = mBattleRandom.NextDouble ();
			float v = Mathf.Lerp (min, max, (float)d);
			vList.Add (v.ToString ());
			return v;
		}

		public static int DefaultRange(){
			return Range(0,10000);
		}

		public static int Range (int min, int max)
		{
			int v = mBattleRandom.Next (min, max);
			vList.Add (v.ToString ());
			return v;
		}

		public static int ScanRange(int min, int max){
			int v = mBattleScanRandom.Next (min, max);
//			vList.Add (v.ToString ());
			return v;
		}

		public static void SetRandomSeed (int seed)
		{
			vList = new List<string> ();
			mBattleRandom = new System.Random (seed);
			mBattleScanRandom = new System.Random (seed);
		}

		public static void DebugSeeds ()
		{
			string strs = "";
			foreach (string str in vList) {
				strs += str + "|";
			}
			Debug.Log (strs);
		}

	}
}
