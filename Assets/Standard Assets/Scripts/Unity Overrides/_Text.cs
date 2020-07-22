using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	[RequireComponent(typeof(Text))]
	[DisallowMultipleComponent]
	public class _Text : MonoBehaviour
	{
		[HideInInspector]
		public string keyboardText;
		public bool useSeperateTextForGamepad;
		[Multiline]
		public string gamepadText;
		public Text text;
		public static _Text[] instances = new _Text[0];

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (text == null)
					text = GetComponent<Text>();
				keyboardText = text.text;
				return;
			}
#endif
		}

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			UpdateText ();
			instances = instances.Add_class(this);
		}
		
		public virtual void UpdateText ()
		{
			if (useSeperateTextForGamepad)
			{
				if (InputManager.UsingGamepad)
					text.text = gamepadText;
				else
					text.text = keyboardText;
			}
			else
				text.text = keyboardText;
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			instances = instances.Remove_class(this);
		}
	}
}
