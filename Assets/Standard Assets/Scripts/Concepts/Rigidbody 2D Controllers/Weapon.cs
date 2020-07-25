using UnityEngine;

namespace BMH
{
	public class Weapon : Rigidbody2DController, ISpawnable
	{
		public delegate void OnCollide(Collision2D coll, Weapon weapon);
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
					GameManager.GetSingleton<ObjectPool>().CancelDelayedDespawn (delayedDespawn);
				delayedDespawn = GameManager.GetSingleton<ObjectPool>().DelayDespawn (prefabIndex, gameObject, trs, despawnDelay);
			}
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (delayedDespawn != null)
				GameManager.GetSingleton<ObjectPool>().CancelDelayedDespawn (delayedDespawn);
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			if (onCollide != null)
				onCollide (coll, this);
		}
	}
}