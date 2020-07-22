using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

namespace BMH
{
	[Serializable]
	public class TemporaryTextObject : TemporaryDisplayObject
	{
		public Text text;
		public float durationPerCharacter;
		
		public override IEnumerator DisplayRoutine ()
		{
			duration = text.text.Length * durationPerCharacter;
			yield return base.DisplayRoutine ();
		}
	}
}