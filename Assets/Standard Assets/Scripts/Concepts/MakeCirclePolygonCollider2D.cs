using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;

namespace BMH
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    // [RequireComponent(typeof(PolygonCollider2D))]
    public class MakeCirclePolygonCollider2D : MonoBehaviour
    {
        public Transform trs;
        public PolygonCollider2D polygonCollider2D;
        public float radius;
        [Range(3, 50)]
        public int pointCount;
        Vector2[] points;
#if UNITY_EDITOR
        public bool update;
#endif

    	public virtual void Start ()
        {
#if UNITY_EDITOR
        	if (!Application.isPlaying)
        	{
                if (trs == null)
                    trs = GetComponent<Transform>();
        		if (polygonCollider2D == null)
                    polygonCollider2D = GetComponent<PolygonCollider2D>();
        		return;
        	}
#endif
        }

#if UNITY_EDITOR
        public virtual void Update ()
        {
            if (!update)
                return;
            update = false;
            points = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i ++)
                points[i] = VectorExtensions.FromFacingAngle((i * 360f / pointCount) * (1f / 360f) * 360) * radius;
            polygonCollider2D.points = points;
        }
#endif
    }
}