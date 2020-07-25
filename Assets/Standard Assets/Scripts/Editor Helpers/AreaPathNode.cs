#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;

namespace BMH
{
	public class AreaPathNode : EditorScript
	{
		public AreaPath areaPath;
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
				if (areaPath == null)
					areaPath = GetComponentInParent<AreaPath>();
			}
		}

		public virtual void OnDrawGizmos ()
		{
			Gizmos.color = gizmosColor;
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.DrawSphere(trs.position, radius);
		}
	}
}
#endif