#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class SnapPosition : EditorScript
{
	public Transform trs;
	public Vector3 snap = new Vector3(1, 1, 0);

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
		}
	}

	public virtual void Update ()
	{
		trs.position = trs.position.Snap(snap);
	}
}
#endif