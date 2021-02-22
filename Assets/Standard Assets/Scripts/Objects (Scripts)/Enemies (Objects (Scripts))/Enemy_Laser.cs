using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class Enemy_Laser : Enemy_Follow
	{
		public float laserPartSpacing;
		public float laserRange;
		public float stopDistanceSqr;
		public Timer shootTimer;
		public float laserDelay;
		public float laserDuration;
		public Mine laserPartPrefab;
		public Transform spawnPoint;
		public LayerMask whatICantSpawnIn;
		Transform mineParent;

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			if (enemyGroup != null)
				mineParent = enemyGroup.trs;
			shootTimer.onFinished += Shoot;
			shootTimer.Start ();
		}

		public override void DoUpdate ()
		{
			base.DoUpdate ();
			body.trs.up = HumanPlayer.Instance.body.trs.position - body.trs.position;
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			shootTimer.Stop ();
			shootTimer.onFinished -= Shoot;
			shootTimer.timeRemaining = 0;
		}

		public override void HandleMovement ()
		{
			if ((HumanPlayer.Instance.body.trs.position - body.trs.position).sqrMagnitude < stopDistanceSqr)
				Move (body.trs.position - HumanPlayer.Instance.body.trs.position);
			else
				base.HandleMovement ();
		}

		public virtual void Shoot (params object[] args)
		{
			for (float distance = 0; distance < laserRange; distance += laserPartSpacing)
			{
				if (Physics2D.OverlapCircle(spawnPoint.position + body.trs.up * distance, laserPartPrefab.radius, whatICantSpawnIn) == null)
				{
					Mine laserPart = ObjectPool.Instance.SpawnComponent<Mine>(laserPartPrefab.prefabIndex, spawnPoint.position + body.trs.up * distance, spawnPoint.rotation, mineParent);
					laserPart.player = this;
					laserPart.StartCoroutine(laserPart.ActivateRoutine (laserDelay, laserDuration));
					laserPart.onCollide += OnLaserPartCollide;
				}
			}
		}

		public virtual void OnLaserPartCollide (Collision coll, Weapon weapon)
		{
			weapon.onCollide -= OnLaserPartCollide;
			if (laserDuration != 0)
				ObjectPool.Instance.CancelDelayedDespawn (weapon.delayedDespawn);
			ObjectPool.Instance.Despawn (weapon.prefabIndex, weapon.gameObject, weapon.trs);
		}

		public override void UpdateGraphics ()
		{
		}
	}
}