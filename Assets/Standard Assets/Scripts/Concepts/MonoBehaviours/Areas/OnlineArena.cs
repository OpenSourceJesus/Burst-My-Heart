using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BMH
{
	[DisallowMultipleComponent]
    public class OnlineArena : Area
    {
		public new static OnlineArena instance;
		public new static OnlineArena Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<OnlineArena>();
				return instance;
			}
		}
    	public int areaPerPlayer;
    	public float lerpRate;
		public float noSpawnBorder;
		[HideInInspector]
    	public float desiredRadius;
		[HideInInspector]
    	public float desiredArea;

// 		public override void Awake ()
// 		{
// #if UNITY_EDITOR
// 			if (!Application.isPlaying)
// 			{
// 				if (trs == null)
// 					trs = GetComponent<Transform>();
// 				return;
// 			}
// #endif
// 			base.Awake ();
// 		}

		public virtual void SetSize (uint playerCount)
		{
			StopAllCoroutines();
			StartCoroutine(ChangeSizeRoutine (playerCount));
		}

		public virtual IEnumerator ChangeSizeRoutine (uint playerCount)
		{
			desiredArea = areaPerPlayer * playerCount;
			desiredRadius = Mathf.Sqrt(desiredArea / Mathf.PI);
			while (trs.localScale.x != desiredRadius * 2)
			{
				trs.localScale = Vector3.one * Mathf.Lerp(trs.localScale.x, desiredRadius * 2, lerpRate * Time.unscaledDeltaTime);
				yield return new WaitForEndOfFrame();
			}
		}
    }
}