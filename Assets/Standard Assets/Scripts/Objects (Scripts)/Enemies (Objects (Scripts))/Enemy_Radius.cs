using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class Enemy_Radius : Enemy_Follow
	{
		public float triggerDelay;
		public float triggerDuration;
		public Mine minePrefab;
		public LayerMask whatTriggersMe;
		bool isTriggered;

		public override void DoUpdate ()
		{
			if (!isTriggered)
			{
				Transform child;
				for (int i = 1; i < trs.childCount; i ++)
				{
					child = trs.GetChild(i);
					if (Physics2D.OverlapCircle(child.position, minePrefab.radius, whatTriggersMe) != null)
					{
						LayMines ();
						return;
					}
				}
			}
		}

		public virtual void LayMines ()
		{
			isTriggered = true;
			Transform child;
			Mine mine;
			for (int i = 1; i < trs.childCount; i ++)
			{
				child = trs.GetChild(i);
				mine = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Mine>(minePrefab.prefabIndex, child.position, child.rotation, trs.parent);
				mine.player = this;
				mine.StartCoroutine(mine.ActivateRoutine (triggerDelay, triggerDuration));
				mine.onDeath += delegate { isTriggered = false; };
			}
		}

		public override void UpdateGraphics ()
		{
		}
	}
}