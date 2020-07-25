using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[ExecuteInEditMode]
	public class Obelisk : MonoBehaviour, IUpdatable
	{
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public bool Found
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>(name + " found", false);
			}
			set
			{
				SaveAndLoadManager.SetValue(name + " found", value);
			}
		}
		public static Obelisk[] instances = new Obelisk[0];
		public WorldMapIcon worldMapIcon;
		public GameObject foundIndicator;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (worldMapIcon == null)
					worldMapIcon = GetComponent<WorldMapIcon>();
				return;
			}
			worldMapIcon = GetComponent<WorldMapIcon>();
#endif
			instances = instances.Add(this);
		}

		public virtual void DoUpdate ()
		{
			if ((InputManager.inputters[0].GetButtonDown("Interact") || InputManager.inputters[1].GetButtonDown("Interact")) && !WorldMap.isOpen)
				GameManager.GetSingleton<WorldMap>().Open ();
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
			instances = instances.Remove(this);
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			WorldMapIcon worldMapIcon = other.GetComponent<WorldMapIcon>();
			if (worldMapIcon != null && worldMapIcon.isActive)
				return;
			WorldMap.playerIsAtObelisk = true;
			GameManager.updatables = GameManager.updatables.Add(this);
			Found = true;
			GameManager.GetSingleton<SaveAndLoadManager>().Save ();
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (other.gameObject.layer == LayerMask.NameToLayer("Map"))
				return;
			GameManager.updatables = GameManager.updatables.Remove(this);
			WorldMap.playerIsAtObelisk = false;
		}
	}
}