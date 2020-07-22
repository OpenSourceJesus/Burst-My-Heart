using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class WindEvent : BountyMultiplierEvent
	{
		public new static WindEvent instance;
		public float windSpeed;

		public override void ApplyEffect (Player player)
		{
            if (GameManager.GetSingleton<OnlineBattle>() == null || GameManager.GetSingleton<Player>() == player)
            {
				player.body.extraVelocity = trs.up * windSpeed;
				player.weapon.extraVelocity = trs.up * windSpeed;
            }
		}

		public override void UnapplyEffect (Player player)
		{
            if (GameManager.GetSingleton<OnlineBattle>() == null || GameManager.GetSingleton<Player>() == player)
            {
				player.body.extraVelocity = Vector2.zero;
				player.weapon.extraVelocity = Vector2.zero;
            }
		}

		public new static WindEvent GetInstance ()
		{
			if (instance == null)
				instance = FindObjectOfType<WindEvent>();
			return instance;
		}
	}
}