using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class JailEvent : Event
	{
		public override void AddPlayer (Player player)
		{
			base.AddPlayer (player);
			if (hasStarted)
				player.body.Death ();
		}

		public override void RemovePlayer (Player player)
		{
			base.RemovePlayer (player);
			if (!isDestroyed && hasStarted && !player.body.isDead)
				player.body.Death ();
		}
	}
}