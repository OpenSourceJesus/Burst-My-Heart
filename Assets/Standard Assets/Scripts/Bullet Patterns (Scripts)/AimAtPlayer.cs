﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	[CreateAssetMenu]
	public class AimAtPlayer : BulletPattern
	{
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return HumanPlayer.Instance.body.trs.position - spawner.position;
		}
	}
}