using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;

namespace BMH
{
	public class Body : Rigidbody2DController, IDestructable
	{
		public uint maxHp;
		public uint MaxHp
		{
			get
			{
				return maxHp;
			}
			set
			{
				maxHp = value;
			}
		}
		float hp;
		public float Hp
		{
			get
			{
				return hp;
			}
			set
			{
				hp = value;
			}
		}
		public delegate void OnDeath(Player killer, Body victim);
		public event OnDeath onDeath;
		[HideInInspector]
		public bool isDead;
		
		public override void Awake ()
		{
			base.Awake ();
			hp = maxHp;
		}

		public virtual void TakeDamage (float amount)
		{
			hp -= amount;
			if (hp <= 0)
				Death ();
		}

		public virtual void Death ()
		{
			Death (null);
		}

		public virtual void Death (Player killer)
		{
			if (isDead)
				return;
			isDead = true;
			if (onDeath != null)
				onDeath (killer, this);
			if (GameManager.GetSingleton<Hunt>() == null && GameManager.GetSingleton<OnlineArena>() == null && (GameManager.GetSingleton<RPG>() == null || player.owner == GameManager.GetSingleton<GameManager>().teams[0]) && !(GameManager.GetSingleton<War>() != null && player.owner != GameManager.GetSingleton<GameManager>().teams[0]))
			{
				Destroy(player.gameObject);
				if (controllerToSwitchTo == null)
					GameManager.GetSingleton<GameManager>().ReloadActiveScene ();
				else
				{
					if (canControl)
						SwitchControl ();
					if (player.weapon.canControl)
						player.weapon.SwitchControl ();
				}
			}
		}
	}
}
