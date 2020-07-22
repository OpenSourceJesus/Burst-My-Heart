using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[CreateAssetMenu]
	public class AimWhereFacingThenRotate : AimWhereFacing
	{
		[MakeConfigurable]
		public float rotate;
		
		public override Weapon[] Shoot (Transform spawner, Weapon bulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = base.Shoot (spawner, bulletPrefab, positionOffset);
			spawner.up = spawner.up.Rotate(rotate);
			return output;
		}
	}
}