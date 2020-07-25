using UnityEngine;
using System;
using Extensions;

namespace BMH
{
	public class KillingQuest : Quest
	{
		public Enemy[] enemiesToKill = new Enemy[0];
		Enemy[] enemiesToKillRemaining = new Enemy[0];
		public bool changePlayerHealth;
		public int playerHp;
		
		public override void Start ()
		{
			base.Start ();
			enemiesToKillRemaining = enemiesToKill;
			// foreach (Enemy enemy in enemiesToKill)
			// 	enemy.onDeath += delegate { OnDeath(enemy); };
		}

		public virtual void OnDeath (Enemy enemy)
		{
			enemiesToKillRemaining = enemiesToKillRemaining.Remove(enemy);
			if (enemiesToKillRemaining.Length == 0)
				IsComplete = true;
		}

		public override void Begin ()
		{
			base.Begin ();
			if (changePlayerHealth)
			{
				// GameManager.GetSingleton<Player>().Hp = playerHp;
				// for (int i = playerHp; i < GameManager.GetSingleton<Player>().Hp; i ++)
					// Destroy(GameManager.GetSingleton<Player>().lifeIconsParent.GetChild(0).gameObject);
			}
		}
	}
}