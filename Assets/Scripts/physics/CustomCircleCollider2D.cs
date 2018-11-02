using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
	[RequireComponent (typeof(UnityEngine.CircleCollider2D))]
	public class CustomCircleCollider2D : CustomCollider2D
	{
		public UnityEngine.CircleCollider2D circle;

		protected override void Awake ()
		{
			base.Awake ();
			circle = GetComponent<UnityEngine.CircleCollider2D> ();
		}

		public override bool CheckCollision (CustomCollider2D other,out Vector2 normal)
		{
			bool isColl = false;
			normal = Vector2.zero;
			switch (other.type) {
			case Collider2DType.CircleCollider2D:
				isColl = CustomCollisionUtility.IsCircleAndCircle (this, (CustomCircleCollider2D)other);
				break;
			case Collider2DType.BoxCollider2D:
				Vector2 hit;
				isColl = CustomCollisionUtility.IsCircleAndBox (this, (CustomBoxCollider2D)other, out hit, out normal);
				break;
			}
			return isColl;
		}

		public override void CheckCollisionEnter ()
		{
			for (int i = 0; i < CustomColliderManager.Instance.colliders.Count; i++) {
				CustomCollider2D other = CustomColliderManager.Instance.colliders [i];
				if (other != this) {
					Vector2 normal = Vector2.zero;
					if (CheckCollision (other,out normal)) {
						OnColliderEnter (other,normal);
					}
				}
			}
		}

		public override void CheckCollisionExit ()
		{
			for (int i = 0; i < this.exsitingColliderList.Count; i++) {
				CustomCollider2D other = exsitingColliderList [i];
				Vector2 normal = Vector2.zero;
				if (!CheckCollision (other,out normal)) {
					OnColliderExit (other);
				}
			}
		}

	}

}
