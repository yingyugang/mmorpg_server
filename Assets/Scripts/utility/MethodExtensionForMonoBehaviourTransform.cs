using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class MethodExtensionForMonoBehaviourTransform
{
	static public T GetOrAddComponent<T> (this GameObject comp) where T: Component
	{
		T result = comp.GetComponent<T> ();
		if (result == null) {
			result = comp.AddComponent<T> ();
		}
		return result;
	}
}
