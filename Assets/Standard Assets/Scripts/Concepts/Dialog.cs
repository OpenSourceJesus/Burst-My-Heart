using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Extensions;
using TMPro;
using BMH;
using UnityEngine.Events;

namespace DialogAndStory
{
	public class Dialog : MonoBehaviour, IUpdatable
	{
		public Canvas canvas;
		public TMP_Text text;
		[Multiline(7)]
		public string textString;
		string textStringCopy;
		float writeTimer;
		float writeDelayTime;
		public float writeSpeed;
		int currentChar;
		public WaitEvent[] waitEvents;
		public RequireInputDeviceEvent[] requireInputDeviceEvents;
		public CustomDialogEvent[] customDialogEvents;
		bool shouldDisplayCurrentChar;
		[HideInInspector]
		public Conversation conversation;
		public bool IsActive
		{
			get
			{
				return gameObject.activeSelf;
			}
		}
		[HideInInspector]
		public bool isFinished;
		public CustomEvent onStartedEvent;
		public CustomEvent onFinishedEvent;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public CustomEvent onLeftWhileTalkingEvent;
		public bool autoEnd;
		public bool runWhilePaused;

		public virtual void OnEnable ()
		{
			currentChar = 0;
			text.text = "";
			isFinished = false;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
		
		public virtual void DoUpdate ()
		{
			textStringCopy = textString;
			if (runWhilePaused)
				writeTimer += Time.unscaledDeltaTime;
			else
				writeTimer += GameManager.UnscaledDeltaTime;
			if (writeTimer > 1f / writeSpeed + writeDelayTime)
			{
				shouldDisplayCurrentChar = true;
				writeTimer -= (1f / writeSpeed + writeDelayTime);
				writeDelayTime = 0;
				foreach (WaitEvent waitEvent in waitEvents)
				{
					if (textStringCopy.IndexOf(waitEvent.indicator, currentChar) == currentChar)
					{
						writeDelayTime = waitEvent.duration;
						currentChar += waitEvent.indicator.Length;
						shouldDisplayCurrentChar = false;
						break;
					}
				}
				if (writeDelayTime == 0)
				{
					if (textStringCopy.IndexOf(ClearEvent.indicator, currentChar) == currentChar)
					{
						text.text = "";
						currentChar += ClearEvent.indicator.Length;
						shouldDisplayCurrentChar = false;
					}
					else
					{
						if (textStringCopy.IndexOf(RequireInputDeviceEvent.endIndicator, currentChar) == currentChar)
						{
							currentChar += RequireInputDeviceEvent.endIndicator.Length;
							shouldDisplayCurrentChar = false;
						}
						else
						{
							foreach (RequireInputDeviceEvent requireInputDeviceEvent in requireInputDeviceEvents)
							{
								if (textStringCopy.IndexOf(requireInputDeviceEvent.startIndicator, currentChar) == currentChar)
								{
									shouldDisplayCurrentChar = false;
									if (InputManager.inputDevices.Contains(requireInputDeviceEvent.inputDevice) == requireInputDeviceEvent.mustHaveInputDevice)
									{
										textStringCopy = textStringCopy.Remove(textStringCopy.IndexOf(RequireInputDeviceEvent.endIndicator, currentChar), RequireInputDeviceEvent.endIndicator.Length);
										currentChar += requireInputDeviceEvent.startIndicator.Length;
									}
									else
									{
										int indexOfEventEnd = textStringCopy.IndexOf(RequireInputDeviceEvent.endIndicator, currentChar) + RequireInputDeviceEvent.endIndicator.Length;
										textStringCopy = textStringCopy.RemoveStartEnd(currentChar, indexOfEventEnd);
										currentChar = indexOfEventEnd;
									}
								}
							}
						}
						foreach (CustomDialogEvent customDialogEvent in customDialogEvents)
						{
							if (textStringCopy.IndexOf(customDialogEvent.indicator, currentChar) == currentChar)
							{
								shouldDisplayCurrentChar = false;
								currentChar += customDialogEvent.indicator.Length;
							}
						}
					}
				}
				if (shouldDisplayCurrentChar)
				{
					if (currentChar < textStringCopy.Length)
					{
						text.text += textStringCopy[currentChar];
						currentChar ++;
					}
					else
					{
						isFinished = true;
						onFinishedEvent.Do ();
						if (autoEnd)
							DialogManager.Instance.EndDialog (this);
					}
				}
			}
		}

		[Serializable]		
		public class Event
		{
		}

		[Serializable]
		public class WaitEvent : Event
		{
			public string indicator;
			public float duration;
		}
		
		[Serializable]
		public class ClearEvent : Event
		{
			public const string indicator = "{clear}";
		}
		
		[Serializable]
		public class RequireInputDeviceEvent : Event
		{
			public string startIndicator;
			public const string endIndicator = "{end}";
			public InputDevice inputDevice;
			public bool mustHaveInputDevice;
		}

		[Serializable]
		public class CustomDialogEvent : Event
		{
			public string indicator;
			public CustomEvent customEvent;
		}
	}
}