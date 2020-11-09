using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	public class Hunt : SinglePlayerGameMode
	{
		public Body heartEnemy;
		public float mapRadius;
		public float timePerDistUnit;
		public Timer deathTimer;
		public float extraTime;
		public float multiplyTimeMultiplier;
		public int multiplyToScoreIncrease;
		public int addToScoreIncrease;
		public Text highscoreText;
		public Transform timerVisualizer;
		public float minSpawnDistance = 10;

		public override void Awake ()
		{
			base.Awake ();
			// heartEnemy.onDeath += OnHeartKilled;
			if (Player.CanSwitchPositions)
				deathTimer.duration = Mathf.Min(Vector2.Distance(HumanPlayer.Instance.body.trs.position, heartEnemy.trs.position), Vector2.Distance(HumanPlayer.Instance.weapon.trs.position, heartEnemy.trs.position)) * timePerDistUnit + extraTime;
			else
				deathTimer.duration = Vector2.Distance(HumanPlayer.Instance.weapon.trs.position, heartEnemy.trs.position) * timePerDistUnit + extraTime;
			deathTimer.Reset ();
			deathTimer.onFinished += GameOver;
			deathTimer.Start ();
			highscoreText.text = "" + Highscore;
			Player.players = FindObjectsOfType<Player>();
			heartEnemy.trs.position = Random.insideUnitCircle * (mapRadius - heartEnemy.radius);
			heartEnemy.trs.eulerAngles = Vector3.forward * Random.value * 360;
			HumanPlayer.Instance.weapon.onCollide += OnWeaponCollide;
		}

		public override void DoUpdate ()
		{
			timerVisualizer.localScale = timerVisualizer.localScale.SetX(deathTimer.timeRemaining / deathTimer.duration);
		}

		public virtual void OnWeaponCollide (Collision2D coll, Weapon weapon)
		{
			if (coll.collider == heartEnemy.collider)
				OnHeartKilled (HumanPlayer.Instance, heartEnemy);
		}

		public virtual void OnHeartKilled (Player killer, Body victim)
		{
			deathTimer.Stop ();
			AddScore ((float) multiplyToScoreIncrease * Mathf.Clamp01((deathTimer.timeRemaining - (deathTimer.duration - extraTime)) / extraTime) + addToScoreIncrease);
			bool willSpawnTooClose;
			do
			{
				heartEnemy.trs.position = Random.insideUnitCircle * (mapRadius - heartEnemy.radius);
				if (Player.CanSwitchPositions)
					willSpawnTooClose = Mathf.Min(Vector2.Distance(killer.body.trs.position, heartEnemy.trs.position), Vector2.Distance(killer.weapon.trs.position, heartEnemy.trs.position)) < minSpawnDistance;
				else
					willSpawnTooClose = Vector2.Distance(killer.weapon.trs.position, heartEnemy.trs.position) < minSpawnDistance;
			}
			while (willSpawnTooClose);
			heartEnemy.trs.eulerAngles = Vector3.forward * Random.value * 360;
			extraTime *= multiplyTimeMultiplier;
			if (Player.CanSwitchPositions)
				deathTimer.duration = Mathf.Min(Vector2.Distance(killer.body.trs.position, heartEnemy.trs.position), Vector2.Distance(killer.weapon.trs.position, heartEnemy.trs.position)) * timePerDistUnit + extraTime;
			else
				deathTimer.duration = Vector2.Distance(killer.weapon.trs.position, heartEnemy.trs.position) * timePerDistUnit + extraTime;
			deathTimer.Reset ();
			deathTimer.Start ();
		}

		public override void SetScore (float amount)
		{
			if ((int) score > Highscore)
				highscoreText.text = "" + (int) score;
			base.SetScore (amount);
		}

		public virtual void GameOver (params object[] args)
		{
			GameManager.Instance.ReloadActiveScene ();
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			deathTimer.onFinished -= GameOver;
			// heartEnemy.onDeath -= OnHeartKilled;
			HumanPlayer.Instance.weapon.onCollide -= OnWeaponCollide;
		}
	}
}