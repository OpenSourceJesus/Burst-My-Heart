using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[CreateAssetMenu]
	public class AimWhereFacingWithOffsetThenTargetPlayer : AimWhereFacingThenTargetPlayer
	{
		[MakeConfigurable]
		public float shootOffset;
		
		public override Vector2 GetShootDirection (Transform spawner)
		{
			return VectorExtensions.Rotate(base.GetShootDirection(spawner), shootOffset);
		}
	}
}