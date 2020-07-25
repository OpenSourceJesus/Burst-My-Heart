#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;
using UnityEditor;

namespace BMH
{
	public class AreaMaker : EditorScript
	{
		public AreaPath[] areaPaths;

		[MenuItem("RPG/Make Area")]
		public static void _MakeArea ()
		{
			GameManager.GetSingleton<AreaMaker>().MakeArea ();
		}

		public virtual void MakeArea ()
		{
			StopAllCoroutines();
			foreach (AreaPath areaPath in areaPaths)
				areaPath.StopAllCoroutines();
			StartCoroutine(MakeAreaRoutine ());
		}

		public virtual IEnumerator MakeAreaRoutine ()
		{
			foreach (AreaPath areaPath in areaPaths)
				yield return StartCoroutine(areaPath.MakeRoutine ());
		}

		[MenuItem("RPG/Stop Making Area")]
		public static void _StopMakingArea ()
		{
			GameManager.GetSingleton<AreaMaker>().StopMakingArea ();
		}

		public virtual void StopMakingArea ()
		{
			StopAllCoroutines();
			foreach (AreaPath areaPath in areaPaths)
				areaPath.StopAllCoroutines();
		}
	}
}
#endif