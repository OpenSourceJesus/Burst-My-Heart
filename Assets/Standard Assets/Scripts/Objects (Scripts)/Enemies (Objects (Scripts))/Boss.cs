using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.UI;

namespace BMH
{
	public class Boss : Enemy_Shoot
	{
		public string name;
		public new static Boss instance;
		public static Boss[] instances = new Boss[0];
		public int partIndex;
		public Boss nextPart;
		public Timer partTimer;
		public Transform partTimerVisualizerTrs;
		public Image partTimerVisualizer;
		public static bool initialized;

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			if (!initialized && partIndex == 0)
			{
				enabled = false;
				nextPart.trs.SetParent(null);
				instances = instances.Add(this);
				Boss part = nextPart;
				while (part != null)
				{
					instances = instances.Add(part);
					part.gameObject.SetActive(false);
					part = part.nextPart;
				}
				initialized = true;
				return;
			}
			if (nextPart != null)
				nextPart.trs.SetParent(null);
			partTimer.onFinished += OnEndOfPart;
			partTimer.Reset ();
			partTimer.Start ();
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			partTimer.onFinished -= OnEndOfPart;
			Weapon[] bullets = GetComponentsInChildren<Weapon>();
			foreach (Weapon bullet in bullets)
				ObjectPool.Instance.Despawn (bullet.prefabIndex, bullet.gameObject, bullet.trs);
		}

		public override void DoUpdate ()
		{
			base.DoUpdate ();
			partTimerVisualizerTrs.eulerAngles = Vector3.zero;
			partTimerVisualizer.fillAmount = partTimer.TimeElapsed / partTimer.duration;
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.N))
			{
				if (body.Hp != 0)
					OnEndOfPart ();
				else
					OnDeath (null, body);
			}
#endif
		}

		public virtual void OnEndOfPart (params object[] args)
		{
			partTimerVisualizer.enabled = false;
			body.spriteRenderer.color = Color.white;
			body.Hp = 0;
		}

		public override void OnDeath (Player killer, Body victim)
		{
			isDead = true;
			Destroy(gameObject);
			if (nextPart != null)
				nextPart.gameObject.SetActive(true);
			else
				enemyGroup.OnDefeat ();
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			if (other.gameObject != HumanPlayer.Instance.body.gameObject)
				return;
			enabled = true;
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (other.gameObject != HumanPlayer.Instance.body.gameObject)
				return;
			Destroy(gameObject);
			if (isDead)
				return;
			foreach (Boss boss in instances)
			{
				if (boss != null)
					Destroy(boss.gameObject);
			}
			instances = new Boss[0];
			initialized = false;
			Instantiate(Resources.Load<Boss>(name), trs.position, trs.rotation, trs.parent);
		}

		public override void UpdateGraphics ()
		{
		}
	}
}