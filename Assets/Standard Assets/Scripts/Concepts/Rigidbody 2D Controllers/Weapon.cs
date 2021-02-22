using UnityEngine;

namespace BMH
{
	public class Weapon : RigidbodyController, ISpawnable
	{
		public delegate void OnCollide(Collision coll, Weapon weapon);
		public event OnCollide onCollide;
		public int prefabIndex;
		public virtual int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public bool useDespawnDelay;
		public float despawnDelay;
		[HideInInspector]
		public ObjectPool.DelayedDespawn delayedDespawn;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (!gameObject.activeInHierarchy)
				return;
			if (useDespawnDelay)
			{
				if (delayedDespawn != null)
					ObjectPool.Instance.CancelDelayedDespawn (delayedDespawn);
				delayedDespawn = ObjectPool.Instance.DelayDespawn (prefabIndex, gameObject, trs, despawnDelay);
			}
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (delayedDespawn != null && ObjectPool.Instance != null)
				ObjectPool.instance.CancelDelayedDespawn (delayedDespawn);
		}

		public virtual void OnCollisionEnter (Collision coll)
		{
			if (onCollide != null)
				onCollide (coll, this);
		}
	}
}