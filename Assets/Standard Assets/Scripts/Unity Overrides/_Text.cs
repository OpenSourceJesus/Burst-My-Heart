using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using System;

namespace BMH
{
	//[ExecuteAlways]
	[RequireComponent(typeof(Text))]
	[DisallowMultipleComponent]
	public class _Text : MonoBehaviour
	{
		[HideInInspector]
		public string keyboardText;
		public bool useSeperateTextForGamepad;
		[Multiline]
		public string gamepadText;
		public InputDeviceEntry[] inputDeviceEntries = new InputDeviceEntry[0];
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
				if (inputDeviceEntries.Length == 0)
				{
					inputDeviceEntries = new InputDeviceEntry[3];
					inputDeviceEntries[0].device = InputDevice.Keyboard;
					inputDeviceEntries[0].text = text.text;
					inputDeviceEntries[1].device = InputDevice.Gamepad;
					if (useSeperateTextForGamepad)
						inputDeviceEntries[1].text = gamepadText;
					// else
						// inputDeviceEntries[1].text = ;
					inputDeviceEntries[2].device = InputDevice.Touchscreen;
					inputDeviceEntries[2].text = text.text;
				}
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
			instances = instances.Add(this);
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
			instances = instances.Remove(this);
		}

		[Serializable]
		public class InputDeviceEntry
		{
			public InputDevice device;
			public bool useSeperateText;
			[Multiline]
			public string text;
		}
	}
}
