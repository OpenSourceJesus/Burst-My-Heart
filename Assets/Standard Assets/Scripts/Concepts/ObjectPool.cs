﻿using UnityEngine;
using System.Collections.Generic;
using System;
using Extensions;
using Object = UnityEngine.Object;

namespace BMH
{
	public class ObjectPool : SingletonMonoBehaviour<ObjectPool>
	{
		public bool preloadOnAwake = true;
		public Transform trs;
		public SpawnEntry[] spawnEntries = new SpawnEntry[0];
		public DelayedDespawn[] delayedDespawns = new DelayedDespawn[0];
		public RangedDespawn[] rangedDespawns = new RangedDespawn[0];
		
		public override void Awake ()
		{
			base.Awake ();
			instance = this;
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
					// Despawn (delayedDespawn.prefabIndex, delayedDespawn.go, delayedDespawn.trs);
					Destroy(delayedDespawn.go);
					delayedDespawns = delayedDespawns.RemoveAt(i);
					i --;
					if (delayedDespawns.Length == 0 && rangedDespawns.Length == 0)
						enabled = false;
				}
				else
					delayedDespawns[i] = delayedDespawn;
			}
			for (int i = 0; i < rangedDespawns.Length; i ++)
			{
				RangedDespawn rangedDespawn = rangedDespawns[i];
				rangedDespawn.range -= Vector3.Distance(rangedDespawn.trs.position, rangedDespawn.previousPos);
				rangedDespawn.previousPos = rangedDespawn.trs.position;
				if (rangedDespawn.range < 0)
				{
					// Despawn (rangedDespawn.prefabIndex, rangedDespawn.go, rangedDespawn.trs);
					Destroy(rangedDespawn.go);
					rangedDespawns = rangedDespawns.RemoveAt(i);
					i --;
					if (rangedDespawns.Length == 0 && delayedDespawns.Length == 0)
						enabled = false;
				}
				else
					rangedDespawns[i] = rangedDespawn;
			}
		}
		
		public virtual DelayedDespawn DelayDespawn (int prefabIndex, GameObject clone, Transform trs, float delay)
		{
			DelayedDespawn delayedDespawn = new DelayedDespawn(delay);
			delayedDespawn.go = clone;
			delayedDespawn.trs = trs;
			// delayedDespawn.prefab = spawnEntries[prefabIndex].prefab;
			delayedDespawn.prefabIndex = prefabIndex;
			delayedDespawns = delayedDespawns.Add(delayedDespawn);
			enabled = true;
			return delayedDespawn;
		}

		public virtual void CancelDelayedDespawn (DelayedDespawn delayedDespawn)
		{
			int indexOfDelayedDespawn = delayedDespawns.IndexOf(delayedDespawn);
			if (indexOfDelayedDespawn != -1)
			{
				delayedDespawns = delayedDespawns.RemoveAt(indexOfDelayedDespawn);
				if (delayedDespawns.Length == 0 && rangedDespawns.Length == 0)
					enabled = false;
			}
		}
		
		public virtual RangedDespawn RangeDespawn (int prefabIndex, GameObject clone, Transform trs, float range)
		{
			RangedDespawn rangedDespawn = new RangedDespawn(trs.position, range);
			rangedDespawn.go = clone;
			rangedDespawn.trs = trs;
			// rangedDespawn.prefab = spawnEntries[prefabIndex].prefab;
			rangedDespawn.prefabIndex = prefabIndex;
			rangedDespawn.previousPos = trs.position;
			rangedDespawns = rangedDespawns.Add(rangedDespawn);
			enabled = true;
			return rangedDespawn;
		}

		public virtual void CancelRangedDespawn (RangedDespawn rangedDespawn)
		{
			int indexOfRangedDespawn = rangedDespawns.IndexOf(rangedDespawn);
			if (indexOfRangedDespawn != -1)
			{
				rangedDespawns = rangedDespawns.RemoveAt(indexOfRangedDespawn);
				if (rangedDespawns.Length == 0 && delayedDespawns.Length == 0 && this != null)
					enabled = false;
			}
		}
		
		public virtual T SpawnComponent<T> (int prefabIndex, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null)
		{
			SpawnEntry spawnEntry = spawnEntries[prefabIndex];
			if (spawnEntry.cache.Count == 0)
			// while (spawnEntry.cache.Count <= spawnEntry.preload)
				Preload (prefabIndex);
			KeyValuePair<GameObject, Transform> cacheEntry;
			do
			{
				cacheEntry = spawnEntry.cache[0];
				spawnEntry.cache.RemoveAt(0);
			} while (cacheEntry.Key == null);
			cacheEntry.Value.position = position;
			cacheEntry.Value.rotation = rotation;
			cacheEntry.Value.localScale = spawnEntry.trs.localScale;
			cacheEntry.Value.SetParent(parent, true);
			cacheEntry.Key.SetActive(true);
			spawnEntries[prefabIndex] = spawnEntry;
			return cacheEntry.Key.GetComponent<T>();
		}

		public virtual T SpawnComponent<T> (T component, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null) where T : Object
		{
			return (T) Instantiate(component, position, rotation, parent);
		}
		
		public virtual T Spawn<T> (T prefab, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null)
		{
			return SpawnComponent<T>((prefab as ISpawnable).PrefabIndex, position, rotation, parent);
		}
		
		public virtual KeyValuePair<GameObject, Transform> Despawn (SpawnedEntry spawnedEntry)
		{
			return Despawn (spawnedEntry.prefabIndex, spawnedEntry.go, spawnedEntry.trs);
		}
		
		public virtual KeyValuePair<GameObject, Transform> Despawn (int prefabIndex, GameObject go, Transform trs)
		{
			// if (go == null)
			// 	return new KeyValuePair<GameObject, Transform>();
			go.SetActive(false);
			trs.SetParent(this.trs, true);
			KeyValuePair<GameObject, Transform> output = new KeyValuePair<GameObject, Transform>(go, trs);
			spawnEntries[prefabIndex].cache.Add(output);
			return output;
		}
		
		public virtual KeyValuePair<GameObject, Transform> Preload (int prefabIndex)
		{
			KeyValuePair<GameObject, Transform> output;
			SpawnEntry spawnEntry = spawnEntries[prefabIndex];
			GameObject clone = Instantiate(spawnEntry.prefab, trs);
			clone.SetActive(false);
			// spawnEntry.createdCount ++;
			// clone.name = clone.name.Insert(clone.name.Length - 1, "" + spawnEntry.createdCount);
			output = new KeyValuePair<GameObject, Transform>(clone, clone.GetComponent<Transform>());
			spawnEntry.cache.Add(output);
			spawnEntries[prefabIndex] = spawnEntry;
			return output;
		}

		[Serializable]
		public class ObjectPoolEntry
		{
			public GameObject prefab;
			public Transform trs;
			[HideInInspector]
			public int prefabIndex;
			// [HideInInspector]
			// public int createdCount;
			
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
			public List<KeyValuePair<GameObject, Transform>> cache = new List<KeyValuePair<GameObject, Transform>>();
			
			public SpawnEntry ()
			{
			}
			
			public SpawnEntry (int preload, List<KeyValuePair<GameObject, Transform>> cache)
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
			public float duration;
			
			public DelayedDespawn ()
			{
			}
			
			public DelayedDespawn (float duration)
			{
				timeRemaining = duration;
				this.duration = duration;
			}
		}
		
		public class RangedDespawn : SpawnedEntry
		{
			public Vector3 previousPos;
			public float range;
			
			public RangedDespawn ()
			{
			}
			
			public RangedDespawn (Vector3 previousPos, float range)
			{
				this.previousPos = previousPos;
				this.range = range;
			}
		}
	}
}