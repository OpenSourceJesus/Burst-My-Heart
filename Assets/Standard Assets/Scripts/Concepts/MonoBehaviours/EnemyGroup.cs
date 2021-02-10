using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	//[ExecuteAlways]
	// [ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class EnemyGroup : MonoBehaviour, ISavableAndLoadable
	{
		public Transform trs;
		public Enemy_Follow[] enemies;
		[HideInInspector]
		public Transform duplicate;
		public CompositeCollider2D compositeCollider2D;
		[HideInInspector]
		public int difficulty;
		[HideInInspector]
		[SaveAndLoadValue]
		public bool defeated;
		public Color defeatedColor;
		[HideInInspector]
		public SpriteRenderer[] visionVisualizers = new SpriteRenderer[0];
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}

		public virtual void Awake ()
		{
			// enemies = GetComponentsInChildren<Enemy_Follow>();
			// EnemyGroup enemyGroup;
			// for (int i = 0; i < enemies.Length; i ++)
			// {
			// 	enemyGroup = enemies[i].GetComponent<EnemyGroup>();
			// 	if (enemyGroup != null && enemyGroup != this)
			// 	{
			// 		enemies = enemies.RemoveAt(i);
			// 		i --;
			// 	}
			// }
			// if (!Application.isPlaying)
			// {
				enemies = GetComponentsInChildren<Enemy_Follow>();
				EnemyGroup enemyGroup;
				for (int i = 0; i < enemies.Length; i ++)
				{
					enemyGroup = enemies[i].GetComponent<EnemyGroup>();
					if (enemyGroup != null && enemyGroup != this)
					{
						enemies = enemies.RemoveAt(i);
						i --;
					}
				}
#if UNITY_EDITOR
				if (uniqueId == 0)
				{
					do
					{
						uniqueId = Random.Range(int.MinValue, int.MaxValue);
					}
					while (GameManager.UniqueIds.Contains(uniqueId));
					GameManager.UniqueIds = GameManager.UniqueIds.Add(uniqueId);
				}
#endif
				difficulty = enemies.Length;
				foreach (Enemy_Follow enemy in enemies)
				{
					enemy.enemyGroup = this;
					enemy.enabled = false;
					enemy.UpdateGraphics ();
				}
				return;
			// }
		}

#if UNITY_EDITOR
		public virtual void Reset ()
		{
			Awake ();
		}
#endif

		public virtual void OnDisable ()
		{
		}

		public virtual void OnDefeat ()
		{
			if (defeated)
				return;
			defeated = true;
			RPG.Instance.AddScore (difficulty);
			foreach (Enemy_Follow enemy in enemies)
				enemy.visionVisualizer.color = defeatedColor;
			HumanPlayer.Instance.nameOfEnemyGroupImInside = null;
			SaveAndLoadManager.Instance.Save ();
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			if (other.gameObject != HumanPlayer.Instance.body.gameObject || !enabled)
				return;
			duplicate = Instantiate(trs, trs.parent);
			duplicate.gameObject.SetActive(false);
			compositeCollider2D.generationType = CompositeCollider2D.GenerationType.Manual;
			foreach (Enemy_Follow enemy in enemies)
				enemy.enabled = true;
			HumanPlayer.Instance.nameOfEnemyGroupImInside = name.Replace("(Clone)", "");
			// if (SaveAndLoadManager.Instance != null)
				SaveAndLoadManager.Instance.Save ();
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (other != HumanPlayer.Instance.body.collider || !enabled)
				return;
			Destroy(trs.gameObject);
			duplicate.gameObject.SetActive(true);
		}
	}
}