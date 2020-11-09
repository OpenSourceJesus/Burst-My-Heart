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
				GameManager.Instance.LoadScene (loadScene);
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
			if (Menus.Instance.toggleAllEventsTextMeshes.Contains(textMesh))
			{
				bool newEnabledValue = !(SaveAndLoadManager.GetValue<bool>(windEventEnabledPlayerPrefsKey, true) && SaveAndLoadManager.GetValue<bool>(scoreMultiplierEventEnabledPlayerPrefsKey, true));
				SaveAndLoadManager.SetValue(windEventEnabledPlayerPrefsKey, newEnabledValue);
				SaveAndLoadManager.SetValue(scoreMultiplierEventEnabledPlayerPrefsKey, newEnabledValue);
			}
			else if (Menus.Instance.toggleWindEventTextMeshes.Contains(textMesh))
				SaveAndLoadManager.SetValue(windEventEnabledPlayerPrefsKey, !SaveAndLoadManager.GetValue<bool>(windEventEnabledPlayerPrefsKey, true));
			else if (Menus.Instance.toggleScoreMultiplierEventTextMeshes.Contains(textMesh))
				SaveAndLoadManager.SetValue(scoreMultiplierEventEnabledPlayerPrefsKey, !SaveAndLoadManager.GetValue<bool>(scoreMultiplierEventEnabledPlayerPrefsKey, true));
			Menus.Instance.UpdateToggleEventTextMeshes ();
		}

		public virtual void TryToGoOnline ()
		{
			if (!NetworkManager.IsOnline)
			{
				if (!OnlineBattle.isWaitingForAnotherPlayer)
				{
					OnlineBattle.isWaitingForAnotherPlayer = true;
					NetworkManager.Instance.notificationTextObject.text.text = OnlineBattle.Instance.isWaitingForAnotherPlayerText;
					GameManager.Instance.StopCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
					NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
					OnlineBattle.Instance.Connect ();
					// NetworkManager.IsOnline = true;
					// GameManager.Instance.LoadScene("Online");
				}
				else
				{
					OnlineBattle.Instance.OnDestroy ();
					NetworkManager.Instance.notificationTextObject.text.text = OnlineBattle.Instance.isNotWaitingForAnotherPlayerText;
					GameManager.Instance.StopCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
					NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
				}
			}
			else
			{
				if (!OnlineBattle.isWaitingForAnotherPlayer)
				{
					NetworkManager.Instance.notificationTextObject.text.text = "Multiple game instances on a single computer playing multiplayer is not allowed";
					GameManager.Instance.StopCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
					NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
				}
				else
				{
					OnlineBattle.Instance.OnDestroy ();
					NetworkManager.Instance.notificationTextObject.text.text = OnlineBattle.Instance.isNotWaitingForAnotherPlayerText;
					GameManager.Instance.StopCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
					NetworkManager.Instance.StartCoroutine(NetworkManager.Instance.notificationTextObject.DisplayRoutine ());
				}
			}
		}

		public virtual void OpenDeleteAccountScreen ()
		{
			ArchivesManager.indexOfCurrentAccountToDelete = trs.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetSiblingIndex() - 1;
			ArchivesManager.currentAccountToDelete = ArchivesManager.Instance.localAccountsData[ArchivesManager.indexOfCurrentAccountToDelete];
			ArchivesManager.Instance.deleteAccountText.text = "Delete Account " + ArchivesManager.currentAccountToDelete.username;
			ArchivesManager.Instance.deleteAccountScreen.SetActive(true);
			Menus.Instance.enabled = false;
		}

		public virtual void OpenAccountInfoScreen ()
		{
			ArchivesManager.currentAccountToViewInfo = ArchivesManager.Instance.localAccountsData[trs.parent.parent.parent.GetSiblingIndex()];
			ArchivesManager.Instance.accountInfoTitleText.text = ArchivesManager.currentAccountToViewInfo.username + " Account Info";
			ArchivesManager.Instance.accountInfoContentText.text = ArchivesManager.currentAccountToViewInfo.ToString();
			ArchivesManager.Instance.accountInfoScreen.SetActive(true);
			Menus.Instance.enabled = false;
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
			ArchivesManager.player1AccountData = ArchivesManager.Instance.localAccountsData[trs.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetSiblingIndex() - 1];
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
			ArchivesManager.player2AccountData = ArchivesManager.Instance.localAccountsData[trs.parent.parent.parent.parent.parent.parent.parent.parent.parent.GetSiblingIndex() - 1];
			enabled = false;
		}

		public virtual void StartContinousManageAccount ()
		{
			StartCoroutine(ContinousManageAccountRoutine ());
		}

		public virtual IEnumerator ContinousManageAccountRoutine ()
		{
			while (Menus.Instance.currentMenu.trs.parent.parent == trs)
			{
				Menus.Instance.currentMenu.description = "Manage Account " + ArchivesManager.LocalAccountNames[trs.parent.parent.parent.GetSiblingIndex() - 1];
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