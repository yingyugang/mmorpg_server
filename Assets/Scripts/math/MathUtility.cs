using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
	//求点到直线的垂线交点
	//注释处为推理过程（代数法）
	//先求出斜率a，再求出常量b
	public static Vector2 GetVerticalForPointAndLine (Vector2 point, Vector2 start, Vector2 end)
	{
		//二個特別な場合
		if (start.x - end.x == 0) {
			return new Vector2 (start.x, point.y);
		}

		if (start.y - end.y == 0) {
			return new Vector2 (point.x, start.y);
		}
		float a = (start.y - end.y) / (start.x - end.x);
		float b = start.y - start.x * a;

		Vector2 rotated = Rotate (new Vector2 (start.x - end.x, start.y - end.y), 90f);
		float a0 = rotated.y / rotated.x;
		float b0 = point.y - point.x * a0;

		float x0 = (b - b0) / (a0 - a);
		float y0 = a * x0 + b;
		return new Vector2 (x0, y0);
	}

	//ポイントがラインであるのかどうか、計算する。精度がある(3D)
	public static bool IsPointOnLineSegmentByMagnitude (Vector3 point, Vector3 start, Vector3 end, uint accuracy = 0)
	{
		float distance = (end - start).magnitude;
		float dis0 = (point - end).magnitude;
		float dis1 = (point - start).magnitude;
		Debug.Log (string.Format("{0}/{1}/{2}",distance,dis0,dis1));
		if (accuracy == 0) {
			return distance == dis0 + dis1;
		} else {
			accuracy = (uint)Mathf.Pow (10, accuracy);
			return Mathf.RoundToInt (distance * accuracy) == Mathf.RoundToInt ((dis0 + dis1) * accuracy);
		}
	}

	//ポイントがラインであるのかどうか、計算する。精度がある(2D)
	public static bool IsPointOnLineSegmentByMagnitude (Vector2 point, Vector2 start, Vector2 end, uint accuracy = 0)
	{
		float distance = (end - start).magnitude;
		float dis0 = (point - end).magnitude;
		float dis1 = (point - start).magnitude;
		Debug.Log (string.Format("{0}/{1}/{2}",distance,dis0,dis1));
		if (accuracy == 0) {
			return distance == dis0 + dis1;
		} else {
			accuracy = (uint)Mathf.Pow (10, accuracy);
			return Mathf.RoundToInt (distance * accuracy) == Mathf.RoundToInt ((dis0 + dis1) * accuracy);
		}
	}

	//ポイントが線分であるのかどうか、計算する。(2D)
	public static bool IsPointOnLineSegmentByAlgebra (Vector2 point, Vector2 start, Vector2 end)
	{
		if (IsPointOnLineByAlgebra (point, start, end)) {
			float minX = Mathf.Min (start.x, end.x);
			float maxX = Mathf.Max (start.x, end.x);
			float minY = Mathf.Min (start.y, end.y);
			float maxY = Mathf.Max (start.y, end.y);
			if (point.x >= minX && point.x <= maxX && point.x >= minY && point.x <= maxY) {
				return true;
			}
		}
		return false;
	}

	//ポイントがラインであるのかどうか、計算する。(2D)
	public static bool IsPointOnLineByAlgebra (Vector2 point, Vector2 start, Vector2 end)
	{
		float a, b;
		if (end.x - start.x == 0) {
			a = 0;
			b = start.y;
		} else {
			a = (end.y - start.y) / (end.x - start.x);
			b = start.y - (end.y - start.y) / (end.x - start.x) / start.x;
		}
		Debug.Log (string.Format ("{0}/{1}", point.y, point.x * a + b));
		if (point.y == point.x * a + b) {
			return true;
		} else
			return false;
	}

	//二つ線の交点（Algebraic）(2D)
	public static bool IsLineSegmentAndLineSegment (Vector2 start0, Vector2 end0, Vector2 start1, Vector2 end1, out Vector2 hit)
	{
		Vector2 delta0 = start0 - end0;
		float a0 = delta0.y / delta0.x;
		float b0 = start0.y - start0.x * a0;
		float a1 = (start1.y - end1.y) / (start1.x - end1.x);
		float b1 = start1.y - start1.x * a1;
		if (a0 == a1) {
			hit = Vector2.zero;
			return false;
		}
		float x0 = (b1 - b0) / (a0 - a1);
		float y0 = x0 * a0 + b0;
		hit = new Vector2 (x0, y0);
		return true;
	}

	//角度を回し
	public static Vector2 Rotate (Vector2 pos, float degrees)
	{
		float angle = degrees / 180f * Mathf.PI;
		float newX = pos.x * Mathf.Cos (angle) - pos.y * Mathf.Sin (angle);
		float newY = pos.x * Mathf.Sin (angle) + pos.y * Mathf.Cos (angle);
		return new Vector2 (newX, newY);
	}

	//点で相対的回し
	public static Vector2 RotateAround (Vector2 pos, Vector2 point, float degrees)
	{
		Vector2 relativePos = pos - point;
		Vector2 rotatePos = Rotate (relativePos, degrees);
		return rotatePos;
	}

	//ポイントが三角形に中にかどうかチェックする（判断点是否在三角形里面）
	public static bool IsPointInTri (Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
	{
		float TotalArea = CalcTriArea (v1, v2, v3);
		float Area1 = CalcTriArea (pt, v2, v3);
		float Area2 = CalcTriArea (pt, v1, v3);
		float Area3 = CalcTriArea (pt, v1, v2);
		if ((Area1 + Area2 + Area3) > TotalArea)
			return false;
		else
			return true;
	}

	public static float CalcTriArea (Vector2 v1, Vector2 v2, Vector2 v3)
	{
		float det = 0.0f;
		det = ((v1.x - v3.x) * (v2.y - v3.y)) - ((v2.x - v3.x) * (v1.y - v3.y));
		return (det / 2.0f);
	}

	//ポイントが三角形中であるかどうかチェックする。（3D）
	public static bool IsPointInTri (Vector3 P, Vector3 A, Vector3 B, Vector3 C)
	{
		return SameSide (A, B, C, P) &&
		SameSide (B, C, A, P) &&
		SameSide (C, A, B, P);
	}

	// Determine whether two vectors v1 and v2 point to the same direction
	// v1 = Cross(AB, AC)
	// v2 = Cross(AB, AP)
	static bool SameSide (Vector3 A, Vector3 B, Vector3 C, Vector3 P)
	{
		Vector3 AB = B - A;
		Vector3 AC = C - A;
		Vector3 AP = P - A;
		Vector3 v1 = Vector3.Cross (AB.normalized, AC.normalized);
		Vector3 v2 = Vector3.Cross (AB.normalized, AP.normalized); 
		// v1 and v2 should point to the same direction
		return Vector3.Dot (v1.normalized, v2.normalized) >= 0;
	}

}
