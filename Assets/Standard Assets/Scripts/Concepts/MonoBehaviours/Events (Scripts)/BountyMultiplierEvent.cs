using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class BountyMultiplierEvent : Event
	{
		public int bountyMultiplier;
		public Timer lifeTimer;

		public override void Begin (params object[] args)
		{
			base.Begin (args);
			if (hasStarted)
			{
				lifeTimer.onFinished += DestroyMe;
				lifeTimer.Reset ();
				lifeTimer.Start ();
				timerVisualizerTrs.gameObject.SetActive(true);
				GameManager.updatables = GameManager.updatables.Add(this);
			}
		}

		public override void DoUpdate ()
		{
			if (hasStarted)
				timerVisualizerTrs.localScale = Vector2.one * lifeTimer.timeRemaining / lifeTimer.duration;
			else
				base.DoUpdate ();
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			GameManager.updatables = GameManager.updatables.Remove(this);
			lifeTimer.onFinished -= DestroyMe;
		}

		public virtual void DestroyMe (params object[] args)
		{
			DestroyMe ();
		}

		public override void ApplyEffect (Player player)
		{
			if (OnlineBattle.isPlaying)
			{
				if (OnlineBattle.localPlayer == player)
					OnlineBattle.Instance.bountyMultiplier *= bountyMultiplier;
				else
				{
					OnlinePlayer onlinePlayer = player as OnlinePlayer;
					onlinePlayer.scoreText.text = "Bounty: " + Mathf.Clamp(onlinePlayer.Score * bountyMultiplier, 1, int.MaxValue);
				}
			}
			else if (Battle.player1 == player)
				Battle.Instance.scoreChangeAmount *= bountyMultiplier;
		}

		public override void UnapplyEffect (Player player)
		{
			if (OnlineBattle.Instance != null)
			{
				if (OnlineBattle.localPlayer == player)
					OnlineBattle.Instance.bountyMultiplier /= bountyMultiplier;
				else
				{
					OnlinePlayer onlinePlayer = player as OnlinePlayer;
					if (onlinePlayer != null)
						onlinePlayer.scoreText.text = "Bounty: " + Mathf.Clamp(onlinePlayer.Score, 1, int.MaxValue);
				}
			}
			else if (Battle.player1 == player)
				Battle.Instance.scoreChangeAmount /= bountyMultiplier;
		}
	}
}