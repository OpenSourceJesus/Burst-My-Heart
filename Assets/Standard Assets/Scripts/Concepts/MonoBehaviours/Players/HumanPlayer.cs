using UnityEngine;
using System.Collections;
using Extensions;

namespace BMH
{
	public class HumanPlayer : Player, ISavableAndLoadable
	{
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
		public LayerMask whatIsEnemyGroup;
		[HideInInspector]
		public LayerMask whatWeaponCollidesWith;
		[SaveAndLoadValue]
		[HideInInspector]
		public string nameOfEnemyGroupImInside;
		public const float amountToPushPlayerOutsideEnemyGroupOnLoad = 1;
		Vector2 loadedBodyPositionOffset;
		[SaveAndLoadValue]
		public Vector2 BodyPosition
		{
			get
			{
				return body.trs.position;
			}
			set
			{
				loadedBodyPositionOffset = new Vector2();
				if (!string.IsNullOrEmpty(nameOfEnemyGroupImInside))
				{
					EnemyGroup insideEnemyGroup = GameObject.Find(nameOfEnemyGroupImInside).GetComponent<EnemyGroup>();
					while (Physics2D.OverlapCircle(value + loadedBodyPositionOffset, body.radius, whatIsEnemyGroup) != null)
						loadedBodyPositionOffset += ((Vector2) value - insideEnemyGroup.compositeCollider2D.bounds.ToRect().center).normalized * amountToPushPlayerOutsideEnemyGroupOnLoad;
				}
				body.trs.position = value + loadedBodyPositionOffset;
			}
		}
		[SaveAndLoadValue]
		public Vector2 WeaponPosition
		{
			get
			{
				return weapon.trs.position;
			}
			set
			{
				while (Physics2D.OverlapCircle(value, weapon.radius, Physics2D.GetLayerCollisionMask(weapon.gameObject.layer)) != null)
					value = (Vector2) body.trs.position + Random.insideUnitCircle.normalized * GameManager.GetSingleton<HumanPlayer>().maxBodyToWeaponDist;
				weapon.trs.position = value;
			}
		}

		public override void Awake ()
		{
			base.Awake ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (uniqueId == 0)
				{
					do
					{
						uniqueId = Random.Range(int.MinValue, int.MaxValue);
					} while (GameManager.UniqueIds.Contains(uniqueId));
					GameManager.UniqueIds = GameManager.UniqueIds.Add(uniqueId);
				}
				whatWeaponCollidesWith = Physics2D.GetLayerCollisionMask(weapon.gameObject.layer);
				return;
			}
#endif
		}

#if UNITY_EDITOR
		public virtual void Reset ()
		{
			Awake ();
		}
#endif

		public override void DoUpdate ()
		{
			if (GameManager.paused)
				return;
			body.DoUpdate ();
			weapon.DoUpdate ();
			if (InputManager.inputters[GameManager.GetSingleton<GameManager>().teams.IndexOf(owner)].GetButtonDown("Switch Positions") && CanSwitchPositions)
				SwitchPositions ();
			base.DoUpdate ();
		}
	}
}