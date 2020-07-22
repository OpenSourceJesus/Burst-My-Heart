using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class DestroyMenuOption : MonoBehaviour
	{
		public Transform trs;

		public virtual void OnDisable ()
		{
		}
		
		public virtual void Awake ()
		{
			if (!enabled)
				return;
			Transform menuChild = trs.parent.parent.parent;
			Menu menu = menuChild.GetComponentInParent<Menu>();
			menu.options = menu.options.RemoveAt_class(menuChild.GetSiblingIndex());
			Destroy(gameObject);
		}
	}
}
