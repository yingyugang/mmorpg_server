using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics2D
{
	public static class CustomCollisionUtility
	{

		//optimized.
		public static bool IsCircleAndCircle (CustomCircleCollider2D circle0, CustomCircleCollider2D circle1)
		{
			Vector2 pos0 = circle0.GetCenter ();
			Vector2 pos1 = circle1.GetCenter ();
			float radius = circle0.circle.radius + circle1.circle.radius;
			radius *= radius;
			float x = pos1.x - pos0.y;
			float y = pos1.y - pos0.y;
			float distance = x * x + y * y;
			return distance <= radius;
		}

		//optimized.
		//圆心到每条边到垂线交点
		public static bool IsCircleAndBox (CustomPhysics2D.CustomCircleCollider2D circle, CustomPhysics2D.CustomBoxCollider2D box, out Vector2 hit,out Vector2 normal )
		{
			//TODO 需要经过旋转
			Vector2 min = (Vector2)box.mBoxCollider2D.bounds.min;// + box.GetCenter ();
			Vector2 max = (Vector2)box.mBoxCollider2D.bounds.max;// + box.GetCenter ();
			normal = Vector2.zero;
			hit = Vector2.zero;
			Vector2 verticalPos = MathUtility.GetVerticalForPointAndLine (circle.GetCenter (), min, new Vector2 (min.x, max.y));
			if (MathUtility.IsPointOnLineSegmentByMagnitude(verticalPos,min, new Vector2 (min.x, max.y)) && (verticalPos - circle.GetCenter()).sqrMagnitude <= circle.circle.radius * circle.circle.radius) {
				normal = MathUtility.Rotate(min - new Vector2 (min.x, max.y),90).normalized;
				hit = verticalPos;
				return true;
			}
//			CreateDebuger (circle.GetCenter (),min, new Vector2 (min.x, max.y),verticalPos);

			verticalPos = MathUtility.GetVerticalForPointAndLine (circle.GetCenter (), new Vector2 (min.x, max.y), max);
			if (MathUtility.IsPointOnLineSegmentByMagnitude(verticalPos, new Vector2 (min.x, max.y), max) &&  (verticalPos - circle.GetCenter()).sqrMagnitude <= circle.circle.radius * circle.circle.radius) {
				normal = MathUtility.Rotate(new Vector2 (min.x, max.y) -  max,90).normalized;
				hit = verticalPos;
				return true;
			}
//			CreateDebuger (circle.GetCenter (), new Vector2 (min.x, max.y), max,verticalPos);

			verticalPos = MathUtility.GetVerticalForPointAndLine (circle.GetCenter (), max, new Vector2 (max.x, min.y));
			if (MathUtility.IsPointOnLineSegmentByMagnitude(verticalPos, max, new Vector2 (max.x, min.y)) && (verticalPos - circle.GetCenter()).sqrMagnitude <= circle.circle.radius * circle.circle.radius) {
				normal = MathUtility.Rotate(max -new Vector2 (max.x, min.y),90).normalized;
				hit = verticalPos;
				return true;
			}
//			CreateDebuger (circle.GetCenter (),max, new Vector2 (max.x, min.y),verticalPos);

			verticalPos = MathUtility.GetVerticalForPointAndLine (circle.GetCenter (), new Vector2 (max.x, min.y), min);
			if (MathUtility.IsPointOnLineSegmentByMagnitude(verticalPos,new Vector2 (max.x, min.y), min) && (verticalPos - circle.GetCenter()).sqrMagnitude <= circle.circle.radius * circle.circle.radius) {
				normal = MathUtility.Rotate(new Vector2 (max.x, min.y) - min,90).normalized;
				hit = verticalPos;
				return true;
			}
//			CreateDebuger (circle.GetCenter (),new Vector2 (max.x, min.y), min,verticalPos);

			hit = Vector2.zero;
			return false;
		}

		static void CreateDebuger(Vector2 pos0,Vector2 pos1,Vector2 pos2,Vector2 pos3){
			TestDebuger td = GameObject.FindObjectOfType<TestDebuger> ();
			if (td == null) {
				GameObject go = new GameObject ();
				td = go.AddComponent<TestDebuger> ();
			}
			td.pos0 = pos0;
			td.pos1 = pos1;
			td.pos2 = pos2;
			td.pos3 = pos3;
		}

		//チェク二つAABBの交差か
		static bool CheckAABBIntersects (Bounds aabb1, Bounds aabb2)
		{
			//1.Bounds.Intersects (bounds1)
			if (aabb1.min.x > aabb2.max.x)
				return false;
			if (aabb1.max.x < aabb2.min.x)
				return false;
			if (aabb1.min.y > aabb2.max.y)
				return false;
			if (aabb1.max.y < aabb2.min.y)
				return false;
			return true;
		}

		//optimized.
		public static bool IsCollideBoxAndBox (CustomBoxCollider2D box0, CustomBoxCollider2D box1)
		{
			//AABB
			if (!CheckAABBIntersects (box0.mBoxCollider2D.bounds, box1.mBoxCollider2D.bounds)) {//  !.Intersects(box1.mBoxCollider2D.bounds)){
				return false;
			}
			Vector2 pos0 = box0.transform.TransformPoint (box0.GetCenter () + new Vector2 (-box0.mBoxCollider2D.size.x, -box0.mBoxCollider2D.size.y) * 0.5f);
			Vector2 pos1 = box0.transform.TransformPoint (box0.GetCenter () + new Vector2 (box0.mBoxCollider2D.size.x, -box0.mBoxCollider2D.size.y) * 0.5f);
			Vector2 pos2 = box0.transform.TransformPoint (box0.GetCenter () + new Vector2 (box0.mBoxCollider2D.size.x, box0.mBoxCollider2D.size.y) * 0.5f);
			Vector2 pos3 = box0.transform.TransformPoint (box0.GetCenter () + new Vector2 (-box0.mBoxCollider2D.size.x, box0.mBoxCollider2D.size.y) * 0.5f);
			Vector2[] boxVerts = new Vector2[]{ pos0, pos1, pos2, pos3 };
			Vector2 pos4 = box1.transform.TransformPoint (box1.GetCenter () + new Vector2 (-box1.mBoxCollider2D.size.x, -box1.mBoxCollider2D.size.y) * 0.5f);
			Vector2 pos5 = box1.transform.TransformPoint (box1.GetCenter () + new Vector2 (box1.mBoxCollider2D.size.x, -box1.mBoxCollider2D.size.y) * 0.5f);
			Vector2 pos6 = box1.transform.TransformPoint (box1.GetCenter () + new Vector2 (box1.mBoxCollider2D.size.x, box1.mBoxCollider2D.size.y) * 0.5f);
			Vector2 pos7 = box1.transform.TransformPoint (box1.GetCenter () + new Vector2 (-box1.mBoxCollider2D.size.x, box1.mBoxCollider2D.size.y) * 0.5f);
			Vector2[] boxVerts1 = new Vector2[]{ pos4, pos5, pos6, pos7 };
			Vector2 hit;
			for (int i = 0; i < boxVerts.Length; i++) {
				for (int j = 0; j < boxVerts1.Length; j++) {
					bool isIntersect = MathUtility.IsLineSegmentAndLineSegment (boxVerts [i], boxVerts [(i + 1) % boxVerts.Length], boxVerts1 [j], boxVerts1 [(j + 1) / boxVerts1.Length], out hit);
					if (isIntersect) {
						return true;
					}
				}
			}
			return false;
		}

		public static List<Vector2> GetBoxAndBoxCollisions (CustomBoxCollider2D box0, CustomBoxCollider2D box1)
		{
			return null;
		}

		public static List<Vector2> GetCircleAndCircleCollisions (CustomCircleCollider2D circle0, CustomCircleCollider2D circle1)
		{
			return null;
		}

		public static List<Vector2> GetCircleAndBoxCollisions (CustomCircleCollider2D circle, CustomBoxCollider2D box)
		{
			return null;
		}

		public static Vector2 GetVerticalLine (Vector2 pos)
		{
			return Vector2.zero;
		}


	}
}