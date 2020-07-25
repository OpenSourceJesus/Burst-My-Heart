using UnityEngine;
using System.Collections;
using Extensions;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace BMH
{
	//[ExecuteAlways]
	public class MenuOption : MonoBehaviour
	{
		public Transform trs;
		public PlayerPrefsChangeEntry[] playerPrefsChanges = new PlayerPrefsChangeEntry[0];
		public Button invokeButton;
		public UnityEvent invokeEvent;
		public string loadScene;
		public TextMesh textMesh;
		public MeshRenderer sphereMeshRenderer;
		public MeshRenderer connectionMeshRenderer;
		public Material enabledSphereMaterial;
		public Material disabledSphereMatierial;
		public float enabledTextMeshColorAlpha = 1;
		public float disabledTextMeshColorAlpha = 0.5f;

#if UNITY_EDITOR
		public virtual void Start ()
		{
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (sphereMeshRenderer == null)
					sphereMeshRenderer = trs.parent.GetComponentInParent<MeshRenderer>();
				if (connectionMeshRenderer == null)
					connectionMeshRenderer = GetComponent<MeshRenderer>();
				if (textMesh == null)
					textMesh = trs.parent.parent.GetChild(1).GetComponentInChildren<TextMesh>();
				if (enabled)
					enabledSphereMaterial = sphereMeshRenderer.sharedMaterial;
				else
					disabledSphereMatierial = sphereMeshRenderer.sharedMaterial;
				if (invokeButton != null)
					invokeEvent = invokeButton.onClick;
				return;
			}
		}
#endif

		public virtual void PickOption ()
		{
			foreach (PlayerPrefsChangeEntry playerPrefsChange in playerPrefsChanges)
				playerPrefsChange.Do ();
			if (invokeEvent != null)
				invokeEvent.Invoke();
			if (!string.IsNullOrEmpty(loadScene) && trs.GetChild(0).GetChild(0).childCount == 0)
				GameManager.GetSingleton<GameManager>().LoadScene (loadScene);
		}

		public virtual void OnDisable ()
		{
			if (!enabled)
			{
				sphereMeshRenderer.sharedMaterial = disabledSphereMatierial;
				textMesh.color = textMesh.color.SetAlpha(disabledTextMeshColorAlpha);
			}
		}

		public virtual void OnEnable ()
		{
			sphereMeshRenderer.sharedMaterial = enabledSphereMaterial;
			textMesh.color = textMesh.color.SetAlpha(enabledTextMeshColorAlpha);
		}

		public virtual void ToggleEvent ()
		{
			string windEventEnabledPlayerPrefsKey = "Wind Event enabled";
			string scoreMultiplierEventEnabledPlayerPrefsKey = "Bounty Multiplier Event enabled";
			if (GameManager.GetSingleton<Menus>().toggleAllEventsTextMeshes.Contains(textMesh))
			{
				bool newEnabledValue = !(SaveAndLoadManager.GetValue<bool>(windEventEnabledPlayerPrefsKey, true) && SaveAndLoadManager.GetValue<bool>(scoreMultiplierEventEnabledPlayerPrefsKey, true));
				SaveAndLoadManager.SetValue(windEventEnabledPlayerPrefsKey, newEnabledValue);
				SaveAndLoadManager.SetValue(scoreMultiplierEventEnabledPlayerPrefsKey, newEnabledValue);
			}
			else if (GameManager.GetSingleton<Menus>().toggleWindEventTextMeshes.Contains(textMesh))
				SaveAndLoadManager.SetValue(windEventEnabledPlayerPrefsKey, !SaveAndLoadManager.GetValue<bool>(windEventEnabledPlayerPrefsKey, true));
			else if (GameManager.GetSingleton<Menus>().toggleScoreMultiplierEventTextMeshes.Contains(textMesh))
				SaveAndLoadManager.SetValue(scoreMultiplierEventEnabledPlayerPrefsKey, !SaveAndLoadManager.GetValue<bool>(scoreMultiplierEventEnabledPlayerPrefsKey, true));
			GameManager.GetSingleton<Menus>().UpdateToggleEventTextMeshes ();
		}

		public virtual void TryToGoOnline ()
		{
			if (!NetworkManager.IsOnline)
			{
				if (!OnlineBattle.isWaitingForAnotherPlayer)
				{
					OnlineBattle.isWaitingForAnotherPlayer = true;
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = GameManager.GetSingleton<OnlineBattle>().isWaitingForAnotherPlayerText;
					GameManager.GetSingleton<GameManager>().StopCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<OnlineBattle>().Connect ();
					// NetworkManager.IsOnline = true;
					// GameManager.GetSingleton<GameManager>().LoadScene("Online");
				}
				else
				{
					GameManager.GetSingleton<OnlineBattle>().OnDestroy ();
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = GameManager.GetSingleton<OnlineBattle>().isNotWaitingForAnotherPlayerText;
					GameManager.GetSingleton<GameManager>().StopCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				}
			}
			else
			{
				if (!OnlineBattle.isWaitingForAnotherPlayer)
				{
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Multiple game instances on a single computer playing multiplayer is not allowed";
					GameManager.GetSingleton<GameManager>().StopCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				}
				else
				{
					GameManager.GetSingleton<OnlineBattle>().OnDestroy ();
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = GameManager.GetSingleton<OnlineBattle>().isNotWaitingForAnotherPlayerText;
					GameManager.GetSingleton<GameManager>().StopCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				}
			}
		}

		public virtual void OpenDeleteAccountScreen ()
		{
			ArchivesManager.indexOfCurrentAccountToDelete = trs.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetSiblingIndex() - 1;
			ArchivesManager.currentAccountToDelete = GameManager.GetSingleton<ArchivesManager>().localAccountsData[ArchivesManager.indexOfCurrentAccountToDelete];
			GameManager.GetSingleton<ArchivesManager>().deleteAccountText.text = "Delete Account " + ArchivesManager.currentAccountToDelete.username;
			GameManager.GetSingleton<ArchivesManager>().deleteAccountScreen.SetActive(true);
			GameManager.GetSingleton<Menus>().enabled = false;
		}

		public virtual void OpenAccountInfoScreen ()
		{
			ArchivesManager.currentAccountToViewInfo = GameManager.GetSingleton<ArchivesManager>().localAccountsData[trs.parent.parent.parent.GetSiblingIndex()];
			GameManager.GetSingleton<ArchivesManager>().accountInfoTitleText.text = ArchivesManager.currentAccountToViewInfo.username + " Account Info";
			GameManager.GetSingleton<ArchivesManager>().accountInfoContentText.text = ArchivesManager.currentAccountToViewInfo.ToString();
			GameManager.GetSingleton<ArchivesManager>().accountInfoScreen.SetActive(true);
			GameManager.GetSingleton<Menus>().enabled = false;
		}

		public virtual void AssignAccountToPlayer1 ()
		{
			if (ArchivesManager.player1AccountAssigner != null)
				ArchivesManager.player1AccountAssigner.enabled = true;
			if (ArchivesManager.player2AccountAssigner != null && ArchivesManager.player2AccountAssigner.trs.parent.parent.parent.parent == trs.parent.parent.parent.parent)
			{
				ArchivesManager.player2AccountAssigner.enabled = true;
				ArchivesManager.player2AccountData = null;
			}
			ArchivesManager.player1AccountAssigner = this;
			ArchivesManager.player1AccountData = GameManager.GetSingleton<ArchivesManager>().localAccountsData[trs.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetSiblingIndex() - 1];
			enabled = false;
		}

		public virtual void AssignAccountToPlayer2 ()
		{
			if (ArchivesManager.player2AccountAssigner != null)
				ArchivesManager.player2AccountAssigner.enabled = true;
			if (ArchivesManager.player1AccountAssigner != null && ArchivesManager.player1AccountAssigner.trs.parent.parent.parent.parent == trs.parent.parent.parent.parent)
			{
				ArchivesManager.player1AccountAssigner.enabled = true;
				ArchivesManager.player1AccountData = null;
			}
			ArchivesManager.player2AccountAssigner = this;
			ArchivesManager.player2AccountData = GameManager.GetSingleton<ArchivesManager>().localAccountsData[trs.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetSiblingIndex() - 1];
			enabled = false;
		}

		public virtual void StartContinousManageAccount ()
		{
			StartCoroutine(ContinousManageAccountRoutine ());
		}

		public virtual IEnumerator ContinousManageAccountRoutine ()
		{
			while (GameManager.GetSingleton<Menus>().currentMenu.trs.parent.parent == trs)
			{
				GameManager.GetSingleton<Menus>().currentMenu.description = "Manage Account " + ArchivesManager.LocalAccountNames[trs.parent.parent.parent.GetSiblingIndex() - 1];
				yield return new WaitForEndOfFrame();
			}
		}

		[Serializable]
		public class PlayerPrefsChangeEntry
		{
			public string key;
			public bool addActivePlayerUsernameToKey;
			public bool isBoolValue;
			public int intValue = MathfExtensions.NULL_INT;
			public float floatValue = MathfExtensions.NULL_FLOAT;
			public string stringValue = null;
			public bool delete;

			public virtual void Do ()
			{
				string _key = key;
				if (addActivePlayerUsernameToKey)
					_key = ArchivesManager.ActivePlayerUsername + ArchivesManager.VALUE_SEPARATOR + _key;
				if (delete)
					SaveAndLoadManager.RemoveData (_key);
				else
				{
					if (intValue != MathfExtensions.NULL_INT)
					{
						if (isBoolValue)
						{
							if (intValue == 0)
								SaveAndLoadManager.SetValue(_key, false);
							else
								SaveAndLoadManager.SetValue(_key, true);
						}
						else
							SaveAndLoadManager.SetValue(_key, intValue);
					}
					else if (floatValue != MathfExtensions.NULL_FLOAT)
						SaveAndLoadManager.SetValue(_key, floatValue);
					else if (!string.IsNullOrEmpty(stringValue))
						SaveAndLoadManager.SetValue(_key, stringValue);
				}
			}
		}
	}
}