using UnityEngine;
using System.Collections;
using Extensions;

namespace BMH
{
	public class Mine : Star
	{
		[HideInInspector]
		public float initMeshRendererAlpha;
		public LayerMask whatICantActivateIn;
		public delegate void OnDeath();
		public event OnDeath onDeath;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (rigid == null)
					rigid = GetComponent<Rigidbody>();
				// if (distanceJoint == null)
				// 	distanceJoint = GetComponent<DistanceJoint2D>();
				if (meshRenderer == null)
					meshRenderer = GetComponent<MeshRenderer>();
				initMeshRendererAlpha = meshRenderer.material.color.a;
				return;
			}
#endif
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			collider.enabled = false;
			if (meshRenderer.material.color.a != initMeshRendererAlpha)
				meshRenderer.material.color = meshRenderer.material.color.SetAlpha(initMeshRendererAlpha);
			if (onDeath != null)
			{
				onDeath ();
				onDeath = null;
			}
		}

		public virtual IEnumerator ActivateRoutine (float delay, float duration)
		{
			yield return new WaitForSeconds(delay);
			if (duration != 0)
				delayedDespawn = ObjectPool.Instance.DelayDespawn (prefabIndex, gameObject, trs, duration);
			yield return new WaitUntil(() => (Physics2D.OverlapCircle(trs.position, radius, whatICantActivateIn) == null));
			Activate ();
		}

		public virtual void Activate ()
		{
			meshRenderer.material.color = meshRenderer.material.color.SetAlpha(1);
			collider.enabled = true;
		}
	}
}