using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Extensions;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace BMH
{
	// [ExecuteAlways]
	[DisallowMultipleComponent]
	public class Survival : SinglePlayerGameMode
	{
		public new static Survival instance;
		public EnemyEntry[] enemyEntries;
		public int deathPenalty = 20;
		Heart heart;
		public LayerMask whatEnemiesCantSpawnIn;
		public Enemy[] startingEnemies;
		
		public override void Awake ()
		{
			base.Awake ();
			instance = this;
			foreach (Enemy enemy in startingEnemies)
				SpawnEnemy (enemy);
			if (Highscore > deathPenalty)
			{
				SetScore (Highscore - deathPenalty);
				foreach (EnemyEntry enemyEntry in enemyEntries)
				{
					if (enemyEntry.enabled)
					{
						enemyEntry.spawnTimer.onFinished += delegate { SpawnEnemy (enemyEntry.enemy); };
						for (float time = 0; time < score; time += enemyEntry.spawnTimer.duration)
							SpawnEnemy (enemyEntry.enemy);
						enemyEntry.spawnTimer.timeRemaining = enemyEntry.spawnTimer.duration - (enemyEntry.spawnTimer.duration % score);
						enemyEntry.spawnTimer.Start ();
					}
				}
			}
			else
			{
				foreach (EnemyEntry enemyEntry in enemyEntries)
				{
					if (enemyEntry.enabled)
					{
						enemyEntry.spawnTimer.onFinished += delegate { SpawnEnemy (enemyEntry.enemy); };
						enemyEntry.spawnTimer.Reset ();
						enemyEntry.spawnTimer.Start ();
					}
				}
			}
			heart = (Heart) GameManager.GetSingleton<HumanPlayer>().body;
			Player.players = new Player[] { GameManager.GetSingleton<HumanPlayer>() };
		}
		
		public override void DoUpdate ()
		{
			base.DoUpdate ();
			AddScore (Time.deltaTime);
		}

		public virtual void SpawnEnemy (Enemy enemy)
		{
			Vector2 spawnPosition = new Vector2();
			do
			{
				spawnPosition = Random.insideUnitCircle * (GameManager.GetSingleton<Area>().trs.localScale.x / 2 - enemy.radius);
			}
			while (Physics2D.OverlapCircle(spawnPosition, enemy.radius, whatEnemiesCantSpawnIn) != null);
			Instantiate(enemy, spawnPosition, Quaternion.identity);
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			foreach (EnemyEntry enemyEntry in enemyEntries)
			{
				if (enemyEntry.enabled)
				{
					enemyEntry.spawnTimer.Stop ();
					enemyEntry.spawnTimer.onFinished -= delegate { SpawnEnemy (enemyEntry.enemy); };
				}
			}
		}

	    public new static Survival GetInstance ()
	    {
	    	if (instance == null)
	    		instance = FindObjectOfType<Survival>();
    		return instance;
	    }

		[Serializable]
		public class EnemyEntry
		{
			public bool enabled;
			public Enemy enemy;
			public Timer spawnTimer;
		}
	}
}