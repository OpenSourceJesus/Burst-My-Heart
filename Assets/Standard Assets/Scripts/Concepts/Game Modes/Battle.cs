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
		public int scoreChangeMultiplier = 1;

		public override void Awake ()
		{
			if (Player.players == null)
				Player.players = FindObjectsOfType<Player>();
			// enabled = false;
			if (!GameManager.GetSingleton<Player>().name.Contains("1"))
			{
				Player player = GameManager.GetSingleton<Player>();
				GameManager.singletons.Remove(typeof(Player));
				GameManager.singletons.Add(typeof(Player), player.owner.opponent.representative);
			}
			scoreText1.text = "" + GameManager.GetSingleton<Player>().Score;
			scoreText2.text = "" + GameManager.GetSingleton<Player>().owner.opponent.representative.Score;
			if (GameManager.GetSingleton<Player>().WinsInARow > 0)
				scoreAnimator1.enabled = true;
			else if (GameManager.GetSingleton<Player>().owner.opponent.representative.WinsInARow > 0)
				scoreAnimator2.enabled = true;
    		Player.players = FindObjectsOfType<Player>();
			foreach (Player player in Player.players)
			{
				player.body.onDeath += OnDeath;
				player.owner.representatives = player.owner.representatives.Add_class(player);
			}
			GameManager.GetSingleton<GameManager>().PauseGame (1);
			pauseInstructionsObj.SetActive(!HasPaused);
			if (EventChance > Random.value)
			{
				for (int i = 0; i < eventPrefabs.Length; i ++)
				{
					if (!eventPrefabs[i].Enabled)
					{
						eventPrefabs = eventPrefabs.RemoveAt_class(i);
						if (eventPrefabs.Length == 0)
							return;
						i --;
					}
				}
				Event newEvent = MakeEvent (eventPrefabs[Random.Range(0, eventPrefabs.Length)], GameManager.GetSingleton<Area>().trs.position, Random.value * 360);
				DestroyImmediate(newEvent.collider);
				foreach (Player player in Player.players)
					newEvent.AddPlayer(player);
				newEvent.Begin ();
				BountyMultiplierEvent bountyMultiplierEvent = newEvent as BountyMultiplierEvent;
				if (bountyMultiplierEvent != null)
					bountyMultiplierEvent.lifeTimer.Stop ();
			}
		}

		public virtual void OnDeath (Player killer, Body victim)
		{
			if (!isPlaying)
				return;
			isPlaying = false;
			killer.Score += (uint) scoreChangeMultiplier;
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
			{
				player.body.onDeath -= OnDeath;
				player.owner.representatives = player.owner.representatives.Remove_class(player);
			}
		}

		public virtual void Begin ()
		{
			GameManager.GetSingleton<GameManager>().PauseGame (-1);
			isPlaying = true;
			if (!HasPaused)
				pauseInstructionsObj.SetActive(true);
		}
    }
}