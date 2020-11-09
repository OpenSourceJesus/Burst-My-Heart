using UnityEngine;
using UnityEngine.UI;
using Extensions;
using PlayerIOClient;

namespace BMH
{
	public class OnlinePlayer : HumanPlayer, ISpawnable
	{
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public int playerId;
		public MonoBehaviour[] disableMonoBehavioursIfIAmToo;
		uint score;
		public override uint Score
		{
			get
			{
				return score;
			}
			set
			{
				score = value;
			}
		}
		public float scoreTextHeight;
		public RectTransform scoreTextRectTrs;
		public Text scoreText;
		public static OnlinePlayer localPlayer;
		public float minMoveDistToSync;
		Vector2 bodySyncedPosition = VectorExtensions.NULL2;
		Vector2 weaponSyncedPosition = VectorExtensions.NULL2;

		public override void Awake ()
		{
			base.Awake ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			body.onDeath += OnDeath;
			scoreTextRectTrs.localScale = Vector3.one / lengthVisualizerTrs.localScale.x;
			scoreTextRectTrs.eulerAngles = Vector3.zero;
			scoreTextRectTrs.position = lengthVisualizerTrs.position + Vector3.up * scoreTextHeight;
		}

		public override void DoUpdate ()
		{
			body.DoUpdate ();
			weapon.DoUpdate ();
			if (InputManager.inputters[0].GetButtonDown("Switch Positions") && CanSwitchPositions)
				SwitchPositions ();
			UpdateGraphics ();
			if (Vector2.Distance(body.trs.position, bodySyncedPosition) >= minMoveDistToSync)
			{
				bodySyncedPosition = body.trs.position;
				NetworkManager.connection.Send("Move Transform", true, bodySyncedPosition.x, bodySyncedPosition.y);
			}
			if (Vector2.Distance(weapon.trs.position, weaponSyncedPosition) >= minMoveDistToSync)
			{
				weaponSyncedPosition = weapon.trs.position;
				NetworkManager.connection.Send("Move Transform", false, weaponSyncedPosition.x, weaponSyncedPosition.y);
			}
		}

		public virtual void OnDeath (Player killer, Body victim)
		{
			if (killer != null)
			{
				OnlinePlayer onlineKiller = killer as OnlinePlayer;
				if (OnlineBattle.localPlayer.playerId == onlineKiller.playerId)
				{
					onlineKiller.ChangeScore ((uint) Mathf.Clamp(victim.player.Score, 1, int.MaxValue));
					SendChangeScoreMessage ((uint) Mathf.Clamp(score * OnlineBattle.Instance.bountyMultiplier, 1, int.MaxValue));
				}
			}
			if (OnlineBattle.localPlayer == this)
			{
				if (ArchivesManager.player1AccountData != null)
				{
					ArchivesManager.player1AccountData.onlineData.deaths ++;
					ArchivesManager.player1AccountData.onlineData.killDeathRatio = ArchivesManager.player1AccountData.onlineData.kills / ArchivesManager.player1AccountData.onlineData.deaths;
					ArchivesManager.Instance.UpdateAccountData (ArchivesManager.player1AccountData);
				}
				GameManager.Instance.ReloadActiveScene ();
			}
		}

		public virtual void SendChangeScoreMessage (uint amount)
		{
			NetworkManager.connection.Send("Change Score", amount);
		}

		public virtual void ChangeScore (uint amount)
		{
			score += amount;
			if (OnlineBattle.localPlayer == this)
			{
				scoreText.text = "Score: " + score;
				if (ArchivesManager.player1AccountData != null)
				{
					ArchivesManager.player1AccountData.onlineData.kills ++;
					ArchivesManager.player1AccountData.onlineData.killDeathRatio = ArchivesManager.player1AccountData.onlineData.kills / ArchivesManager.player1AccountData.onlineData.deaths;
					if (score > ArchivesManager.player1AccountData.onlineData.highscore)
						ArchivesManager.player1AccountData.onlineData.highscore = score;
					ArchivesManager.Instance.UpdateAccountData (ArchivesManager.player1AccountData);
				}
			}
			else
				scoreText.text = "Bounty: " + Mathf.Clamp(score * OnlineBattle.Instance.bountyMultiplier, 1, int.MaxValue);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			foreach (MonoBehaviour monoBehaviour in disableMonoBehavioursIfIAmToo)
				monoBehaviour.enabled = false;
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			body.onDeath -= OnDeath;
			if (OnlineBattle.localPlayer == this)
				GameManager.updatables = GameManager.updatables.Remove(this);
			else
				OnlineBattle.nonLocalPlayers = OnlineBattle.nonLocalPlayers.Remove(this);
		}
	}
}