using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuger  {

	public static void Log(System.Object msg){
		#if UNITY_EDITOR
		Debug.Log (msg.ToString());
		#endif
	}

	public static void LogError(System.Object msg){
		#if UNITY_EDITOR
		Debug.LogError (msg.ToString());
		#endif
	}

	public static void LogWarning(System.Object msg){
		#if UNITY_EDITOR
		Debug.LogWarning(msg.ToString());
		#endif
	}
}
