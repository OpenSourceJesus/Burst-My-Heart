using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[CreateAssetMenu]
	public class AimAtPlayerWithOffsetThenDespawnAndSplitInArcAimedTowardsPlayerWithOffset : AimAtPlayerWithOffset
	{
		[MakeConfigurable]
		public float splitOffset;
		public Weapon splitBulletPrefab;
		[MakeConfigurable]
		public float splitDelay;
		[MakeConfigurable]
		public float splitArc;
		[MakeConfigurable]
		public float splitNumber;
		
		public override Weapon[] Shoot (Transform spawner, Weapon bulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = base.Shoot (spawner, bulletPrefab);
			foreach (Weapon bullet in output)
				GameManager.Instance.StartCoroutine(SplitAfterDelay (bullet, splitBulletPrefab, splitDelay, bullet.radius + splitBulletPrefab.radius));
			return output;
		}
		
		public override Weapon[] Split (Weapon bullet, Vector2 direction, Weapon splitBulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = new Weapon[0];
			float toPlayer = (HumanPlayer.Instance.body.trs.position - bullet.trs.position).GetFacingAngle();
			for (float splitAngle = toPlayer - splitArc / 2 + splitOffset; splitAngle < toPlayer + splitArc / 2 + splitOffset; splitAngle += splitArc / splitNumber)
				output = base.Split (bullet, VectorExtensions.FromFacingAngle(splitAngle), splitBulletPrefab, positionOffset);
			ObjectPool.Instance.Despawn (bullet.prefabIndex, bullet.gameObject, bullet.trs);
			return output;
		}
	}
}