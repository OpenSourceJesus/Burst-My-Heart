using UnityEngine;
using System.Collections;
using System;
using Extensions;

namespace BMH
{
	[Serializable]
	public class TemporaryDisplayObject
	{
		public GameObject obj;
		public float duration;
		public bool realtime;
		public static TemporaryDisplayObject[] displayedInstances = new TemporaryDisplayObject[0];
		
		public virtual IEnumerator DisplayRoutine ()
		{
			Show ();
			if (realtime)
				yield return new WaitForSecondsRealtime(duration);
			else
				yield return new WaitForSeconds(duration);
			Hide ();
		}

		public virtual void Show ()
		{
			if (displayedInstances.Contains_class(this))
				return;
			if (obj != null)
				obj.SetActive(true);
			displayedInstances = displayedInstances.Add_class(this);
		}

		public virtual void Hide ()
		{
			if (obj != null)
				obj.SetActive(false);
			displayedInstances = displayedInstances.Remove_class(this);
		}
	}
}