using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class DeathTrailEvent : LastSurvivorEvent
	{
		public Timer createStarTimer;
		public float activateStarDelay;
		public Mine starPrefab;

		public override void Begin (params object[] args)
		{
			base.Begin (args);
			createStarTimer.onFinished += CreateStars;
		}

		public virtual void CreateStars (params object[] args)
		{
			foreach (Player participator in participators)
			{
				GameManager.GetSingleton<ObjectPool>().SpawnComponent<Mine>(starPrefab.prefabIndex, participator.body.trs.position, participator.body.trs.rotation);
				GameManager.GetSingleton<ObjectPool>().SpawnComponent<Mine>(starPrefab.prefabIndex, participator.weapon.trs.position, participator.weapon.trs.rotation);
			}
		}

		public virtual void OnDestroy ()
		{
			createStarTimer.onFinished -= CreateStars;
		}
	}
}
