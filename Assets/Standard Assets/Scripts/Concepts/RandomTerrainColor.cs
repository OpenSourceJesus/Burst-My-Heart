#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using Extensions;
using Ferr;

namespace BMH
{
	//[ExecuteAlways]
	public class RandomTerrainColor : EditorScript
	{
		public Ferr2DT_PathTerrain terrain;
		public List<Color> pastColors = new List<Color>();

		public virtual void OnEnable ()
		{
			if (!Application.isPlaying)
			{
				if (terrain == null)
					terrain = GetComponent<Ferr2DT_PathTerrain>();
				return;
			}
		}

		public override void Do ()
		{
			pastColors.Add(terrain.vertexColor);
			terrain.vertexColor = ColorExtensions.RandomColor();
			terrain.Build (true);
		}
	}
}
#endif