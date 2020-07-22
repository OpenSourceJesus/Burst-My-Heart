#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;

public class AreaPathNode : EditorHelper
{
    public Transform trs;
    public Color gizmosColor;
	public AreaPathNode[] children;
	public float radius;

	public virtual void Start ()
	{
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
		}
	}

    public virtual void OnDrawGizmos ()
    {
        Gizmos.color = gizmosColor;
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.DrawSphere(trs.position, radius);
    }
}
#endif