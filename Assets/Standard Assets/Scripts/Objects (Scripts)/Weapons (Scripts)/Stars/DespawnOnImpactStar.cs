using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class DespawnOnImpactStar : Star
	{
		public override void OnCollisionEnter2D (Collision2D coll)
		{
			base.OnCollisionEnter2D (coll);
			ObjectPool.Instance.Despawn (prefabIndex, gameObject, trs);
		}
	}
}