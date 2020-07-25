using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace BMH
{
	//[ExecuteAlways]
	public class VirtualKeyboard : MonoBehaviour
	{
		public Transform trs;
		public InputField outputToInputField;
		int outputPosition;
		public int OutputPosition
		{
			get
			{
				return outputPosition;
			}
			set
			{
				outputPosition = value;
				outputToInputField.caretPosition = value;
			}
		}
		public VirtualKey[] keys = new VirtualKey[0];
		public VirtualKey[] shiftKeys = new VirtualKey[0];
		public VirtualKey capsLockKey;
		public float repeatDelay;
		public float repeatRate;
		public string submitButtonName;
		public CanvasGroup canvasGroup;
		bool initialized;

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				keys = GetComponentsInChildren<VirtualKey>();
				return;
			}
#endif
			GameManager.GetSingleton<Menus>().enabled = false;
			if (outputToInputField != null)
			{
				OutputPosition = 0;
				outputToInputField.text = "";
				if (!initialized)
				{
					outputToInputField.GetType().GetField("m_AllowInput", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(outputToInputField, true);
					outputToInputField.GetType().InvokeMember("SetCaretVisible", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, outputToInputField, null);
					Transform outputToInputFieldTrs = outputToInputField.GetComponent<Transform>();
					outputToInputFieldTrs.SetParent(outputToInputFieldTrs.root);
					initialized = true;
				}
				else
					outputToInputField.GetComponent<CanvasGroup>().alpha = 1;
			}
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.GetSingleton<Menus>().enabled = true;
			if (outputToInputField != null)
				outputToInputField.GetComponent<CanvasGroup>().alpha = 0;
		}

		public virtual void DisableInput ()
		{
			foreach (VirtualKey key in keys)
				key.enabled = false;
			canvasGroup.alpha = 0;
		}

		public virtual void EnableInput ()
		{
			foreach (VirtualKey key in keys)
				key.enabled = true;
			canvasGroup.alpha = 1;
		}

		public virtual void OnCapsLockReleased ()
		{
			if (shiftKeys[0].isActivated)
			{
				foreach (VirtualKey shiftKey in shiftKeys)
					shiftKey.ToggleActivate ();
				return;
			}
			foreach (VirtualKey key in keys)
			{
				if (key.text.text == key.normalText)
					key.text.text = key.alternateText;
				else
					key.text.text = key.normalText;
			}
		}

		public virtual void OnShiftReleased (VirtualKey releasedShiftKey)
		{
			foreach (VirtualKey shiftKey in shiftKeys)
			{
				if (shiftKey.isActivated != releasedShiftKey.isActivated)
					shiftKey.ToggleActivate ();
			}
			if (capsLockKey.isActivated)
			{
				capsLockKey.ToggleActivate ();
				return;
			}
			foreach (VirtualKey key in keys)
			{
				if (key.text.text == key.normalText)
					key.text.text = key.alternateText;
				else
					key.text.text = key.normalText;
			}
		}

		public virtual void OnBackspacePressed ()
		{
			StartCoroutine(ContinuouslyDoBackspaceRoutine ());
		}

		public virtual IEnumerator ContinuouslyDoBackspaceRoutine ()
		{
			DoBackspace ();
			yield return new WaitForSecondsRealtime(repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(repeatRate);
				DoBackspace ();
			}
		}

		public virtual void DoBackspace ()
		{
			if (OutputPosition > 0)
			{
				outputToInputField.text = outputToInputField.text.Remove(OutputPosition - 1, 1);
				OutputPosition --;
			}
		}

		public virtual void OnBackspaceRelased ()
		{
			// StopCoroutine(ContinuouslyDoBackspaceRoutine ());
			StopAllCoroutines();
		}

		public virtual void OnDeletePressed ()
		{
			StartCoroutine(ContinuouslyDoDeleteRoutine ());
		}

		public virtual IEnumerator ContinuouslyDoDeleteRoutine ()
		{
			DoDelete ();
			yield return new WaitForSecondsRealtime(repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(repeatRate);
				DoDelete ();
			}
		}

		public virtual void DoDelete ()
		{
			if (OutputPosition < outputToInputField.text.Length)
				outputToInputField.text = outputToInputField.text.Remove(OutputPosition, 1);
		}

		public virtual void OnDeleteReleased ()
		{
			// StopCoroutine(ContinuouslyDoDeleteRoutine ());
			StopAllCoroutines();
		}

		public virtual void OnLeftArrowPressed ()
		{
			StartCoroutine(ContinuouslyDoLeftArrowRoutine ());
		}

		public virtual IEnumerator ContinuouslyDoLeftArrowRoutine ()
		{
			DoLeftArrow ();
			yield return new WaitForSecondsRealtime(repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(repeatRate);
				DoLeftArrow ();
			}
		}

		public virtual void DoLeftArrow ()
		{
			if (OutputPosition > 0)
				OutputPosition --;
		}

		public virtual void OnLeftArrowReleased ()
		{
			// StopCoroutine(ContinuouslyDoLeftArrowRoutine ());
			StopAllCoroutines();
		}

		public virtual void OnRightArrowPressed ()
		{
			StartCoroutine(ContinuouslyDoRightArrowRoutine ());
		}

		public virtual IEnumerator ContinuouslyDoRightArrowRoutine ()
		{
			DoRightArrow ();
			yield return new WaitForSecondsRealtime(repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(repeatRate);
				DoRightArrow ();
			}
		}

		public virtual void DoRightArrow ()
		{
			if (OutputPosition < outputToInputField.text.Length)
				OutputPosition ++;
		}

		public virtual void OnRightArrowReleased ()
		{
			// StopCoroutine(ContinuouslyDoRightArrowRoutine ());
			StopAllCoroutines();
		}

		public virtual void OnDownArrowPressed ()
		{
			StartCoroutine(ContinuouslyDoDownArrowRoutine ());
		}

		public virtual IEnumerator ContinuouslyDoDownArrowRoutine ()
		{
			DoDownArrow ();
			yield return new WaitForSecondsRealtime(repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(repeatRate);
				DoDownArrow ();
			}
		}

		public virtual void DoDownArrow ()
		{
			OutputPosition = outputToInputField.text.Length;
		}

		public virtual void OnDownArrowReleased ()
		{
			// StopCoroutine(ContinuouslyDoDownArrowRoutine ());
			StopAllCoroutines();
		}

		public virtual void OnUpArrowPressed ()
		{
			StartCoroutine(ContinuouslyDoUpArrowRoutine ());
		}

		public virtual IEnumerator ContinuouslyDoUpArrowRoutine ()
		{
			DoUpArrow ();
			yield return new WaitForSecondsRealtime(repeatDelay);
			while (true)
			{
				yield return new WaitForSecondsRealtime(repeatRate);
				DoUpArrow ();
			}
		}

		public virtual void DoUpArrow ()
		{
			OutputPosition = 0;
		}

		public virtual void OnUpArrowReleased ()
		{
			// StopCoroutine(ContinuouslyDoUpArrowRoutine ());
			StopAllCoroutines();
		}
	}
}