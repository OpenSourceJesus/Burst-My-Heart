using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace Extensions
{
	public static class Physics2DExtensions
	{
		public static RaycastHit2D LinecastWithWidth (Vector2 start, Vector2 end, float width, int layerMask)
		{
			return Physics2D.BoxCast((start + end) / 2, new Vector2(Vector2.Distance(start, end), width), (end - start).GetFacingAngle(), end - start, Vector2.Distance(start, end), layerMask);
		}

		public static RaycastHit2D[] LinecastAllWithWidth (Vector2 start, Vector2 end, float width, int layerMask)
		{
			return Physics2D.BoxCastAll((start + end) / 2, new Vector2(Vector2.Distance(start, end), width), (end - start).GetFacingAngle(), end - start, Vector2.Distance(start, end), layerMask);
		}
		
		public static int LinecastWithWidth (Vector2 start, Vector2 end, float width, ContactFilter2D contactFilter, RaycastHit2D[] results)
		{
			return Physics2D.BoxCast((start + end) / 2, new Vector2(Vector2.Distance(start, end), width), (end - start).GetFacingAngle(), end - start, contactFilter, results, Vector2.Distance(start, end));
		}

		public static RaycastHit2D[] LinecastAllWithWidthAndOrder (Vector2 start, Vector2 end, float width, int layerMask)
		{
			LineSegment2D lineSegment = new LineSegment2D(start, end);
			List<RaycastHit2D> hits = new List<RaycastHit2D>();
			RaycastHit2D hit;
			do
			{
				hit = LinecastWithWidth(lineSegment.start, end, width, layerMask);
				if (hit.collider != null)
				{
					lineSegment.start = lineSegment.GetPointWithDirectedDistance(lineSegment.GetDirectedDistanceAlongParallel(hit.point));
					hits.Add(hit);
				}
			} while (hit.collider != null);
			return hits.ToArray();
		}
	}
}