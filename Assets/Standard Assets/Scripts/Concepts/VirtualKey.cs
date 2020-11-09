using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Extensions;

namespace BMH
{
	//[ExecuteAlways]
	public class VirtualKey : MonoBehaviour, IUpdatable
	{
		public Text text;
		public Outline outline;
		public Image image;
		bool isSelected;
		[HideInInspector]
		public Color notSelectedColor;
		public Color selectedColor;
		[HideInInspector]
		public Color notPressedColor;
		public Color pressedColor;
		public bool isActivatable;
		[HideInInspector]
		public bool isActivated;
		public Color activeColor;
		public UnityEvent invokeOnPressed;
		public UnityEvent invokeOnReleased;
		[HideInInspector]
		public string normalText;
		public bool alternateTextSameAsNormal;
		public string alternateText;
		[HideInInspector]
		public bool isPressed;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				notSelectedColor = outline.effectColor;
				notPressedColor = image.color;
				normalText = text.text;
				if (alternateTextSameAsNormal)
					alternateText = normalText;
				return;
			}
#endif
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void DoUpdate ()
		{
			if (isSelected)
			{
				if (InputManager.inputters[0].GetButtonDown(VirtualKeyboard.Instance.submitButtonName) || InputManager.inputters[1].GetButtonDown(VirtualKeyboard.Instance.submitButtonName))
					OnPressed ();
				else if (isPressed && (InputManager.inputters[0].GetButtonUp(VirtualKeyboard.Instance.submitButtonName) || InputManager.inputters[1].GetButtonUp(VirtualKeyboard.Instance.submitButtonName)))
					OnReleased ();
			}
			else if (isPressed)
				OnReleased ();
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void OnSelected ()
		{
			isSelected = true;
			outline.effectColor = selectedColor;
		}

		public virtual void OnDeselected ()
		{
			isSelected = false;
			outline.effectColor = notSelectedColor;
		}

		public virtual void OnPressed ()
		{
			if (isPressed && isSelected)
				return;
			isPressed = true;
			image.color = pressedColor;
			if (invokeOnPressed != null)
				invokeOnPressed.Invoke(); 
		}

		public virtual void OnReleased ()
		{
			if (!isPressed)
				return;
			isPressed = false;
			image.color = notPressedColor;
			if (isActivatable)
				ToggleActivate ();
			if (invokeOnReleased != null)
				invokeOnReleased.Invoke();
		}

		public virtual void ToggleActivate ()
		{
			isActivated = !isActivated;
			if (isActivated)
				image.color = activeColor;
			else
				image.color = notPressedColor;
		}

		public virtual void StartContinuousOutputToInputField ()
		{
			StartCoroutine(ContinuousOutputToInputFieldRoutine ());
		}

		public virtual IEnumerator ContinuousOutputToInputFieldRoutine ()
		{
			OutputToInputField ();
			yield return new WaitForSecondsRealtime(VirtualKeyboard.Instance.repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(VirtualKeyboard.Instance.repeatRate);
				OutputToInputField ();
			}
		}

		public virtual void OutputToInputField ()
		{
			if (VirtualKeyboard.Instance.outputToInputField.text.Length == VirtualKeyboard.Instance.outputToInputField.characterLimit)
				return;
			VirtualKeyboard.Instance.outputToInputField.text = VirtualKeyboard.Instance.outputToInputField.text.Insert(VirtualKeyboard.Instance.OutputPosition, text.text);
			VirtualKeyboard.Instance.OutputPosition ++;
			if (VirtualKeyboard.Instance.shiftKeys[0].isActivated)
			{
				VirtualKeyboard.Instance.shiftKeys[0].ToggleActivate ();
				VirtualKeyboard.Instance.OnShiftReleased (VirtualKeyboard.Instance.shiftKeys[0]);
			}
		}

		public virtual void EndContinuousOutputToInputField ()
		{
			StopAllCoroutines();
		}
	}
}