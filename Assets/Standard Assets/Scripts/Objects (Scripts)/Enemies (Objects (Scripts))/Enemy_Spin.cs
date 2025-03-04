﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class Enemy_Spin : Enemy_Follow
	{
		public float spinRate;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				UpdateGraphics ();
				return;
			}
#endif
			base.Awake ();
			weapon.onCollide += SwitchSpinDirection;
		}

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			if (!isTrap && Random.value < 0.5f)
				spinRate *= -1;
		}

		public virtual void SwitchSpinDirection (Collision coll, Weapon weapon)
		{
			spinRate *= -1;
		}

		public override void DoUpdate ()
		{
			base.DoUpdate ();
			trs.eulerAngles += Vector3.forward * spinRate * Time.deltaTime;
			UpdateGraphics ();
		}

		public override void HandleMovement ()
		{
			if (isTrap)
				return;
			if ((HumanPlayer.Instance.body.trs.position - trs.position).sqrMagnitude < maxBodyToWeaponDistanceSqr)
				Move (trs.position - HumanPlayer.Instance.body.trs.position);
			else
				base.HandleMovement ();
		}

		public override void UpdateGraphics ()
		{
            lineRenderer.SetPosition(0, body.trs.position);
            lineRenderer.SetPosition(1, weapon.trs.position);
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			weapon.onCollide -= SwitchSpinDirection;
		}

		public override void OnAwaken ()
		{
			base.OnAwaken ();
			lineRenderer.startColor = lineRenderer.startColor.MultiplyAlpha(4);
			lineRenderer.endColor = lineRenderer.endColor.MultiplyAlpha(4);
		}
	}
}