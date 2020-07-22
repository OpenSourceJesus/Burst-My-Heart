using UnityEngine;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	public class OnlinePlayer : HumanPlayer
	{
		public new static OnlinePlayer instance;
		public uint playerId;
		public SyncTransform[] syncTransforms;
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

		public virtual void Init ()
		{
			for (int i = 0; i < syncTransforms.Length; i ++)
			{
				SyncTransform syncTransform = syncTransforms[i];
				syncTransform.id = (uint) (playerId * syncTransforms.Length + i);
				syncTransform.Init ();
				if (instance == this)
					GameManager.updatables = GameManager.updatables.Add(syncTransform);
			}
		}

		public virtual void OnDeath (Player killer, Body victim)
		{
			if (instance != this)
				return;
			OnlinePlayer onlineKiller = killer as OnlinePlayer;
			uint changeScoreAmount = (uint) Mathf.Clamp(score * GameManager.GetSingleton<OnlineBattle>().bountyMultiplier, 1, int.MaxValue);
			GameManager.GetSingleton<NetworkManager>().SendChangeScoreEvent (onlineKiller.playerId, changeScoreAmount);
			onlineKiller.ChangeScore (changeScoreAmount);
			if (ArchivesManager.player1AccountData != null)
			{
				ArchivesManager.player1AccountData.onlineData.deaths ++;
				ArchivesManager.player1AccountData.onlineData.killDeathRatio = ArchivesManager.player1AccountData.onlineData.kills / ArchivesManager.player1AccountData.onlineData.deaths;
				GameManager.GetSingleton<ArchivesManager>().UpdateAccountData (ArchivesManager.player1AccountData);
			}
			GameManager.GetSingleton<GameManager>().ReloadActiveScene ();
		}

		public virtual void ChangeScore (uint amount)
		{
			Score += amount;
			if (instance == this)
			{
				scoreText.text = "Score: " + score;
				if (ArchivesManager.player1AccountData != null)
				{
					ArchivesManager.player1AccountData.onlineData.kills ++;
					ArchivesManager.player1AccountData.onlineData.killDeathRatio = ArchivesManager.player1AccountData.onlineData.kills / ArchivesManager.player1AccountData.onlineData.deaths;
					if (score > ArchivesManager.player1AccountData.onlineData.highscore)
						ArchivesManager.player1AccountData.onlineData.highscore = score;
					GameManager.GetSingleton<ArchivesManager>().UpdateAccountData (ArchivesManager.player1AccountData);
				}
			}
			else
				scoreText.text = "Bounty: " + Mathf.Clamp(score * GameManager.GetSingleton<OnlineBattle>().bountyMultiplier, 1, int.MaxValue);
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
			foreach (SyncTransform syncTransform in syncTransforms)
				GameManager.updatables = GameManager.updatables.Remove(syncTransform);
			OnlineBattle.nonLocalPlayers = OnlineBattle.nonLocalPlayers.Remove(this);
			if (instance == this)
				GameManager.GetSingleton<NetworkManager>().Disconnect();
		}
	}
}