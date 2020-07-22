using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class BulletPattern : ScriptableObject, IConfigurable
	{
		public virtual string Name
		{
			get
			{
				return name;
			}
		}
		public virtual string Category
		{
			get
			{
				return "Bullet Patterns";
			}
		}

		public virtual void Init (Transform spawner)
		{			
		}

		public virtual Vector2 GetShootDirection (Transform spawner)
		{
			return spawner.up;
		}
		
		public virtual Weapon[] Shoot (Transform spawner, Weapon bulletPrefab, float positionOffset = 0)
		{
			spawner.up = GetShootDirection(spawner);
			return new Weapon[] {  GameManager.GetSingleton<ObjectPool>().SpawnComponent<Weapon>(bulletPrefab.prefabIndex, spawner.position + spawner.up * positionOffset, Quaternion.LookRotation(Vector3.forward, spawner.up)) };
			// return new Weapon[] { GameManager.GetSingleton<ObjectPool>().SpawnComponent<Weapon>(bulletPrefab.prefabIndex, spawner.position, Quaternion.LookRotation(Vector3.forward, GetShootDirection(spawner))) };
		}
		
		public virtual Weapon[] Shoot (Vector2 spawnPos, Vector2 direction, Weapon bulletPrefab, float positionOffset = 0)
		{
			return new Weapon[] { GameManager.GetSingleton<ObjectPool>().SpawnComponent<Weapon>(bulletPrefab.prefabIndex, spawnPos + direction.normalized * positionOffset, Quaternion.LookRotation(Vector3.forward, direction)) };
		}
		
		public virtual IEnumerator RetargetAfterDelay (Weapon bullet, float delay)
		{
			yield return new WaitForSeconds(delay);
			if (bullet == null || !bullet.gameObject.activeSelf)
				yield break;
			yield return Retarget (bullet);
		}
		
		public virtual IEnumerator RetargetAfterDelay (Weapon bullet, Vector2 direction, float delay)
		{
			yield return new WaitForSeconds(delay);
			if (bullet == null || !bullet.gameObject.activeSelf)
				yield break;
			yield return Retarget (bullet, direction);
		}

		public virtual Weapon Retarget (Weapon bullet)
		{
			bullet.trs.up = GetRetargetDirection(bullet);
			bullet.Move (bullet.trs.up);
			return bullet;
		}
		
		public virtual Weapon Retarget (Weapon bullet, Vector2 direction)
		{
			bullet.trs.up = direction;
			bullet.Move (bullet.trs.up);
			return bullet;
		}
		
		public virtual Vector2 GetRetargetDirection (Weapon bullet)
		{
			return bullet.trs.up;
		}
		
		public virtual IEnumerator SplitAfterDelay (Weapon bullet, Weapon splitBulletPrefab, float delay, float positionOffset = 0)
		{
			yield return new WaitForSeconds(delay);
			if (!bullet.gameObject.activeSelf)
				yield break;
			yield return Split (bullet, splitBulletPrefab, positionOffset);
		}
		
		public virtual IEnumerator SplitAfterDelay (Weapon bullet, Vector2 direction, Weapon splitBulletPrefab, float delay, float positionOffset = 0)
		{
			yield return new WaitForSeconds(delay);
			if (!bullet.gameObject.activeSelf)
				yield break;
			yield return Split (bullet, direction, splitBulletPrefab, positionOffset);
		}

		public virtual Weapon[] Split (Weapon bullet, Weapon splitBulletPrefab, float positionOffset = 0)
		{
			return Shoot (bullet.trs.position, GetSplitDirection(bullet), splitBulletPrefab, positionOffset);
		}

		public virtual Weapon[] Split (Weapon bullet, Vector2 direction, Weapon splitBulletPrefab, float positionOffset = 0)
		{
			return Shoot (bullet.trs.position, direction, splitBulletPrefab, positionOffset);
		}
		
		public virtual Vector2 GetSplitDirection (Weapon bullet)
		{
			return bullet.trs.up;
		}
	}
}