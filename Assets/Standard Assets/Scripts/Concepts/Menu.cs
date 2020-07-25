using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Extensions;
using System;
using Random = UnityEngine.Random;

namespace BMH
{
	//[ExecuteAlways]
	public class Menu : MonoBehaviour
	{
		[HideInInspector]
		public int currentSelection;
		public string description;
		public Transform trs;
		public MenuOption[] options = new MenuOption[0];
		public bool hasOptions;

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (hasOptions)
				{
					options = new MenuOption[trs.childCount];
					for (int i = 0; i < options.Length; i ++)
						options[i] = trs.GetChild(i).GetChild(0).GetChild(0).GetComponentInChildren<MenuOption>();
				}
				return;
			}
#endif
			// currentSelection = 0;
			if (hasOptions)
			{
				foreach (MenuOption option in options)
				{
					if (!option.enabled)
						option.OnDisable ();
					option.gameObject.SetActive(false);
				}
				options[currentSelection].gameObject.SetActive(true);
			}
		}

		public virtual void ChangeSelectionVertical (int direction)
		{
			options[currentSelection].gameObject.SetActive(false);
			currentSelection -= direction;
			if (currentSelection == -1)
				currentSelection = trs.childCount - 1;
			else if (currentSelection == trs.childCount)
				currentSelection = 0;
			options[currentSelection].gameObject.SetActive(true);
		}

		public virtual void ChangeSelectionHorizonal (int direction)
		{
			if (direction == -1 && trs.parent.parent.parent == null)
				return;
			if (direction == 1)
			{
				if (!options[currentSelection].enabled)
					return;
				if (options[currentSelection].connectionMeshRenderer.enabled)
					GameManager.GetSingleton<Menus>().SetCurrentMenu (options[currentSelection].trs.GetChild(0).GetComponentInChildren<Menu>());
				options[currentSelection].PickOption ();
			}
			else if (direction == -1)
			   GameManager.GetSingleton<Menus>().SetCurrentMenu (trs.parent.parent.parent.parent.parent.GetComponentInParent<Menu>());
		}

		public virtual void SetCurrentSelection (int selection)
		{
			options[currentSelection].gameObject.SetActive(false);
			currentSelection = selection;
			options[currentSelection].gameObject.SetActive(true);
		}
	}
}