using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class WindEvent : BountyMultiplierEvent
	{
		public float windSpeed;

		public override void ApplyEffect (Player player)
		{
			base.ApplyEffect (player);
            if (GameManager.GetSingleton<OnlineBattle>() == null || OnlineBattle.localPlayer == player)
            {
				player.body.extraVelocity = trs.up * windSpeed;
				player.weapon.extraVelocity = trs.up * windSpeed;
            }
		}

		public override void UnapplyEffect (Player player)
		{
			base.UnapplyEffect (player);
            if (GameManager.GetSingleton<OnlineBattle>() == null || OnlineBattle.localPlayer == player)
            {
				player.body.extraVelocity = Vector2.zero;
				player.weapon.extraVelocity = Vector2.zero;
            }
		}
	}
}