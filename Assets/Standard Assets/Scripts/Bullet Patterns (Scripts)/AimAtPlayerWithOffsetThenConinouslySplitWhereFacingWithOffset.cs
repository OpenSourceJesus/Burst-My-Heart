using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[CreateAssetMenu]
	public class AimAtPlayerWithOffsetThenConinouslySplitWhereFacingWithOffset : AimAtPlayerWithOffset
	{
		[MakeConfigurable]
		public float splitOffset;
		public Weapon splitBulletPrefab;
		[MakeConfigurable]
		public float splitDelay;
		
		public override Weapon[] Shoot (Transform spawner, Weapon bulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = base.Shoot(spawner, bulletPrefab);
			foreach (Weapon bullet in output)
				GameManager.Instance.StartCoroutine(SplitAfterDelay (bullet, splitBulletPrefab, splitDelay, bullet.radius + splitBulletPrefab.radius));
			return output;
		}
		
		public override IEnumerator SplitAfterDelay (Weapon bullet, Weapon splitBulletPrefab, float delay, float positionOffset = 0)
		{
			while (true)
			{
				yield return new WaitForSeconds(delay);
				if (!bullet.gameObject.activeSelf)
					yield break;
				yield return Split (bullet, splitBulletPrefab, positionOffset);
			}
		}
		
		public override Vector2 GetSplitDirection (Weapon bullet)
		{
			return bullet.trs.up.Rotate(splitOffset);
		}
	}
}