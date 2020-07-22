using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace BMH
{
	public class Enemy_Bounce : Enemy
	{
		public override void OnAwaken ()
		{
			base.OnAwaken ();
			Move (Random.insideUnitCircle.normalized);
		}

		public override void HandleMovement ()
		{
			Move (rigid.velocity.normalized);
		}
	}
}