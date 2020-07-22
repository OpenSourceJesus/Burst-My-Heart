#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;

public class Terrain : EditorHelper
{
	public Transform trs;
	public Ferr2DT_PathTerrain terrain;
	public bool canBeErased = true;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (terrain == null)
				terrain = GetComponent<Ferr2DT_PathTerrain>();
			if (trs == null)
				trs = GetComponent<Transform>();
		}
	}

	public virtual void Update ()
	{
        for (int i = 0; i < terrain.pathData._points.Count; i ++)
            terrain.pathData._points[i] = terrain.pathData._points[i].Snap(Vector2.one);
	}
}
#endif