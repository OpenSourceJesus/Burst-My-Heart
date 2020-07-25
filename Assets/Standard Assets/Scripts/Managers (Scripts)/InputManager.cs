using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Extensions;
using System;
using UnityEngine.SceneManagement;

namespace BMH
{
	public class InputManager : MonoBehaviour, IUpdatable
	{
		public static Rewired.Player[] inputters = new Rewired.Player[0];
		public InputDevice[] defaultInputDevices = new InputDevice[0];
		public static InputDevice[] inputDevices = new InputDevice[0];
		public static bool UsingGamepad
		{
			get
			{
				return inputDevices.Contains(InputDevice.Gamepad);
			}
		}
		public static bool UsingTouchscreen
		{
			get
			{
				return inputDevices.Contains(InputDevice.Touchscreen);
			}
		}
		public static HumanPlayer[] humanPlayers = new HumanPlayer[0];
		public float joystickDeadzone;
		public Timer cursorUnhideTimer;
		Vector2 previousMousePosition;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		
		public virtual void OnEnable ()
		{
			inputters = new Rewired.Player[2];
			inputters[0] = ReInput.players.GetPlayer("Player 1");
			inputters[1] = ReInput.players.GetPlayer("Player 2");
			inputDevices = defaultInputDevices;
			if (ReInput.controllers.joystickCount > 0 && !inputDevices.Contains(InputDevice.Gamepad))
				inputDevices = inputDevices.Add(InputDevice.Gamepad);
			if (UsingGamepad)
			{
				if (ReInput.controllers.joystickCount > 1)
				{
					inputters[1].controllers.AddController(ReInput.controllers.Joysticks[1], true);
					inputters[0].controllers.AddController(ReInput.controllers.Joysticks[0], true);
				}
				else
					inputters[0].controllers.AddController(ReInput.controllers.Joysticks[0], true);
				Cursor.visible = false;
			}
			ReInput.ControllerConnectedEvent += OnControllerConnected;
			ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
			SceneManager.sceneLoaded += OnSceneLoaded;
			cursorUnhideTimer.onFinished += delegate{Cursor.visible = false;};
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void DoUpdate ()
		{
			if (UsingGamepad)
			{
				if ((Vector2) Input.mousePosition != previousMousePosition)
				{
					Cursor.visible = true;
					cursorUnhideTimer.Reset ();
					cursorUnhideTimer.Start ();
				}
			}
			else
				Cursor.visible = true;
			if (inputters[0].GetAnyButton())
			{
				if (ArchivesManager.activeAccountData != ArchivesManager.player1AccountData)
				{
					ArchivesManager.activeAccountData = ArchivesManager.player1AccountData;
					foreach (Player player in Player.players)
					{
						player.body.inputterId = 1 - player.body.inputterId;
						player.weapon.inputterId = 1 - player.weapon.inputterId;
					}
				}
			}
			else if (inputters[1].GetAnyButton())
			{
				if (ArchivesManager.activeAccountData != ArchivesManager.player2AccountData)
				{
					ArchivesManager.activeAccountData = ArchivesManager.player2AccountData;
					foreach (Player player in Player.players)
					{
						player.body.inputterId = 1 - player.body.inputterId;
						player.weapon.inputterId = 1 - player.weapon.inputterId;
					}
				}
			}
			previousMousePosition = Input.mousePosition;
		}

		public virtual void OnDisable ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void OnSceneLoaded (Scene scene = new Scene(), LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			humanPlayers = FindObjectsOfType<HumanPlayer>();
			if (humanPlayers.Length < 2 && inputters[1].controllers.hasKeyboard)
				inputters[0].controllers.AddController(inputters[1].controllers.GetLastActiveController(ControllerType.Keyboard), true);
			else if (humanPlayers.Length == 2 && inputters[0].controllers.joystickCount > 0)
				inputters[1].controllers.AddController(inputters[0].controllers.GetLastActiveController(ControllerType.Keyboard), true);
		}
		
		public virtual void OnControllerConnected (ControllerStatusChangedEventArgs args)
		{
			if (!inputDevices.Contains(InputDevice.Gamepad))
				inputDevices = inputDevices.Add(InputDevice.Gamepad);
			if (inputters[0].controllers.joystickCount == 0)
			{
				inputters[0].controllers.AddController(args.controller, true);
				if (humanPlayers.Length == 2)
					inputters[1].controllers.AddController(inputters[0].controllers.GetLastActiveController(ControllerType.Keyboard), true);
			}
			else if (inputters[1].controllers.joystickCount == 0)
				inputters[1].controllers.AddController(args.controller, true);
			foreach (_Text text in _Text.instances)
				text.UpdateText ();
		}

		public virtual void OnControllerPreDisconnect (ControllerStatusChangedEventArgs args)
		{
			if (!inputDevices.Contains(InputDevice.Gamepad) && ReInput.controllers.joystickCount > 0)
				inputDevices = inputDevices.Add(InputDevice.Gamepad);
			else if (inputDevices.Contains(InputDevice.Gamepad) && ReInput.controllers.joystickCount == 0)
				inputDevices = inputDevices.Remove(InputDevice.Gamepad);
			if (inputters[0].controllers.Joysticks.Contains_IList<Joystick>((Joystick) args.controller))
			{
				inputters[0].controllers.RemoveController(args.controller);
				if (inputters[1].controllers.joystickCount > 0)
					inputters[0].controllers.AddController(inputters[1].controllers.GetLastActiveController(ControllerType.Joystick), true);
			}
			else if (inputters[1].controllers.Joysticks.Contains_IList<Joystick>((Joystick) args.controller))
				inputters[1].controllers.RemoveController(args.controller);
			foreach (_Text text in _Text.instances)
				text.UpdateText ();
		}
		
		public virtual void OnDestroy ()
		{
			ReInput.ControllerConnectedEvent -= OnControllerConnected;
			ReInput.ControllerPreDisconnectEvent -= OnControllerPreDisconnect;
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public static Vector2 GetAxis2D (string xAxisName, string yAxisName, int inputterId)
		{
			return Vector2.ClampMagnitude(inputters[inputterId].GetAxis2D(xAxisName, yAxisName), 1);
		}

		public static Vector2 GetAxis2D (string xAxisName, string yAxisName)
		{
			Vector2 output = new Vector2();
			for (int i = 0; i < inputters.Length; i ++)
				output += GetAxis2D(xAxisName, yAxisName, i);
			return output;
		}

		public static bool GetButtonDown (string buttonName, int inputterId)
		{
			return inputters[inputterId].GetButtonDown(buttonName);
		}

		public static bool GetButtonDown (string buttonName)
		{
			for (int i = 0; i < inputters.Length; i ++)
			{
				if (inputters[i].GetButtonDown(buttonName))
					return true;
			}
			return false;
		}

		public static float GetAxis (string axisName, int inputterId)
		{
			return inputters[inputterId].GetAxis(axisName);
		}

		public static float GetAxis (string axisName)
		{
			float output = 0;
			for (int i = 0; i < inputters.Length; i ++)
				output += GetAxis(axisName, i);
			return output;
		}
	}

	[Serializable]
	public class InputButton
	{
		public string[] buttonNames;
		public KeyCode[] keyCodes;

		public virtual bool GetDown ()
		{
			bool output = false;
			foreach (KeyCode keyCode in keyCodes)
				output |= Input.GetKeyDown(keyCode);
			foreach (string buttonName in buttonNames)
				output |= InputManager.inputters[0].GetButtonDown(buttonName) || InputManager.inputters[1].GetButtonDown(buttonName);
			return output;
		}

		public virtual bool Get ()
		{
			bool output = false;
			foreach (KeyCode keyCode in keyCodes)
				output |= Input.GetKey(keyCode);
			foreach (string buttonName in buttonNames)
				output |= InputManager.inputters[0].GetButton(buttonName) || InputManager.inputters[1].GetButton(buttonName);
			return output;
		}

		public virtual bool GetUp ()
		{
			bool output = false;
			foreach (KeyCode keyCode in keyCodes)
				output |= Input.GetKeyUp(keyCode);
			foreach (string buttonName in buttonNames)
				output |= InputManager.inputters[0].GetButtonUp(buttonName) || InputManager.inputters[1].GetButtonUp(buttonName);
			return output;
		}
	}

	[Serializable]
	public class InputAxis
	{
		public InputButton positiveButton;
		public InputButton negativeButton;

		public virtual int Get ()
		{
			int output = 0;
			if (positiveButton.Get())
				output ++;
			if (negativeButton.Get())
				output --;
			return output;
		}
	}

	public enum InputDevice
	{
		Keyboard,
		Gamepad,
		Touchscreen
	}
}