#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class ScalePolygonCollider2D : EditorHelper
{
	public PolygonCollider2D polygonCollider;
	public Vector2 scale;
	public bool update;
	Vector2[] points;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (polygonCollider == null)
				polygonCollider = GetComponent<PolygonCollider2D>();
		}
	}

	public virtual void Update ()
	{
		if (!update)
			return;
		update = false;
		points = polygonCollider.points;
		for (int i = 0; i < points.Length; i ++)
			points[i] = points[i].Multiply(scale);
		polygonCollider.points = points;
	}
}
#endif