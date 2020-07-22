#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;
using UnityEditor;

namespace BMH
{
	public class MakeArea : EditorHelper
	{
		public AreaPath[] areaPaths;

		[MenuItem("RPG/Make Area")]
		public static void _MakeArea_ ()
		{
			FindObjectOfType<MakeArea>().MakeArea_ ();
		}

		public virtual void MakeArea_ ()
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
			FindObjectOfType<MakeArea>().StopMakingArea ();
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