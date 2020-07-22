using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace BMH
{
	public class Enemy_Bounce_Oscillate : Enemy
	{
		Vector2 moveDirection;

		public override void OnAwaken ()
		{
			base.OnAwaken ();
			moveDirection = Random.insideUnitCircle.normalized;
		}

		public override void HandleMovement ()
		{
			Move (moveDirection);
		}

		public override void OnCollisionEnter2D (Collision2D coll)
		{
			base.OnCollisionEnter2D (coll);
			moveDirection *= -1;
		}
	}
}