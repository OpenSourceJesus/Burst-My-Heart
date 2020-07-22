using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class RandomPosition : MonoBehaviour
{
	public Transform trs;
    public float maxDistFromOrigin;
    public Vector2 translateOrigin;
#if UNITY_EDITOR
    public bool update;
    public bool setTranslateOriginToCurrentPosition;
#endif

	public virtual void Start ()
    {
#if UNITY_EDITOR
    	if (!Application.isPlaying)
    	{
    		if (trs == null)
    			trs = GetComponent<Transform>();
    		return;
    	}
#endif
        trs.position = Random.insideUnitCircle * maxDistFromOrigin;
        trs.position += (Vector3) translateOrigin;
    }

#if UNITY_EDITOR
    public virtual void Update ()
    {
        if (setTranslateOriginToCurrentPosition)
        {
            setTranslateOriginToCurrentPosition = false;
            translateOrigin = trs.position;
        }
        if (!update)
            return;
        update = false;
        trs.position = Random.insideUnitCircle * maxDistFromOrigin;
        trs.position += (Vector3) translateOrigin;
    }
#endif
}
