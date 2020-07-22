using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class DisableObject : MonoBehaviour
	{
		public virtual void OnDisable ()
		{
		}
		
		public virtual void Awake ()
		{
			if (!enabled)
				return;
			gameObject.SetActive(false);
		}
	}
}