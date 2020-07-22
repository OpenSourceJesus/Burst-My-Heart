using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using Extensions;

namespace BMH
{
	public class ObjectPool : SingletonMonoBehaviour<ObjectPool>
	{
		public bool preloadOnAwake = true;
		public Transform trs;
		public SpawnEntry[] spawnEntries = new SpawnEntry[0];
		public DelayedDespawn[] delayedDespawns = new DelayedDespawn[0];
		public RangedDespawn[] rangedDespawns = new RangedDespawn[0];
		public SpawnedEntry[] spawnedEntries = new SpawnedEntry[0];
		
		public override void Awake ()
		{
			base.Awake ();
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
			enabled = false;
			gameObject.SetActive(false);
			if (!preloadOnAwake)
				return;
			for (int i = 0 ; i < spawnEntries.Length; i ++)
			{
				for (int i2 = 0; i2 < spawnEntries[i].preload; i2 ++)
					Preload (i);
			}
		}
		
		public virtual void DoUpdate ()
		{
			for (int i = 0; i < delayedDespawns.Length; i ++)
			{
				DelayedDespawn delayedDespawn = delayedDespawns[i];
				delayedDespawn.timeRemaining -= Time.deltaTime;
				if (delayedDespawn.timeRemaining < 0)
				{
					Despawn (delayedDespawn.prefabIndex, delayedDespawn.go, delayedDespawn.trs);
					delayedDespawns = delayedDespawns.RemoveAt_class(i);
					i --;
					if (delayedDespawns.Length == 0 && rangedDespawns.Length == 0)
						enabled = false;
				}
			}
			for (int i = 0; i < rangedDespawns.Length; i ++)
			{
				RangedDespawn rangedDespawn = rangedDespawns[i];
				rangedDespawn.range -= Vector2.Distance(rangedDespawn.trs.localPosition, rangedDespawn.previousPos);
				rangedDespawn.previousPos = rangedDespawn.trs.localPosition;
				if (rangedDespawn.range < 0)
				{
					Despawn (rangedDespawn.prefabIndex, rangedDespawn.go, rangedDespawn.trs);
					rangedDespawns = rangedDespawns.Remove_class(rangedDespawn);
					i --;
					if (rangedDespawns.Length == 0 && delayedDespawns.Length == 0)
						enabled = false;
				}
			}
		}
		
		public virtual DelayedDespawn DelayDespawn (int prefabIndex, GameObject clone, Transform trs, float delay)
		{
			DelayedDespawn delayedDespawn = new DelayedDespawn(delay);
			delayedDespawn.go = clone;
			delayedDespawn.trs = trs;
			delayedDespawn.prefab = spawnEntries[prefabIndex].prefab;
			delayedDespawn.prefabIndex = prefabIndex;
			delayedDespawns = delayedDespawns.Add_class(delayedDespawn);
			enabled = true;
			return delayedDespawn;
		}

		public virtual void CancelDelayedDespawn (DelayedDespawn delayedDespawn)
		{
			int indexOfDelayedDespawn = delayedDespawns.IndexOf_class(delayedDespawn);
			if (indexOfDelayedDespawn != -1)
			{
				delayedDespawns = delayedDespawns.RemoveAt_class(indexOfDelayedDespawn);
				if (delayedDespawns.Length == 0 && rangedDespawns.Length == 0)
					enabled = false;
			}
		}
		
		public RangedDespawn RangeDespawn (int prefabIndex, GameObject clone, Transform trs, float range)
		{
			RangedDespawn rangedDespawn = new RangedDespawn(trs.position, range);
			rangedDespawn.go = clone;
			rangedDespawn.trs = trs;
			rangedDespawn.prefab = spawnEntries[prefabIndex].prefab;
			rangedDespawn.prefabIndex = prefabIndex;
			rangedDespawn.previousPos = trs.localPosition;
			rangedDespawns = rangedDespawns.Add_class(rangedDespawn);
			enabled = true;
			return rangedDespawn;
		}

		public virtual void CancelRangedDespawn (RangedDespawn rangedDespawn)
		{
			int indexOfRangedDespawn = rangedDespawns.IndexOf_class(rangedDespawn);
			if (indexOfRangedDespawn != -1)
			{
				rangedDespawns = rangedDespawns.RemoveAt_class(indexOfRangedDespawn);
				if (rangedDespawns.Length == 0 && delayedDespawns.Length == 0)
					enabled = false;
			}
		}
		
