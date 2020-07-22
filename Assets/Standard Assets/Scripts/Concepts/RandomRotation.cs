using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class RandomRotation : MonoBehaviour
{
	public Transform trs;
#if UNITY_EDITOR
	public bool update;
#endif

	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			return;
		}
		update = false;
#endif
	}

#if UNITY_EDITOR
	public virtual void Update ()
	{
		if (!update)
			return;
		update = false;
		trs.eulerAngles = Vector3.forward * Random.value * 360;
	}
#endif
}
