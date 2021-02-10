using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;

namespace BMH
{
	public class Enemy_Shoot : Enemy_Follow
	{
		public float stopDistanceSqr;
		public float shootIntervalsMultiplier;
		public ShootEntry[] shootEntries;
		Transform bulletParent;

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			if (enemyGroup != null)
				bulletParent = enemyGroup.trs;
			for (int i = 0; i < shootEntries.Length; i ++)
			{
				ShootEntry shootEntry = shootEntries[i];
				if (!shootEntry.enabled)
				{
					shootEntries = shootEntries.RemoveAt(i);
					i --;
				}
				else
				{
					shootEntry.shootTimer.duration = shootEntry.defaultShootInterval * shootIntervalsMultiplier;
					shootEntry.shootTimer.onFinished += _Shoot;
					shootEntry.shootTimer.args = new object[] { shootEntry };
					shootEntry.bulletPattern.Init (shootEntry.spawner);
					shootEntry.shootTimer.Start ();
				}
			}
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			foreach (ShootEntry shootEntry in shootEntries)
				shootEntry.shootTimer.onFinished -= _Shoot;
		}

		public override void DoUpdate ()
		{
			if (isTrap)
				return;
			base.DoUpdate ();
			body.trs.up = HumanPlayer.Instance.body.trs.position - body.trs.position;
		}

		public virtual void _Shoot (params object[] args)
		{
			Shoot (args);
		}

		public virtual Weapon[] Shoot (params object[] args)
		{
			ShootEntry shootEntry = (ShootEntry) args[0];
			float positionOffset = shootEntry.positionOffsetOverride;
			if (!shootEntry.overridePositionOffset)
			{
				positionOffset = shootEntry.bulletPrefab.radius;
				if (shootEntry.spawner == body.trs)
					positionOffset += radius;
			}
			Weapon[] bullets = shootEntry.bulletPattern.Shoot(shootEntry.spawner, shootEntry.bulletPrefab, positionOffset);
			foreach (Weapon bullet in bullets)
			{
				bullet.player = this;
				bullet.Move (bullet.trs.up);
				bullet.trs.SetParent(bulletParent);
			}
			return bullets;
		}

		public override void HandleMovement ()
		{
			if ((HumanPlayer.Instance.body.trs.position - body.trs.position).sqrMagnitude < stopDistanceSqr)
				Move (body.trs.position - HumanPlayer.Instance.body.trs.position);
			else
				base.HandleMovement ();
		}

		public override void UpdateGraphics ()
		{
		}
	
		[Serializable]
		public class ShootEntry
		{
			public BulletPattern bulletPattern;
			public Weapon bulletPrefab;
			public Timer shootTimer;
			public float defaultShootInterval;
			public Transform spawner;
			public bool overridePositionOffset;
			public float positionOffsetOverride;
			public bool enabled = true;
		}
	}
}