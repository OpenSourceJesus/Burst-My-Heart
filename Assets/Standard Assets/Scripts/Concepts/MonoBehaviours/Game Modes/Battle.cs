using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;

namespace BMH
{
    public class Battle : GameMode
    {
    	public Text scoreText1;
    	public Text scoreText2;
		public Animator scoreAnimator1;
		public Animator scoreAnimator2;
    	bool isPlaying;
		public float EventChance
		{
			get
			{
				return SaveAndLoadManager.GetValue<float>("Event chance", 0.2f);
			}
			set
			{
				SaveAndLoadManager.SetValue("Event chance", value);
			}
		}
		[HideInInspector]
		public int scoreChangeAmount = 1;
		public static Player player1;
		public static Player player2;

		public override void Awake ()
		{
			player1 = GameManager.GetSingleton<Player>();
			if (player1.owner != GameManager.GetSingleton<GameManager>().teams[0])
				player1 = player1.owner.opponent.representative;
			player2 = player1.owner.opponent.representative;
			scoreText1.text = "" + player1.Score;
			scoreText2.text = "" + player2.Score;
			if (player1.WinsInARow > 0)
				scoreAnimator1.enabled = true;
			else if (player2.WinsInARow > 0)
				scoreAnimator2.enabled = true;
			Player.players = FindObjectsOfType<Player>();
			foreach (Player _player in Player.players)
				_player.body.onDeath += OnDeath;
			GameManager.GetSingleton<GameManager>().PauseGame (1);
			pauseInstructionsObj.SetActive(!HasPaused);
			if (EventChance > Random.value)
			{
				for (int i = 0; i < eventPrefabs.Length; i ++)
				{
					if (!eventPrefabs[i].Enabled)
					{
						eventPrefabs = eventPrefabs.RemoveAt(i);
						if (eventPrefabs.Length == 0)
							return;
						i --;
					}
				}
				Event newEvent = MakeEvent (eventPrefabs[Random.Range(0, eventPrefabs.Length)], GameManager.GetSingleton<Area>().trs.position, Random.value * 360);
				DestroyImmediate(newEvent.collider);
				foreach (Player _player in Player.players)
					newEvent.AddPlayer(_player);
				newEvent.Begin ();
				BountyMultiplierEvent bountyMultiplierEvent = newEvent as BountyMultiplierEvent;
				if (bountyMultiplierEvent != null)
					bountyMultiplierEvent.lifeTimer.Stop ();
			}
		}

		public virtual void OnDeath (Player killer, Body victim)
		{
			if (!isPlaying || !(victim.player.owner.representatives.Contains(null) || victim.player.owner.representatives.Length == 1))
				return;
			isPlaying = false;
			killer.Score += (uint) scoreChangeAmount;
			killer.owner.opponent.representative.ChangeWinsInARow (false);
			killer.ChangeWinsInARow (true);
			if (Player.AutoBalanceTeams && killer.WinsInARow >= 2)
			{
				if (killer.MoveSpeedMultiplier <= killer.owner.opponent.representative.MoveSpeedMultiplier)
				{
					foreach (Player player in killer.owner.representatives)
						player.MoveSpeedMultiplier = Mathf.Clamp(player.MoveSpeedMultiplier - player.autoBalanceMoveSpeedMultiplier, player.autoBalanceMoveSpeedMultiplier, player.defaultMoveSpeedMultiplier);
				}
				else
				{
					foreach (Player player in killer.owner.opponent.representatives)
						player.MoveSpeedMultiplier = Mathf.Clamp(player.MoveSpeedMultiplier + player.autoBalanceMoveSpeedMultiplier, 0, player.defaultMoveSpeedMultiplier);
				}
			}
		}

		public override void OnDestroy ()
		{
			foreach (Player player in Player.players)
				player.body.onDeath -= OnDeath;
		}

		public virtual void Begin ()
		{
			GameManager.GetSingleton<GameManager>().PauseGame (-1000);
			isPlaying = true;
			if (!HasPaused)
				pauseInstructionsObj.SetActive(true);
		}
    }
}