		public virtual T SpawnComponent<T> (int prefabIndex, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null)
		{
			SpawnEntry spawnEntry = spawnEntries[prefabIndex];
			while (spawnEntry.cache.Length <= spawnEntry.preload)
				Preload (prefabIndex);
			KeyValuePair<GameObject, Transform> cacheEntry = spawnEntry.cache[0];
			GameObject clone = cacheEntry.Key;
			if (clone == null)
				return default(T);
			spawnEntry.cache = spawnEntry.cache.RemoveAt(0);
			SpawnedEntry entry = new SpawnedEntry(clone, cacheEntry.Value);
			entry.prefab = spawnEntry.prefab;
			entry.prefabIndex = prefabIndex;
			entry.trs.position = position;
			entry.trs.rotation = rotation;
			entry.trs.localScale = spawnEntry.trs.localScale;
			entry.trs.SetParent(parent, true);
			clone.SetActive(true);
			spawnedEntries = spawnedEntries.Add_class(entry);
			return entry.go.GetComponent<T>();
		}
		
		public virtual T Spawn<T> (T prefab, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null)
		{
			return SpawnComponent<T>((prefab as ISpawnable).PrefabIndex, position, rotation, parent);
		}
		
		public virtual GameObject Despawn (SpawnedEntry spawnedEntry)
		{
			return Despawn (spawnedEntry.prefabIndex, spawnedEntry.go, spawnedEntry.trs);
		}
		
		public virtual GameObject Despawn (int prefabIndex, GameObject go, Transform trs)
		{
			if (go == null)
				return null;
			go.SetActive(false);
			trs.SetParent(this.trs, true);
			spawnEntries[prefabIndex].cache = spawnEntries[prefabIndex].cache.Add(new KeyValuePair<GameObject, Transform>(go, trs));
			return go;
		}
		
		public virtual GameObject Preload (int prefabIndex)
		{
			GameObject clone = Instantiate(spawnEntries[prefabIndex].prefab, trs);
			clone.SetActive(false);
			spawnEntries[prefabIndex].createdCount ++;
			clone.name = clone.name.Substring(0, clone.name.Length - 1) + spawnEntries[prefabIndex].createdCount + clone.name[clone.name.Length - 1];
			spawnEntries[prefabIndex].cache = spawnEntries[prefabIndex].cache.Add(new KeyValuePair<GameObject, Transform>(clone, clone.GetComponent<Transform>()));
			return clone;
		}

		[Serializable]
		public class ObjectPoolEntry
		{
			public GameObject prefab;
			public Transform trs;
			[HideInInspector]
			public int prefabIndex;
			[HideInInspector]
			public int createdCount;
			
			public ObjectPoolEntry ()
			{
			}
			
			public ObjectPoolEntry (GameObject prefab, Transform trs, int prefabIndex)
			{
				this.prefab = prefab;
				this.trs = trs;
				this.prefabIndex = prefabIndex;
			}
		}
		
		[Serializable]
		public class SpawnEntry : ObjectPoolEntry
		{
			public int preload;
			public KeyValuePair<GameObject, Transform>[] cache = new KeyValuePair<GameObject, Transform>[0];
			
			public SpawnEntry ()
			{
			}
			
			public SpawnEntry (int preload, KeyValuePair<GameObject, Transform>[] cache)
			{
				this.preload = preload;
				this.cache = cache;
			}
		}
		
		public class SpawnedEntry : ObjectPoolEntry
		{
			public GameObject go;
			
			public SpawnedEntry ()
			{
			}
			
			public SpawnedEntry (GameObject go, Transform trs)
			{
				this.go = go;
				this.trs = trs;
			}
		}
		
		public class DelayedDespawn : SpawnedEntry
		{
			public float timeRemaining;
			
			public DelayedDespawn ()
			{
			}
			
			public DelayedDespawn (float timeRemaining)
			{
				this.timeRemaining = timeRemaining;
			}
		}
		
		public class RangedDespawn : SpawnedEntry
		{
			public Vector2 previousPos;
			public float range;
			
			public RangedDespawn ()
			{
			}
			
			public RangedDespawn (Vector2 previousPos, float range)
			{
				this.previousPos = previousPos;
				this.range = range;
			}
		}
	}
}