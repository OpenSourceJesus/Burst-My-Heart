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
		public new Collider collider;
		public MeshRenderer meshRenderer;
		public Timer awakenTimer;
		public Rigidbody rigid;
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
					rigid = GetComponent<Rigidbody>();
				if (meshRenderer == null)
					meshRenderer = GetComponent<MeshRenderer>();
				return;
			}
#endif
			maxBodyToWeaponDistance *= trs.localScale.x;
			if (Survival.Instance != null)
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
				rigid.isKinematic = false;
			if (body != null)
			{
				body.Hp = body.maxHp;
				body.rigid.isKinematic = false;
			}
			if (weapon != null)
				weapon.rigid.isKinematic = false;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (rigid != null)
				rigid.isKinematic = true;
			if (body != null)
				body.rigid.isKinematic = true;
			if (weapon != null)
				weapon.rigid.isKinematic = true;
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void OnTransformParentChanged ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (trs.parent == ObjectPool.Instance.trs)
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
			GameManager.Instance.StartCoroutine (AwakenRoutine ());
		}

		public virtual IEnumerator AwakenRoutine ()
		{
			yield return new WaitUntil(() => (Physics2D.OverlapCircle(trs.position, radius, Survival.Instance.whatEnemiesCantSpawnIn) == null));
			OnAwaken ();
		}

		public virtual void OnAwaken ()
		{
			if (body != null)
			{
				body.meshRenderer.material.color = body.meshRenderer.material.color.MultiplyAlpha(4);
				body.collider.enabled = true;
			}
			if (weapon != null)
			{
				weapon.meshRenderer.material.color = weapon.meshRenderer.material.color.MultiplyAlpha(4);
				weapon.collider.enabled = true;
			}
			if (meshRenderer != null)
				meshRenderer.material.color = meshRenderer.material.color.SetAlpha(1);
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
			if (Survival.Instance != null)
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