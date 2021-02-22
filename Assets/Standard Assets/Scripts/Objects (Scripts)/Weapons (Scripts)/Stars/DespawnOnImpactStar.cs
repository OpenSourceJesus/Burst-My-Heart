using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class DespawnOnImpactStar : Star
	{
		public override void OnCollisionEnter (Collision coll)
		{
			base.OnCollisionEnter (coll);
			ObjectPool.Instance.Despawn (prefabIndex, gameObject, trs);
		}
	}
}