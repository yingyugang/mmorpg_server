using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
	public enum Collider2DType
	{
		CircleCollider2D,
		BoxCollider2D
	};

	public abstract class CustomCollider2D : MonoBehaviour
	{
		public Vector3 direct;
		public UnityEngine.Collider2D col2D;
		//contants效率高（所花时间是List的1/700，相差700毫秒，对性能影响较大）（主要原因,optimized），Remove效率比List高（次要原因）。（10000个int）
		protected HashSet<CustomCollider2D> exsitingColliders;
		//循环效率高,且不产生垃圾回收（主要原因,optimized）。ADD效率比HashSet高（次要原因）。总体来说都是个位数毫米的差距。（10000个int）
		protected List<CustomCollider2D> exsitingColliderList;
		//为了效率考虑，不直接用System.Type类型。optimized
		[HideInInspector]
		public Collider2DType type;

		protected virtual void Awake ()
		{
			CustomColliderManager.Instance.colliders.Add (this);
			Debug.Log (CustomColliderManager.Instance.colliders.Count);
			exsitingColliders = new HashSet<CustomCollider2D> ();
			exsitingColliderList = new List<CustomCollider2D> ();
			col2D = GetComponent<UnityEngine.Collider2D> ();
			switch(this.GetType ().ToString()){
			case "CustomPhysics2D.CustomCircleCollider2D":
				type = Collider2DType.CircleCollider2D;
				break;
			case "CustomPhysics2D.CustomBoxCollider2D":
				type = Collider2DType.BoxCollider2D;
				break;
			}
		}

		public virtual Vector2 GetCenter ()
		{
			return (Vector2)transform.position + col2D.offset;
		}

		public virtual bool IsColliderExisting (CustomCollider2D other)
		{
			return exsitingColliders.Contains (other);
		}

		public virtual void OnColliderEnter (CustomCollider2D other,Vector2 normal)
		{
			if (!IsColliderExisting (other)) {
				exsitingColliders.Add (other);
				exsitingColliderList.Add (other);
				Debug.Log (string.Format("{0} entered!",other));
			}
		}

		public virtual void OnColliderExisting (CustomCollider2D other)
		{
			for (int i = 0; i < exsitingColliderList.Count; i++) {
				CustomCollider2D coll = exsitingColliderList [i];
			}
		}

		public virtual void OnColliderExit (CustomCollider2D other)
		{
			if (IsColliderExisting (other)) {
				exsitingColliders.Remove (other);
				exsitingColliderList.Remove (other);
				Debug.Log (string.Format("{0} exit!",other));
			}
		}

		public virtual bool CheckCollision (CustomCollider2D other,out Vector2 normal)
		{
			normal = Vector2.zero;
			return false;
		}

		public virtual void CheckCollisionEnter(){
		
		}

		public virtual void CheckCollisionExit(){
			
		}

	}
}
