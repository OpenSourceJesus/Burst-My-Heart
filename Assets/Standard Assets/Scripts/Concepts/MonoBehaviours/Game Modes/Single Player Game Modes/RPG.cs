using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;
using System;

namespace BMH
{
	//[ExecuteAlways]
	public class RPG : SinglePlayerGameMode
	{
		public TerrainMaterialEntry[] terrainMaterialEntries;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
			// GameManager.GetSingleton<SaveAndLoadManager>().Load ();
			Boss[] bosses = FindObjectsOfType<Boss>();
			foreach (Boss boss in bosses)
				boss.OnTriggerExit2D (GameManager.GetSingleton<HumanPlayer>().body.collider);
		}

// #if UNITY_EDITOR
// 		public virtual void Update ()
// 		{
// 			foreach (TerrainMaterialEntry terrainMaterialEntry in terrainMaterialEntries)
// 			{
// 				terrainMaterialEntry.terrainMaterial.fillMaterial = terrainMaterialEntry.fillMaterial;
// 				terrainMaterialEntry.terrainMaterial.edgeMaterial = terrainMaterialEntry.edgeMaterial;
// 			}
// 		}
// #endif

		public override void DoUpdate ()
		{
			base.DoUpdate ();
			GameManager.GetSingleton<CameraScript>().trs.position = GameManager.GetSingleton<HumanPlayer>().lengthVisualizerTrs.position.SetZ(GameManager.GetSingleton<CameraScript>().trs.position.z);
		}

		public virtual void SetScore (int amount)
		{
			score = amount;
			scoreText.text = "" + (int) score;
		}

		public virtual void AddScore (int amount)
		{
			SetScore ((int) score + amount);
		}

		[Serializable]
		public struct TerrainMaterialEntry
		{
			public Ferr2DT_Material terrainMaterial;
			public Material fillMaterial;
			public Material edgeMaterial;
		}
	}
}