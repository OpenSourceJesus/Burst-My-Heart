using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class CompositeEnemy : Enemy_Follow
	{
		public Enemy[] enemies;
		public LineRenderer[] lineRenderers = new LineRenderer[0];

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				enemies = trs.GetComponentsInChildren<Enemy>();
				if (lineRenderers.Length != enemies.Length)
				{
					foreach (LineRenderer lineRenderer in lineRenderers)
						DestroyImmediate(lineRenderer.gameObject);
					lineRenderers = new LineRenderer[0];
					foreach (Enemy enemy in enemies)
					{
						LineRenderer newLineRenderer = new GameObject().AddComponent<LineRenderer>();
						newLineRenderer.GetComponent<Transform>().SetParent(enemy.trs.parent);
						newLineRenderer.positionCount = 2;
						newLineRenderer.useWorldSpace = true;
						newLineRenderer.sharedMaterial = lineRenderer.sharedMaterial;
						newLineRenderer.colorGradient = lineRenderer.colorGradient;
						newLineRenderer.widthCurve = lineRenderer.widthCurve;
						newLineRenderer.widthMultiplier = lineRenderer.widthMultiplier;
						newLineRenderer.sortingOrder = lineRenderer.sortingOrder;
						newLineRenderer.sortingLayerName = lineRenderer.sortingLayerName;
						lineRenderers = lineRenderers.Add(newLineRenderer);
					}
				}
				foreach (Enemy enemy in enemies)
					enemy.enabled = false;
				UpdateGraphics ();
				return;
			}
#endif
			base.Awake ();
			UpdateGraphics ();
			enabled = false;
		}

#if UNITY_EDITOR
		public virtual void LateUpdate ()
		{
			UpdateGraphics ();
		}
#endif

		public virtual void OnTriggerEnter2D (Collider2D collider)
		{
			if (collider.gameObject != HumanPlayer.Instance.body.gameObject)
				return;
			foreach (Enemy enemy in enemies)
				enemy.enabled = true;
		}

		public virtual void OnTriggerExit2D (Collider2D collider)
		{
			if (collider.gameObject != HumanPlayer.Instance.body.gameObject)
				return;
			foreach (Enemy enemy in enemies)
			{
				enemy.enabled = false;
				enemy.trs.position = enemy.initPosition;
				enemy.trs.eulerAngles = Vector3.forward * enemy.initRotation;
			}
			UpdateGraphics ();
		}

		public override void DoUpdate ()
		{
			UpdateGraphics ();
		}

		public override void UpdateGraphics ()
		{
			Enemy enemy;
			for (int i = 0; i < enemies.Length; i ++)
			{
				enemy = enemies[i];
				enemy.UpdateGraphics ();
				lineRenderers[i].SetPosition(0, enemy.trs.position);
				lineRenderers[i].SetPosition(1, trs.position);
			}
		}
	}
}