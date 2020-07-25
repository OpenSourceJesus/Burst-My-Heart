using UnityEngine;
using System.Collections.Generic;
using Extensions;

namespace BMH
{
	public class NoteMaker : MonoBehaviour
	{
		[HideInInspector]
		public Vector2 positionOfPreviousNote;
		public Transform[] transforms;

		public virtual void OnEnable ()
		{
			MakeNotes.noteMakers = MakeNotes.noteMakers.Add(this);
		}

		public virtual void OnDisable ()
		{
			MakeNotes.noteMakers = MakeNotes.noteMakers.Remove(this);
		}
	}
}