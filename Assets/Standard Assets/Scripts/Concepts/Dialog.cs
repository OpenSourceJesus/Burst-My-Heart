using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Extensions;

namespace BMH
{
	public class Dialog : MonoBehaviour, IUpdatable
	{
		public Text text;
		[Multiline]
		public string textString;
		float writeTimer;
		float writeDelayTime;
		// public const string COMMENT = "↕";
		public float writeSpeed;
		int currentChar;
		public WaitEvent[] waitEvents;
		public RequireInputDeviceEvent[] requireInputDeviceEvents;
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
		public Button activateOnEnd;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}

		public virtual void OnEnable ()
		{
			currentChar = 0;
		}
		
		public virtual void DoUpdate ()
		{
            writeTimer += GameManager.UnscaledDeltaTime;
            if (writeTimer > 1f / writeSpeed + writeDelayTime)
            {
                shouldDisplayCurrentChar = true;
                writeTimer = writeTimer - (1f / writeSpeed + writeDelayTime);
                writeDelayTime = 0;
                foreach (WaitEvent waitEvent in waitEvents)
                {
                    if (waitEvent.indicator == textString[currentChar])
                    {
                        writeDelayTime = waitEvent.duration;
                        shouldDisplayCurrentChar = false;
                        break;
                    }
                }
                if (writeDelayTime == 0)
                {
                    if (ClearEvent.indicator == textString[currentChar])
                    {
                        text.text = "";
                        shouldDisplayCurrentChar = false;
                    }
                    else
                    {
                        foreach (RequireInputDeviceEvent requireInputDeviceEvent in requireInputDeviceEvents)
                        {
                            if (requireInputDeviceEvent.startIndicator == textString[currentChar])
                            {
                                shouldDisplayCurrentChar = false;
                                if (InputManager.inputDevices.Contains(requireInputDeviceEvent.inputDevice))
                                    textString = textString.Remove(textString.IndexOf(RequireInputDeviceEvent.endIndicator), 1);
                                else
                                {
                                    textString = textString.Remove(currentChar, textString.IndexOf(RequireInputDeviceEvent.endIndicator) - currentChar + 1);
                                    currentChar --;
                                }
                                break;
                            }
                        }
                    }
                }
                if (shouldDisplayCurrentChar)
                    text.text += textString[currentChar];
                currentChar ++;
                if (currentChar == textString.Length)
                    GameManager.GetSingleton<DialogManager>().EndDialog (this);
            }
		}
		
		[Serializable]
		public class WaitEvent
		{
			public char indicator;
			public float duration;
		}
		
		[Serializable]
		public class ClearEvent
		{
			public const char indicator = '§';
		}
		
		[Serializable]
		public class RequireInputDeviceEvent
		{
			public char startIndicator;
			public const char endIndicator = '☺';
			public InputDevice inputDevice;
		}
	}
}