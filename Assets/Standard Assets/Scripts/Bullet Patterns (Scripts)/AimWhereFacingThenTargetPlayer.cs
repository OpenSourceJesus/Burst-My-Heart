using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	[CreateAssetMenu]
	public class AimWhereFacingThenTargetPlayer : AimWhereFacing
	{
		[MakeConfigurable]
		public float retargetTime;
		
		public override Weapon[] Shoot (Transform spawner, Weapon bulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = base.Shoot (spawner, bulletPrefab, positionOffset);
			foreach (Weapon bullet in output)
				GameManager.GetSingleton<GameManager>().StartCoroutine(RetargetAfterDelay (bullet, retargetTime));
			return output;
		}
		
		public override Vector2 GetRetargetDirection (Weapon bullet)
		{
			return GameManager.GetSingleton<HumanPlayer>().body.trs.position - bullet.trs.position;
		}
	}
}