using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BMH;

namespace Extensions
{
	public static class VectorExtensions
	{
		public static Vector3 NULL = new Vector3(MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT);
		public static Vector3 INFINITE = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
		
		public static Vector3 Snap (this Vector3 v, Vector3 snap)
		{
			return new Vector3(MathfExtensions.SnapToInterval(v.x, snap.x), MathfExtensions.SnapToInterval(v.y, snap.y), MathfExtensions.SnapToInterval(v.z, snap.z));
		}

		public static Vector2 Snap (this Vector2 v, Vector2 snap)
		{
			return new Vector2(MathfExtensions.SnapToInterval(v.x, snap.x), MathfExtensions.SnapToInterval(v.y, snap.y));
		}
		
		public static Vector3 Multiply (this Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		}

		public static Vector2 Multiply (this Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.x * v2.x, v1.y * v2.y);
		}
		
		public static Vector3 Divide (this Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
		}

		public static Vector2 Divide (this Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.x / v2.x, v1.y / v2.y);
		}
		
		public static Vector2 Rotate (this Vector2 v, float degrees)
		{
			float ang = GetFacingAngle(v) + degrees;
			ang *= Mathf.Deg2Rad;
			// ang = MathfExtensions.RegularizeAngle(ang);
			return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized * v.magnitude;
		}
		
		public static Vector2 Rotate (this Vector2 v, Vector2 pivotPoint, float degrees)
		{
			float ang = GetFacingAngle(v - pivotPoint) + degrees;
			ang *= Mathf.Deg2Rad;
			return pivotPoint + (new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized * Vector2.Distance(v, pivotPoint));
		}
		
		public static Vector2 Rotate (this Vector3 v, float degrees)
		{
			float ang = GetFacingAngle(v) + degrees;
			ang *= Mathf.Deg2Rad;
			// ang = MathfExtensions.RegularizeAngle(ang);
			return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized * v.magnitude;
		}

		public static Vector2 Rotate (this Vector3 v, Vector3 pivotPoint, float degrees)
		{
			float ang = GetFacingAngle(v - pivotPoint) + degrees;
			ang *= Mathf.Deg2Rad;
			return (Vector2) pivotPoint + (new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized * Vector2.Distance(v, pivotPoint));
		}
		
		public static float GetFacingAngle (this Vector2 v)
		{
			v = v.normalized;
			return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
		}
		
		public static float GetFacingAngle (this Vector3 v)
		{
			v = v.normalized;
			return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
		}
		
		public static Vector2 RotateTo (this Vector2 from, Vector2 to, float maxDegrees)
		{
			float ang = from.GetFacingAngle();
			ang += Mathf.Clamp(Vector2.SignedAngle(from, to), -maxDegrees, maxDegrees);
			ang *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized * from.magnitude;
		}
		
		public static Vector2 RotateTo (this Vector3 from, Vector3 to, float maxDegrees)
		{
			float ang = from.GetFacingAngle();
			ang += Mathf.Clamp(Vector2.SignedAngle(from, to), -maxDegrees, maxDegrees);
			ang *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)).normalized * from.magnitude;
		}
		
		public static Vector3 ClampVectorComponents (this Vector3 v, Vector3 min, Vector3 max)
		{
			return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
		}
		
		public static Vector2 ClampVectorComponents (this Vector2 v, Vector2 min, Vector2 max)
		{
			return new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
		}
		
		public static Vector3Int ToVec3Int (this Vector3 v, MathfExtensions.RoundingMethod roundMethod = MathfExtensions.RoundingMethod.HalfOrLessRoundsDown)
		{
			switch (roundMethod)
			{
				case MathfExtensions.RoundingMethod.HalfOrLessRoundsDown:
					return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
				default:
					throw new UnityException("The logic for handling the " + roundMethod.ToString() + " round method has not yet been implemented.");
			}
		}

		public static Vector2Int ToVec2Int (this Vector2 v, MathfExtensions.RoundingMethod roundMethod = MathfExtensions.RoundingMethod.HalfOrLessRoundsDown)
		{
			switch (roundMethod)
			{
				case MathfExtensions.RoundingMethod.HalfOrLessRoundsDown:
					return new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
				default:
					throw new UnityException("The logic for handling the " + roundMethod.ToString() + " round method has not yet been implemented.");
			}
		}

		public static Vector2 ToVec2 (this Vector2Int v)
		{
			return new Vector2(v.x, v.y);
		}
		
		public static Vector3 SetX (this Vector3 v, float x)
		{
			return new Vector3(x, v.y, v.z);
		}
		
		public static Vector3 SetY (this Vector3 v, float y)
		{
			return new Vector3(v.x, y, v.z);
		}
		
		public static Vector3 SetZ (this Vector3 v, float z)
		{
			return new Vector3(v.x, v.y, z);
		}
		
		public static Vector2 SetX (this Vector2 v, float x)
		{
			return new Vector2(x, v.y);
		}
		
		public static Vector2 SetY (this Vector2 v, float y)
		{
			return new Vector2(v.x, y);
		}

        public static Vector3 SetZ (this Vector2 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }
		
		public static Vector2 FromFacingAngle (float angle)
		{
			angle *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
		}

		public static Vector3 GetClosestPoint (Vector3 v, params Vector3[] points)
		{
			Vector3 closestPoint = points[0];
			Vector3 point;
			for (int i = 1; i < points.Length; i ++)
			{
				point = points[i];
				if (Vector3.Distance(v, point) < Vector3.Distance(v, closestPoint))
					closestPoint = point;
			}
			return closestPoint;
		}

		public static int GetIndexOfClosestPoint (Vector3 v, params Vector3[] points)
		{
			int indexOfClosestPoint = 0;
			Vector3 closestPoint = points[0];
			Vector3 point;
			for (int i = 1; i < points.Length; i ++)
			{
				point = points[i];
				if (Vector3.Distance(v,- point) < Vector3.Distance(v, closestPoint))
				{
					closestPoint = point;
					indexOfClosestPoint = i;
				}
			}
			return indexOfClosestPoint;
		}

		public static Vector2 GetClosestPoint (Vector2 v, params Vector2[] points)
		{
			Vector2 closestPoint = points[0];
			Vector2 point;
			for (int i = 1; i < points.Length; i ++)
			{
				point = points[i];
				if (Vector2.Distance(v, point) < Vector2.Distance(v, closestPoint))
					closestPoint = point;
			}
			return closestPoint;
		}

		public static int GetIndexOfClosestPoint (Vector2 v, params Vector2[] points)
		{
			int indexOfClosestPoint = 0;
			Vector2 closestPoint = points[0];
			Vector2 point;
			for (int i = 1; i < points.Length; i ++)
			{
				point = points[i];
				if (Vector2.Distance(v, point) < Vector2.Distance(v, closestPoint))
				{
					closestPoint = point;
					indexOfClosestPoint = i;
				}
			}
			return indexOfClosestPoint;
		}

		public static float Sign (Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
		}

		public static bool IsInTriangle (Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
		{
			float d1, d2, d3;
			bool has_neg, has_pos;
			d1 = Sign(pt, v1, v2);
			d2 = Sign(pt, v2, v3);
			d3 = Sign(pt, v3, v1);
			has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
			has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
			return !(has_neg && has_pos);
		}
    }
}