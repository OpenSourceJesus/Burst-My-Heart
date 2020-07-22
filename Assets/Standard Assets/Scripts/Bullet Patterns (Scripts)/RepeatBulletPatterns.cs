using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[CreateAssetMenu]
	public class RepeatBulletPatterns : BulletPattern
	{
		[MakeConfigurable]
		public int repeatCount;
		public BulletPattern[] bulletPatterns;

		public override void Init (Transform spawner)
		{
			base.Init (spawner);
			foreach (BulletPattern bulletPattern in bulletPatterns)
				bulletPattern.Init (spawner);
		}

		public override Weapon[] Shoot (Transform spawner, Weapon bulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = new Weapon[0];
			for (int i = 0; i < repeatCount; i ++)
			{
				foreach (BulletPattern bulletPattern in bulletPatterns)
					output = output.AddRange_class(bulletPattern.Shoot (spawner, bulletPrefab, positionOffset));
			}
			return output;
		}
		
		public override Weapon[] Shoot (Vector2 spawnPos, Vector2 direction, Weapon bulletPrefab, float positionOffset = 0)
		{
			Weapon[] output = new Weapon[0];
			for (int i = 0; i < repeatCount; i ++)
			{
				foreach (BulletPattern bulletPattern in bulletPatterns)
					output = output.AddRange_class(bulletPattern.Shoot (spawnPos, direction, bulletPrefab, positionOffset));
			}
			return output;
		}
	}
}