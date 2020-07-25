using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;
using System;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;
using PlayerIOClient;

namespace BMH
{
	//[ExecuteAlways]
	public class ArchivesManager : SingletonMonoBehaviour<ArchivesManager>
	{
		public const string EMPTY_ACCOUNT_INDICATOR = "␀";
		public const int MAX_ACCOUNTS = 5;
		public const string VALUE_SEPARATOR = "⧫";
		public static string[] LocalAccountNames
		{
			get
			{
				return SaveAndLoadManager.GetValue<string[]>("Local account names", new string[0]);
			}
			set
			{
				SaveAndLoadManager.SetValue("Local account names", value);
			}
		}
		public static string[] LocalAccountPasswords
		{
			get
			{
				return SaveAndLoadManager.GetValue<string[]>("Local account passwords", new string[0]);
			}
			set
			{
				SaveAndLoadManager.SetValue("Local account passwords", value);
			}
		}
		public AccountData[] localAccountsData = new AccountData[0];
		public AccountData newAccountData;
		public static AccountData player1AccountData;
		public static AccountData player2AccountData;
		public static AccountData activeAccountData;
		public MenuOption addAccountOption;
		public MenuOption[] accountOptions = new MenuOption[0];
		public MenuOption[] viewInfoMenuOptions = new MenuOption[0];
		public static string ActivePlayerUsername
		{
			get
			{
				if (activeAccountData == null)
					return EMPTY_ACCOUNT_INDICATOR;
				else
					return activeAccountData.username;
			}
		}
		public GameObject deleteAccountScreen;
		public Text deleteAccountText;
		public static int indexOfCurrentAccountToDelete;
		public static AccountData currentAccountToDelete;
		public GameObject accountInfoScreen;
		public Text accountInfoTitleText;
		public Text accountInfoContentText;
		public static AccountData currentAccountToViewInfo;
		public Scrollbar accountInfoScrollbar;
		public static MenuOption player1AccountAssigner;
		public static MenuOption player2AccountAssigner;
		public Transform trs;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (GameManager.GetSingleton<ArchivesManager>() != null && GameManager.GetSingleton<ArchivesManager>() != this)
			{
				UpdateMenus ();
				GameManager.GetSingleton<ArchivesManager>().addAccountOption = addAccountOption;
				GameManager.GetSingleton<ArchivesManager>().accountOptions = accountOptions;
				GameManager.GetSingleton<ArchivesManager>().viewInfoMenuOptions = viewInfoMenuOptions;
				GameManager.GetSingleton<ArchivesManager>().deleteAccountScreen = deleteAccountScreen;
				GameManager.GetSingleton<ArchivesManager>().deleteAccountText = deleteAccountText;
				GameManager.GetSingleton<ArchivesManager>().accountInfoScreen = accountInfoScreen;
				GameManager.GetSingleton<ArchivesManager>().accountInfoTitleText = accountInfoTitleText;
				GameManager.GetSingleton<ArchivesManager>().accountInfoContentText = accountInfoContentText;
				GameManager.GetSingleton<ArchivesManager>().accountInfoScrollbar = accountInfoScrollbar;
				return;
			}
			base.Awake ();
			trs.SetParent(null);
			if (BuildManager.IsFirstStartup)
			{
				if (GameManager.GetSingleton<BuildManager>().clearDataOnFirstStartup)
				{
					SaveAndLoadManager.data.Clear();
					if (GameManager.GetSingleton<SaveAndLoadManager>().usePlayerPrefs)
						PlayerPrefs.DeleteAll();
					else
						File.Delete(GameManager.GetSingleton<SaveAndLoadManager>().saveFileFullPath);
				}
				else
					GameManager.GetSingleton<SaveAndLoadManager>().Load ();
				BuildManager.IsFirstStartup = false;
			}
			else
				GameManager.GetSingleton<SaveAndLoadManager>().Load ();
			Connect ();
		}

		public virtual void Connect ()
		{
			GameManager.GetSingleton<NetworkManager>().Connect (OnAuthenticateSucess, OnAuthenticateFail);
		}

		public virtual void OnAuthenticateSucess (Client client)
		{
			Debug.Log("OnAuthenticateSucess");
			NetworkManager.client = client;
		}

		public virtual void OnAuthenticateFail (PlayerIOError error)
		{
			Debug.Log("OnAuthenticateFail: " + error.ToString());
			// Connect ();
		}

		public virtual void UpdateMenus ()
		{
			if (accountOptions[0] == null)
				return;
			MenuOption accountOption;
			for (int i = 0; i < MAX_ACCOUNTS; i ++)
			{
				if (LocalAccountNames.Length > i)
				{
					accountOption = accountOptions[i];
					accountOption.enabled = true;
					accountOption.textMesh.text = LocalAccountNames[i];
					accountOption = viewInfoMenuOptions[i];
					accountOption.enabled = true;
					accountOption.textMesh.text = LocalAccountNames[i];
				}
				else
				{
					accountOption = accountOptions[i];
					accountOption.enabled = false;
					accountOption.textMesh.text = "Account " + (i + 1);
					accountOption = viewInfoMenuOptions[i];
					accountOption.enabled = false;
					accountOption.textMesh.text = "Account " + (i + 1);
				}
				accountOptions[i].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = true;
				accountOptions[i].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = true;
			}
			if (player1AccountData != null)
				accountOptions[GameManager.GetSingleton<ArchivesManager>().localAccountsData.IndexOf(player1AccountData)].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = false;
			if (player2AccountData != null)
				accountOptions[GameManager.GetSingleton<ArchivesManager>().localAccountsData.IndexOf(player2AccountData)].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = false;
			addAccountOption.enabled = LocalAccountNames.Length < MAX_ACCOUNTS;
		}
		
