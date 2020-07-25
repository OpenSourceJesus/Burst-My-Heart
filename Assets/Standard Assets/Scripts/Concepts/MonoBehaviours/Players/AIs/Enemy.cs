using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Extensions;

namespace BMH
{
	public class Enemy : AI, ISpawnable
	{
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public float moveSpeed;
		public new Collider2D collider;
		public SpriteRenderer spriteRenderer;
		public Timer awakenTimer;
		public Rigidbody2D rigid;
		public float radius;
		[HideInInspector]
		public Vector2 initPosition;
		[HideInInspector]
		public float initRotation;
		public float difficulty;
		public bool isTrap;
		public SpriteRenderer[] extraSpriteRenderers = new SpriteRenderer[0];

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (rigid == null)
					rigid = GetComponent<Rigidbody2D>();
				if (spriteRenderer == null)
					spriteRenderer = GetComponent<SpriteRenderer>();
				return;
			}
#endif
			maxBodyToWeaponDist *= trs.localScale.x;
			if (GameManager.GetSingleton<Survival>() != null)
			{
				enabled = false;
				collider.enabled = false;
				awakenTimer.onFinished += Awaken;
				awakenTimer.Reset ();
				awakenTimer.Start ();
				trs.eulerAngles = Vector3.forward * Random.value * 360;
			}
			else
			{
				initPosition = trs.position;
				initRotation = trs.eulerAngles.z;
			}
		}

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (rigid != null)
				rigid.simulated = true;
			if (body != null)
			{
				body.Hp = body.maxHp;
				body.rigid.simulated = true;
			}
			if (weapon != null)
				weapon.rigid.simulated = true;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (rigid != null)
				rigid.simulated = false;
			if (body != null)
				body.rigid.simulated = false;
			if (weapon != null)
				weapon.rigid.simulated = false;
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void OnTransformParentChanged ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (trs.parent == GameManager.GetSingleton<ObjectPool>().trs)
				OnDisable ();
		}

		public override void DoUpdate ()
		{
			HandleMovement ();
		}

		public virtual void HandleMovement ()
		{
		}

		public virtual void Move (Vector2 move)
		{
			move = Vector2.ClampMagnitude(move, 1);
			rigid.velocity = move * moveSpeed;
		}

		public virtual void Awaken (params object[] args)
		{
			GameManager.GetSingleton<GameManager>().StartCoroutine (AwakenRoutine ());
		}

		public virtual IEnumerator AwakenRoutine ()
		{
			yield return new WaitUntil(() => (Physics2D.OverlapCircle(trs.position, radius, GameManager.GetSingleton<Survival>().whatEnemiesCantSpawnIn) == null));
			OnAwaken ();
		}

		public virtual void OnAwaken ()
		{
			if (body != null)
			{
				body.spriteRenderer.color = body.spriteRenderer.color.MultiplyAlpha(4);
				body.collider.enabled = true;
			}
			if (weapon != null)
			{
				weapon.spriteRenderer.color = weapon.spriteRenderer.color.MultiplyAlpha(4);
				weapon.collider.enabled = true;
			}
			if (spriteRenderer != null)
				spriteRenderer.color = spriteRenderer.color.SetAlpha(1);
			if (collider != null)
				collider.enabled = true;
			foreach (SpriteRenderer extraSpriteRenderer in extraSpriteRenderers)
				extraSpriteRenderer.color = extraSpriteRenderer.color.MultiplyAlpha(4);
			enabled = true;
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			if (GameManager.GetSingleton<Survival>() != null)
				awakenTimer.onFinished -= Awaken;
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			Body collidedBody = coll.gameObject.GetComponent<Body>();
			if (collidedBody == null || collidedBody.Hp == MathfExtensions.NULL_FLOAT || collidedBody.player == null || collidedBody.player.owner == owner)
				return;
			collidedBody.Death (this);
		}
	}
}