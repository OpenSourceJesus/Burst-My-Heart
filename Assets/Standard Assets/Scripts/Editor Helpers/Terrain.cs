#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;

public class Terrain : EditorScript
{
	public Transform trs;
	public Ferr2DT_PathTerrain terrain;
	public bool canBeErased = true;
	public RandomTerrainColor randomTerrainColor;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (terrain == null)
				terrain = GetComponent<Ferr2DT_PathTerrain>();
			if (trs == null)
				trs = GetComponent<Transform>();
			if (randomTerrainColor == null)
				randomTerrainColor = GetComponent<RandomTerrainColor>();
		}
	}

	// public virtual void Update ()
	// {
	// 	if (Application.isPlaying)
	// 		return;
    //     for (int i = 0; i < terrain.pathData._points.Count; i ++)
    //         terrain.pathData._points[i] = terrain.pathData._points[i].Snap(Vector2.one);
	// }
	
	public virtual LineSegment2D[] GetBorders ()
	{
		LineSegment2D[] output = new LineSegment2D[terrain.pathData._points.Count];
		for (int i = 0; i < terrain.pathData._points.Count - 1; i ++)
			output[i] = new LineSegment2D((Vector2) trs.position + terrain.pathData._points[i], (Vector2) trs.position + terrain.pathData._points[i + 1]);
		output[output.Length - 1] = new LineSegment2D((Vector2) trs.position + terrain.pathData._points[output.Length - 1], (Vector2) trs.position + terrain.pathData._points[0]);
		return output;
	}

	public virtual bool IsPolygon ()
	{
		LineSegment2D[] borders = GetBorders();
		foreach (LineSegment2D border in borders)
		{
			foreach (LineSegment2D otherBorder in borders)
			{
				if (border != otherBorder && border.DoIIntersectWith(otherBorder, false))
					return false;
			}
		}
		return true;
	}
}
#endif