		public virtual void StartContinuousContinuousScrollAccountInfo (float velocity)
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().StartContinuousContinuousScrollAccountInfo (velocity);
				return;
			}
			StartCoroutine(ContinuousScrollAccountInfoRoutine (velocity));
		}

		public virtual IEnumerator ContinuousScrollAccountInfoRoutine (float velocity)
		{
			while (true)
			{
				ScrollAccountInfo (velocity);
				yield return new WaitForEndOfFrame();
			}
		}

		public virtual void ScrollAccountInfo (float velocity)
		{
			accountInfoScrollbar.value += velocity * Time.deltaTime;
		}

		public virtual void EndContinuousContinuousScrollAccountInfo ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().EndContinuousContinuousScrollAccountInfo ();
				return;
			}
			StopAllCoroutines();
		}

		public virtual void TryToSetNewAccountUsername ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().TryToSetNewAccountUsername ();
				return;
			}
			GameManager.GetSingleton<VirtualKeyboard>().DisableInput ();
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(true);
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Loading...";
			string username = GameManager.GetSingleton<VirtualKeyboard>().outputToInputField.text;
			newAccountData.username = username.Replace(" ", "");
			if (newAccountData.username.Length == 0)
			{
				GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "The username must contain at least one non-space character";
				GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				return;
			}
			NetworkManager.client.BigDB.LoadMyPlayerObject(
				delegate (DatabaseObject dbObject)
				{
					if (dbObject.Count > 0)
					{
						GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "The username can't be used. It has already been registered online by someone else.";
						GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
						GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
						return;
					}
					else
					{
						GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(false);
						GameManager.GetSingleton<VirtualKeyboard>().trs.parent.gameObject.SetActive(false);
						GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
						NetworkManager.client.BigDB.LoadOrCreate("PlayerObjects", username, OnNewAccountDBObjectCreateSuccess, OnNewAccountDBObjectCreateFail);
					}
				},
				delegate (PlayerIOError error)
				{
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				}
			);
		}

		public virtual void OnNewAccountDBObjectCreateSuccess (DatabaseObject dbObject)
		{
			newAccountData.username = dbObject.Key;
			GameManager.GetSingleton<VirtualKeyboard>().trs.parent.parent.GetChild(1).gameObject.SetActive(true);
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
		}

		public virtual void OnNewAccountDBObjectCreateFail (PlayerIOError error)
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
			GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
		}

		public virtual void TryToSetNewAccountPassword ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().TryToSetNewAccountPassword ();
				return;
			}
			GameManager.GetSingleton<VirtualKeyboard>().DisableInput ();
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(true);
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Loading...";
			newAccountData.password = GameManager.GetSingleton<VirtualKeyboard>().outputToInputField.text;
			NetworkManager.client.BigDB.LoadMyPlayerObject(
				delegate (DatabaseObject dbObject)
				{
					if (dbObject.Count > 0)
					{
						GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "The username previously chosen can't be used. It has already been registered online by someone else.";
						GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
						GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
						return;
					}
					else
					{
						dbObject.Set("password", newAccountData.password);
						dbObject.Save(true, false, OnNewAccountDBObjectSaveSuccess, OnNewAccountDBObjectSaveFail);
					}
				},
				delegate (PlayerIOError error)
				{
					GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
					GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				}
			);
		}

		public virtual void OnNewAccountDBObjectSaveSuccess ()
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().trs.parent.gameObject.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
			localAccountsData[LocalAccountNames.Length].username = newAccountData.username;
			localAccountsData[LocalAccountNames.Length].password = newAccountData.password;
			foreach (AccountData accountData in localAccountsData)
				accountData.UpdateData ();
			LocalAccountNames = LocalAccountNames.Add(newAccountData.username);
			UpdateMenus ();
		}

		public virtual void OnNewAccountDBObjectSaveFail (PlayerIOError error)
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
			GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
		}

		public virtual void DeleteAccount ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().DeleteAccount ();
				return;
			}
			NetworkManager.client.BigDB.DeleteKeys("PlayerObjects", new string[1] { LocalAccountNames[indexOfCurrentAccountToDelete] }, OnDelteAccountDBObjectSuccess, OnDeleteAccountDBObjectFail);
		}

		public virtual void OnDelteAccountDBObjectSuccess ()
		{
			foreach (string key in SaveAndLoadManager.data.Keys)
			{
				if (key.StartsWith(LocalAccountNames[indexOfCurrentAccountToDelete] + VALUE_SEPARATOR))
					SaveAndLoadManager.data.Remove(key);
			}
			localAccountsData[indexOfCurrentAccountToDelete].Reset ();
			LocalAccountNames = LocalAccountNames.RemoveAt(indexOfCurrentAccountToDelete);
			LocalAccountPasswords = LocalAccountPasswords.RemoveAt(indexOfCurrentAccountToDelete);
			foreach (AccountData accountData in localAccountsData)
				accountData.UpdateData ();
			if (activeAccountData == localAccountsData[indexOfCurrentAccountToDelete])
				activeAccountData = null;
			if (player1AccountData == localAccountsData[indexOfCurrentAccountToDelete])
				player1AccountData = null;
			else if (player2AccountData == localAccountsData[indexOfCurrentAccountToDelete])
				player2AccountData = null;
			GameManager.GetSingleton<SaveAndLoadManager>().Save ();
			UpdateMenus ();
		}

		public virtual void OnDeleteAccountDBObjectFail (PlayerIOError error)
		{
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.text.text = "Error: " + error.ToString();
			GameManager.GetSingleton<NetworkManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
		}

		public virtual void UpdateAccountData (AccountData accountData)
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().UpdateAccountData (accountData);
				return;
			}
		}
	}
}