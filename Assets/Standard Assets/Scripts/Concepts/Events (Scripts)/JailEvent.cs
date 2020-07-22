using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class JailEvent : Event
	{
		public new static JailEvent instance;

		public override void AddPlayer (Player player)
		{
			base.AddPlayer (player);
			if (hasStarted)
				player.body.Death ();
		}

		public override void RemovePlayer (Player player)
		{
			base.RemovePlayer (player);
			if (hasStarted && !player.body.isDead)
				player.body.Death ();
		}

		public new static JailEvent GetInstance ()
		{
			if (instance == null)
				instance = FindObjectOfType<JailEvent>();
			return instance;
		}
	}
}