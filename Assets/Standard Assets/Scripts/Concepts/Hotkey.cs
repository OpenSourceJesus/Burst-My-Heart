using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	public class Hotkey : MonoBehaviour, IUpdatable
	{
		public bool anyButton;
		public InputButton inputButton;
		public Button button;
		bool anyButtonDown;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true && GameManager.GetSingleton<OnlineBattle>() == null;
			}
		}

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (button == null)
					button = GetComponent<Button>();
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
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void DoUpdate ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (anyButton)
			{
				anyButtonDown = Input.anyKeyDown || InputManager.inputters[0].GetAnyButtonDown() || InputManager.inputters[1].GetAnyButtonDown() || InputManager.inputters[0].GetAnyNegativeButtonDown() || InputManager.inputters[1].GetAnyNegativeButtonDown();
				if (!anyButtonDown)
				{
					for (int i = 0; i < 20; i ++)
					{
						if (Input.GetKeyDown("joystick button " + i))
						{
							anyButtonDown = true;
							break;
						}
					}
				}
			}
			if (inputButton.GetDown() || (anyButtonDown))
				button.onClick.Invoke();
		}
	}
}