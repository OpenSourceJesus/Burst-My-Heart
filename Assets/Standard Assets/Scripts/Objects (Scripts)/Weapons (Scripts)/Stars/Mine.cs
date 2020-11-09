using UnityEngine;
using System.Collections;
using Extensions;

namespace BMH
{
	public class Mine : Star
	{
		[HideInInspector]
		public float initSpriteRendererAlpha;
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
					rigid = GetComponent<Rigidbody2D>();
				if (distanceJoint == null)
					distanceJoint = GetComponent<DistanceJoint2D>();
				if (spriteRenderer == null)
					spriteRenderer = GetComponent<SpriteRenderer>();
				initSpriteRendererAlpha = spriteRenderer.color.a;
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
			if (spriteRenderer.color.a != initSpriteRendererAlpha)
				spriteRenderer.color = spriteRenderer.color.SetAlpha(initSpriteRendererAlpha);
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
			spriteRenderer.color = spriteRenderer.color.SetAlpha(1);
			collider.enabled = true;
		}
	}
}