using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class LastSurvivorEvent : JailEvent
	{
		[HideInInspector]
		public uint reward;

		public override void Begin (params object[] args)
		{
			base.Begin (args);
			if (hasStarted)
			{
				foreach (Player participator in participators)
					reward += (participator as OnlinePlayer).Score;
			}
		}

		public override void RemovePlayer (Player player)
		{
			base.RemovePlayer (player);
			if (hasStarted && participators.Length == 1)
				(participators[0] as OnlinePlayer).SendChangeScoreMessage (reward);
		}
	}
}
