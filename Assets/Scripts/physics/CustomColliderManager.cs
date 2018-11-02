using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
	public class CustomColliderManager : SingleMonoBehaviour<CustomColliderManager>
	{

		public List<CustomCollider2D> colliders;

		protected override void Awake ()
		{
			base.Awake ();
		}

		void FixedUpdate ()
		{
			//Collider Moving.
			for(int i = 0; i < colliders.Count; i++){
				CustomCollider2D col = colliders [i];
				col.transform.position += (col.direct * Time.fixedDeltaTime);
			}

			//Check Collision Enter.
			for (int i = 0; i < colliders.Count; i++) {
				CustomCollider2D col = colliders [i];
				col.CheckCollisionEnter ();
			}

			//Check Collision Exit.
			for (int i = 0; i < colliders.Count; i++) {
				CustomCollider2D col = colliders [i];
				col.CheckCollisionExit ();
			}
		}
	}
}
