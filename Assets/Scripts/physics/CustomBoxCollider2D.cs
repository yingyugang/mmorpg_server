using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
	[RequireComponent(typeof(UnityEngine.BoxCollider2D))]
	public class CustomBoxCollider2D : CustomCollider2D
	{
		public UnityEngine.BoxCollider2D mBoxCollider2D;

		protected override void Awake ()
		{
			base.Awake ();
			mBoxCollider2D = GetComponent<UnityEngine.BoxCollider2D> ();
		}


	}
}
