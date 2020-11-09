using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Extensions;

namespace BMH
{
	public class Star : Weapon
	{
		public override void OnCollisionEnter2D (Collision2D coll)
		{
			base.OnCollisionEnter2D (coll);
			Body collidedBody = coll.gameObject.GetComponent<Body>();
			if (collidedBody == null || collidedBody.Hp == MathfExtensions.NULL_FLOAT || collidedBody.player == null || collidedBody.player.owner == player.owner)
			{
				if (collidedBody != null && collidedBody.player == null && Hunt.Instance != null)
					collidedBody.Death (player);
				return;
			}
			collidedBody.Death (player);
		}
	}
}