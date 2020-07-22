using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class Enemy_Minelayer : Enemy_Follow
	{
		public Timer layMineTimer;
		public Mine minePrefab;
		public Transform spawnPoint;
		Mine mine;
		Transform mineParent;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
			layMineTimer.onFinished += LayMine;
		}

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			if (enemyGroup != null)
				mineParent = enemyGroup.trs;
			layMineTimer.Start ();
			rigid.velocity = Random.insideUnitCircle.normalized * moveSpeed;
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			layMineTimer.Stop ();
			layMineTimer.timeRemaining = 0;
		}

		public override void HandleMovement ()
		{
			rigid.velocity = rigid.velocity.normalized * moveSpeed;
		}

		public virtual void LayMine (params object[] args)
		{
			StartCoroutine(LayMineRoutine ());
		}

		public virtual IEnumerator LayMineRoutine ()
		{
			mine = GameManager.GetSingleton<ObjectPool>().SpawnComponent<Mine>(minePrefab.prefabIndex, spawnPoint.position, spawnPoint.rotation, mineParent);
			mine.player = this;
			yield return new WaitUntil(() => (Vector2.Distance(trs.position, mine.trs.position) > radius + mine.radius));
			mine.Activate ();
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			layMineTimer.onFinished -= LayMine;
			if (mine != null && mine.gameObject.activeSelf && !mine.collider.enabled)
				mine.Activate ();
			StopAllCoroutines();
		}

		public override void UpdateGraphics ()
		{
		}
	}
}