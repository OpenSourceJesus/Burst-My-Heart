#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class RotatePolygonCollider2D : EditorScript
	{
		public PolygonCollider2D polygonCollider;
		public float rotate;

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
				points[i] = points[i].Rotate(rotate);
			polygonCollider.points = points;
		}
	}
}
#endif