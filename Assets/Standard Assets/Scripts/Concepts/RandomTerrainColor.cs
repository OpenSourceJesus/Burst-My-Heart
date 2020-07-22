#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using Extensions;
using Ferr;

[ExecuteAlways]
public class RandomTerrainColor : EditorHelper
{
	public Ferr2DT_PathTerrain terrain;
    public bool update;
    public Color[] colors;

	public virtual void OnEnable ()
    {
    	if (!Application.isPlaying)
    	{
            if (terrain == null)
                terrain = GetComponent<Ferr2DT_PathTerrain>();
    		return;
    	}
    }

    public virtual void Update ()
    {
        if (!update)
            return;
        update = false;
        colors = colors.Add(terrain.vertexColor);
        terrain.vertexColor = ColorExtensions.RandomColor();
        terrain.Build (true);
    }
}
#endif