using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Extensions;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace BMH
{
	[ExecuteAlways]
	public class Menus : SingletonMonoBehaviour<Menus>, IUpdatable
	{
		float repeatTimer;
		float prevHorizontalAxis;
		float prevVerticalAxis;
		Vector2 input;
		public float repeatDelay;
		public float repeatRate;
		public float cameraLerpRate;
		public float slerpRate;
		public Vector3 cameraOffset;
		public Vector3 cameraRota;
		public Menu currentMenu;
		int desiredDescriptionAlpha = 1;
		public TextMesh descriptionTextMesh;
		public InputAxis horizontalKeyboardAxis;
		public InputAxis verticalKeyboardAxis;
		public float randomizePlayerPositionsRange;
		public float minDescriptionTextMeshColorAlpha = .05f;
		public Menu[] menus = new Menu[0];
		public Button autoClickAfterInput;
		public Button AutoClickAfterInput
		{
			set
			{
				autoClickAfterInput = value;
			}
		}
		public TextMesh[] toggleAllEventsTextMeshes = new TextMesh[0];
		public TextMesh[] toggleWindEventTextMeshes = new TextMesh[0];
		public TextMesh[] toggleScoreMultiplierEventTextMeshes = new TextMesh[0];
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}

		public override void Awake ()
		{
#if UNITY_EDITOR
	    	if (!Application.isPlaying)
	    	{
		    	if (descriptionTextMesh == null)
		    		descriptionTextMesh = GameObject.Find("Fade Text").GetComponent<TextMesh>();
	    		// if (menus.Length == 0)
		    	// 	menus = FindObjectsOfType<Menu>();
	    		return;
	    	}
#endif
			if (!enabled)
				return;
			menus = FindObjectsOfType<Menu>();
			for (int i = 0; i < menus.Length; i ++)
			{
				if (!menus[i].trs.root.GetComponent<Menus>().enabled)
				{
					menus = menus.RemoveAt(i);
					i --;
				}
			}
			UpdateToggleEventTextMeshes ();
	    	base.Awake ();
			if (Player.players.Length == 0)
			{
				Player.players = FindObjectsOfType<Player>();
				foreach (Player player in Player.players)
				{
					player.body.Hp = player.body.maxHp;
					player.trs.position = Random.insideUnitCircle * randomizePlayerPositionsRange;
					player.trs.eulerAngles = Vector3.forward * Random.value * 360;
				}
			}
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
		}

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
	    	if (!Application.isPlaying)
	    		return;
#endif
			if (enabled)
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

		public virtual void UpdateToggleEventTextMeshes ()
		{
			bool windEventEnabled = SaveAndLoadManager.GetValue<bool>("Wind Event enabled", true);
			bool scoreMultiplierEventEnabled = SaveAndLoadManager.GetValue<bool>("Bounty Multiplier Event enabled", true);
			bool anyEventEnabled = windEventEnabled || scoreMultiplierEventEnabled;
			for (int i = 0; i < toggleAllEventsTextMeshes.Length; i ++)
			{
				if (anyEventEnabled)
				{
					toggleAllEventsTextMeshes[i].text = "Disable";
				}
				else
				{
					toggleAllEventsTextMeshes[i].text = "Enable";
				}
				if (windEventEnabled)
				{
					toggleWindEventTextMeshes[i].text = "Disable Wind Event";
					toggleWindEventTextMeshes[i].GetComponent<Transform>().localScale = new Vector3(0.0001f, 0.0001f, 1);
				}
				else
				{
					toggleWindEventTextMeshes[i].text = "Enable Wind Event";
					toggleWindEventTextMeshes[i].GetComponent<Transform>().localScale = new Vector3(0.0001f, 0.0001f, 1);
				}
				if (scoreMultiplierEventEnabled)
				{
					toggleScoreMultiplierEventTextMeshes[i].text = "Disable Score Multiplier Event";
					toggleScoreMultiplierEventTextMeshes[i].GetComponent<Transform>().localScale = new Vector3(0.0000625f, 0.0000625f, 1);
				}
				else
				{
					toggleScoreMultiplierEventTextMeshes[i].text = "Enable Score Multiplier Event";
					toggleScoreMultiplierEventTextMeshes[i].GetComponent<Transform>().localScale = new Vector3(0.0000625f, 0.0000625f, 1);
				}
			}
		}

		public virtual void DoUpdate ()
		{
	    	foreach (Menu menu in menus)
			{
				if (menu != null)
					menu.trs.rotation = Quaternion.Slerp(menu.trs.rotation, Quaternion.Euler(Vector3.forward * 360f / menu.trs.childCount * menu.currentSelection), slerpRate * Time.deltaTime);
			}
			input = GetInput();
			if (Mathf.Abs(input.y) > GameManager.GetSingleton<InputManager>().joystickDeadzone)
			{
				if (Mathf.Abs(prevVerticalAxis) <= GameManager.GetSingleton<InputManager>().joystickDeadzone)
				{
					repeatTimer = -repeatDelay;
					ChangeSelectionVertical ((int) Mathf.Sign(input.y));
				}
				else
				{
					repeatTimer += Time.deltaTime;
					if (repeatTimer >= repeatRate)
					{
						ChangeSelectionVertical ((int) Mathf.Sign(input.y));
						repeatTimer = 0;
					}
				}
			}
			GameManager.GetSingleton<CameraScript>().trs.position = Vector3.Lerp(GameManager.GetSingleton<CameraScript>().trs.position, currentMenu.trs.position + cameraOffset, cameraLerpRate * Time.deltaTime);
			GameManager.GetSingleton<CameraScript>().trs.eulerAngles = cameraRota;
	        GameManager.GetSingleton<CameraScript>().trs.GetChild(0).gameObject.SetActive(true);
			float descriptionTextMeshColorAlpha = descriptionTextMesh.color.a;
			if (descriptionTextMeshColorAlpha <= minDescriptionTextMeshColorAlpha)
			{
				descriptionTextMesh.text = currentMenu.description;
				desiredDescriptionAlpha = 1;
			}
			descriptionTextMeshColorAlpha = Mathf.Lerp(descriptionTextMeshColorAlpha, desiredDescriptionAlpha, cameraLerpRate * 2 * Time.deltaTime);
			descriptionTextMesh.color = descriptionTextMesh.color.SetAlpha(descriptionTextMeshColorAlpha);
			if (Mathf.Abs(input.x) > GameManager.GetSingleton<InputManager>().joystickDeadzone && Mathf.Abs(prevHorizontalAxis) <= GameManager.GetSingleton<InputManager>().joystickDeadzone)
				ChangeSelectionHorizonal ((int) Mathf.Sign(input.x));
			prevHorizontalAxis = input.x;
			prevVerticalAxis = input.y;
		}

		public virtual void ChangeSelectionVertical (int direction)
		{
			currentMenu.ChangeSelectionVertical (direction);
		}

		public virtual void ChangeSelectionHorizonal (int direction)
		{
			currentMenu.ChangeSelectionHorizonal (direction);
			desiredDescriptionAlpha = 0;
		}

	    public virtual void SetCurrentMenu (Menu menu)
	    {
	        currentMenu = menu;
	        currentMenu.enabled = true;
	        currentMenu.Awake ();
	    }

	    public virtual void SetCurrentSelection (int selection)
	    {
			currentMenu.SetCurrentSelection (selection);
	    }

	    public virtual Vector2 GetInput ()
	    {
	    	Vector2 output = new Vector2();
	    	output.x = InputManager.inputters[0].GetAxis("Menu Horizontal") + InputManager.inputters[1].GetAxis("Menu Horizontal") + horizontalKeyboardAxis.Get();
	    	output.y = InputManager.inputters[0].GetAxis("Menu Vertical") + InputManager.inputters[1].GetAxis("Menu Vertical") + verticalKeyboardAxis.Get();
	    	if (autoClickAfterInput != null && output != new Vector2())
	    	{
	    		autoClickAfterInput.onClick.Invoke();
	    		autoClickAfterInput = null;
	    	}
	    	return output;
	    }
	}
}