#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class ScalePolygonCollider2D : EditorScript
	{
		public PolygonCollider2D polygonCollider;
		public Vector2 scale;

		public virtual void Start ()
		{
			if (!Application.isPlaying)
			{
				if (polygonCollider == null)
					polygonCollider = GetComponent<PolygonCollider2D>();
			}
		}

		public override void Do ()
		{
			Vector2[] points = polygonCollider.points;
			for (int i = 0; i < points.Length; i ++)
				points[i] = points[i].Multiply(scale);
			polygonCollider.points = points;
		}
	}
}
#endif