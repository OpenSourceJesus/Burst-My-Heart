using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class Enemy_Follow : Enemy
	{
		[HideInInspector]
		public bool isDead;
		public EnemyGroup enemyGroup;
		public Transform visionVisualizerTrs;
		public SpriteRenderer visionVisualizer;
		public Collider2D visionCollider;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
			if (War.instance != null)
			{
				enabled = false;
				Destroy(visionCollider);
				body.collider.enabled = false;
				if (weapon != null)
					weapon.collider.enabled = false;
				if (visionVisualizer != null)
					visionVisualizer.enabled = false;
				awakenTimer.onFinished += Awaken;
				awakenTimer.Reset ();
				awakenTimer.Start ();
				trs.eulerAngles = Vector3.forward * Random.value * 360;
			}
			if (body != null)
				body.onDeath += OnDeath;
			isDead = false;
		}

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			if (visionVisualizerTrs != null && enemyGroup != null)
			{
				visionVisualizerTrs.SetParent(enemyGroup.trs);
				enemyGroup.visionVisualizers = enemyGroup.visionVisualizers.Add_class(visionVisualizer);
				Awaken ();
			}
		}

		public virtual void OnDeath (Player killer, Body victim)
		{
			isDead = true;
			if (enemyGroup != null)
			{
				enemyGroup.enemies = enemyGroup.enemies.Remove_class(this);
				if (enemyGroup.enemies.Length == 0)
					enemyGroup.OnDefeat ();
			}
			Destroy(gameObject);
		}

		public override void HandleMovement ()
		{
			Move (GameManager.GetSingleton<HumanPlayer>().body.trs.position - body.trs.position);
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			if (body != null)
				body.onDeath -= OnDeath;
			if (War.instance != null)
				awakenTimer.onFinished -= Awaken;
		}

		public override void Awaken (params object[] args)
		{
			OnAwaken ();
		}
	}
}