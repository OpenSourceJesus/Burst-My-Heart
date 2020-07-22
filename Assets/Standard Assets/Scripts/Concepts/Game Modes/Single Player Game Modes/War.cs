using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace BMH
{
	[ExecuteAlways]
	public class War : SinglePlayerGameMode
	{
		public new static War instance;
		public float deathPenalty = 10;
		public float addToIdealDifficulty = 1;
		public float idealDifficulty = 1;
		float difficulty;
		[HideInInspector]
		public float minEnemyDifficulty;
		public EnemyEntry[] enemyEntries = new EnemyEntry[0];
		Enemy[] enemies = new Enemy[0];

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				minEnemyDifficulty = Mathf.Infinity;
				foreach (EnemyEntry enemyEntry in enemyEntries)
				{
					if (enemyEntry.enemyPrefab.difficulty < minEnemyDifficulty)
						minEnemyDifficulty = enemyEntry.enemyPrefab.difficulty;
				}
				return;
			}
#endif
			base.Awake ();
			if (Highscore > deathPenalty)
			{
				SetScore (Highscore - deathPenalty);
				idealDifficulty += score;
			}
			// ObjectPool.GetInstance ();
			SpawnEnemies ();
		}

		public virtual void SpawnEnemies ()
		{
			EnemyEntry[] _enemyEntries = enemyEntries;
			EnemyEntry nextEnemyEntry = null;
			int indexOfNextEnemyEntry;
			while (idealDifficulty - difficulty >= minEnemyDifficulty)
			{
				while (true)
				{
					indexOfNextEnemyEntry = Random.Range(0, _enemyEntries.Length);
					nextEnemyEntry = _enemyEntries[indexOfNextEnemyEntry];
					if (difficulty + nextEnemyEntry.enemyPrefab.difficulty > idealDifficulty)
						_enemyEntries = _enemyEntries.RemoveAt_class(indexOfNextEnemyEntry);
					else
						break;
				}
				difficulty += nextEnemyEntry.enemyPrefab.difficulty;
				StartCoroutine(nextEnemyEntry.Spawn ());
			}
		}

		public virtual void OnEnemyKilled (Player killer, Body victim)
		{
			victim.onDeath -= OnEnemyKilled;
			Enemy victimEnemy = victim.player as Enemy;
			enemies = enemies.Remove_class(victimEnemy);
			float victimDifficulty = victimEnemy.difficulty;
			difficulty -= victimDifficulty;
			AddScore (victimDifficulty + addToIdealDifficulty);
			idealDifficulty += addToIdealDifficulty;
			SpawnEnemies ();
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			StopAllCoroutines();
			foreach (Enemy enemy in enemies)
				enemy.body.onDeath -= OnEnemyKilled;
		}

		[Serializable]
		public class EnemyEntry
		{
			public Enemy enemyPrefab;
			public float minCreateRangeFromWall;
			public float minCreateRangeFromEnemy;
			
			public virtual IEnumerator Spawn ()
			{
				Vector2 spawnPosition;
				bool isValidSpawnPosition;
				while (true)
				{
					spawnPosition = Random.insideUnitCircle * (GameManager.GetSingleton<Area>().trs.localScale.x / 2 - minCreateRangeFromWall);
					isValidSpawnPosition = true;
					foreach (Enemy enemy in War.instance.enemies)
					{
						if (Vector2.Distance(enemy.trs.position, spawnPosition) < minCreateRangeFromEnemy)
						{
							isValidSpawnPosition = false;
							break;
						}
					}
					if (!isValidSpawnPosition)
						yield return new WaitForEndOfFrame();
					else
						break;
				}
				// Enemy enemy = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Enemy>(enemyPrefab.prefabIndex, position);
				Enemy newEnemy = (Enemy) GameManager.Clone (enemyPrefab, spawnPosition, Quaternion.identity);
				newEnemy.UpdateGraphics ();
				newEnemy.body.onDeath += War.instance.OnEnemyKilled;
				War.instance.enemies = War.instance.enemies.Add_class(newEnemy);
			}
		}
	}
}
