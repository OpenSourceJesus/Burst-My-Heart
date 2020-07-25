using UnityEngine;
using System.Collections;
using Extensions;
using BMH;

//[ExecuteAlways]
[DisallowMultipleComponent]
public class VectorGridForce : MonoBehaviour, IUpdatable
{
	public Transform trs;
	public Rigidbody2D rigid;
	public float forceScale;
	public bool isDirectional;
	public float radius;
	public Color color = Color.white;
	public bool hasColor;
	public bool PauseWhileUnfocused
	{
		get
		{
			return GameManager.GetSingleton<OnlineBattle>() == null;
		}
	}

	public virtual void OnEnable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (trs == null)
				trs = GetComponent<Transform>();
			if (rigid == null)
				rigid = GetComponent<Rigidbody2D>();
			return;
		}
#endif
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	public virtual void OnDisable ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

	public virtual void DoUpdate () 
	{
		if (isDirectional)
			GameManager.GetSingleton<VectorGrid>().AddGridForce(trs.position, rigid.velocity * forceScale, radius, color, hasColor);
		else
			GameManager.GetSingleton<VectorGrid>().AddGridForce(trs.position, forceScale, radius, color, hasColor);
	}
}
