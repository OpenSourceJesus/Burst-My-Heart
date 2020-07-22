using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
	public static class RectExtensions
	{
		public static Rect NULL = new Rect(MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT, MathfExtensions.NULL_FLOAT);
		
		public static bool IsEncapsulating (this Rect b1, Rect b2, bool equalRectsRetunsTrue)
		{
			if (equalRectsRetunsTrue)
			{
				bool minIsOk = b1.min.x <= b2.min.x && b1.min.y <= b2.min.y;
				bool maxIsOk = b1.max.x >= b2.max.x && b1.max.y >= b2.max.y;
				return minIsOk && maxIsOk;
			}
			else
			{
				bool minIsOk = b1.min.x < b2.min.x && b1.min.y < b2.min.y;
				bool maxIsOk = b1.max.x > b2.max.x && b1.max.y > b2.max.y;
				return minIsOk && maxIsOk;
			}
		}
		
		public static bool IsExtendingOutside (this Rect b1, Rect b2, bool equalRectsRetunsTrue)
		{
			if (equalRectsRetunsTrue)
			{
				bool minIsOk = b1.min.x <= b2.min.x || b1.min.y <= b2.min.y;
				bool maxIsOk = b1.max.x >= b2.max.x || b1.max.y >= b2.max.y;
				return minIsOk || maxIsOk;
			}
			else
			{
				bool minIsOk = b1.min.x < b2.min.x || b1.min.y < b2.min.y;
				bool maxIsOk = b1.max.x > b2.max.x || b1.max.y > b2.max.y;
				return minIsOk || maxIsOk;
			}
		}
		
		public static Rect ToRect (this Bounds bounds)
		{
			return Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
		}

		public static Rect Combine (Rect[] rectsArray)
		{
			Rect output = rectsArray[0];
			for (int i = 1; i < rectsArray.Length; i ++)
			{
				if (rectsArray[i].min.x < output.min.x)
					output.min = new Vector2(rectsArray[i].min.x, output.min.y);
				if (rectsArray[i].min.y < output.min.y)
					output.min = new Vector2(output.min.x, rectsArray[i].min.y);
				if (rectsArray[i].max.x > output.max.x)
					output.max = new Vector2(rectsArray[i].max.x, output.max.y);
				if (rectsArray[i].max.y > output.max.y)
					output.max = new Vector2(output.max.x, rectsArray[i].max.y);
			}
			return output;
		}

		public static Rect Expand (this Rect rect, Vector2 amount)
		{
			Vector2 center = rect.center;
			rect.size += amount;
			rect.center = center;
			return rect;
		}

		public static Vector2 ClosestPoint (this Rect rect, Vector2 point)
		{
			return point.ClampVectorComponents(rect.min, rect.max);
		}

		public static Vector2 ToNormalizedPosition (this Rect rect, Vector2 point)
		{
			return Rect.PointToNormalized(rect, point);
			// return Vector2.one.Divide(rect.size) * (point - rect.min);
		}

		public static Vector2 ToNormalizedPosition (this RectInt rect, Vector2Int point)
		{
			return Vector2.one.Divide(rect.size.ToVec2()).Multiply(point.ToVec2() - rect.min.ToVec2());
		}

		public static Rect SetToPositiveSize (this Rect rect)
		{
			Rect output = rect;
			output.size = new Vector2(Mathf.Abs(output.size.x), Mathf.Abs(output.size.y));
			output.center = rect.center;
			return output;
		}
	}
}