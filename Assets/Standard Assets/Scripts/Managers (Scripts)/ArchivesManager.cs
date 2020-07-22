using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;
using System;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;

namespace BMH
{
	[ExecuteAlways]
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
		public static AccountData currentAccountToDelete;
		public GameObject accountInfoScreen;
		public Text accountInfoTitleText;
		public Text accountInfoContentText;
		public static AccountData currentAccountToViewInfo;
		public Scrollbar accountInfoScrollbar;
		public static MenuOption player1AccountAssigner;
		public static MenuOption player2AccountAssigner;

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
			if (GameManager.GetSingleton<BuildManager>().clearDataOnFirstStartup && BuildManager.IsFirstStartup)
			{
				SaveAndLoadManager.data.Clear();
				BuildManager.IsFirstStartup = false;
#if UNITY_WEBGL
				PlayerPrefs.DeleteAll();
#else
				File.Delete(GameManager.GetSingleton<SaveAndLoadManager>().saveFileFullPath);
#endif
			}
			else
				GameManager.GetSingleton<SaveAndLoadManager>().Load ();
		}

		public virtual void UpdateMenus ()
		{
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
				accountOptions[GameManager.GetSingleton<ArchivesManager>().localAccountsData.IndexOf_class(player1AccountData)].trs.GetChild(0).GetComponentInChildren<Menu>().options[0].enabled = false;
			if (player2AccountData != null)
				accountOptions[GameManager.GetSingleton<ArchivesManager>().localAccountsData.IndexOf_class(player2AccountData)].trs.GetChild(0).GetComponentInChildren<Menu>().options[1].enabled = false;
			addAccountOption.enabled = LocalAccountNames.Length < MAX_ACCOUNTS;
		}
		
		public virtual void StartContinuousContinuousScrollAccountInfo (float velocity)
		{
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
			StopAllCoroutines();
		}

		public virtual IEnumerator GetInfoRoutine ()
		{
			CoroutineWithData cd = new CoroutineWithData(this, GameManager.GetSingleton<NetworkManager>().PostFormToResource ("GetPlayersData.php", NetworkManager.defaultDatabaseAccessForm));
			string result = "";
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				if (cd.result.GetType() == typeof(Exception))
				{
					yield return cd.result;
					yield break;	
				}
				else
					result = cd.result as string;
			}
			// result = result.StartAfter(NetworkManager.DEBUG_INDICATOR);
			// Debug.Log(result);
			// result = result.Remove(result.LastIndexOf(NetworkManager.DEBUG_INDICATOR));
			yield return result;
		}

		public virtual void TryToSetNewAccountUsername ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().TryToSetNewAccountUsername ();
				return;
			}
			StartCoroutine(TryToSetNewAccountUsernameRoutine ());
		}

		public virtual IEnumerator TryToSetNewAccountUsernameRoutine ()
		{
			GameManager.GetSingleton<VirtualKeyboard>().DisableInput ();
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(true);
			GameManager.GetSingleton<NetworkManager>().notificationText.text = "Loading...";
			string username = GameManager.GetSingleton<VirtualKeyboard>().outputToInputField.text;
			newAccountData.username = username;
			username = username.Replace(" ", "");
			if (username.Length == 0)
			{
				GameManager.GetSingleton<NetworkManager>().notificationText.text = "The username must contain at least one non-space character";
				GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				yield break;
			}
			CoroutineWithData cd = new CoroutineWithData(this, GetInfoRoutine ());
			string result = "";
			Exception exception;
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				exception = cd.result as Exception;
				if (exception != null)
				{
					GameManager.GetSingleton<NetworkManager>().notificationText.text = exception.Message;
					GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
					yield break;	
				}
				else
					result = cd.result as string;
			}
			if (result.Contains("username: " + newAccountData.username))
			{
				GameManager.GetSingleton<NetworkManager>().notificationText.text = "The username can't be used. It has already been registered online by someone else.";
				GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				yield break;
			}
			cd = new CoroutineWithData(this, AddNewPlayerRoutine ());
			result = "";
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				exception = cd.result as Exception;
				if (exception != null)
				{
					GameManager.GetSingleton<NetworkManager>().notificationText.text = exception.Message;
					GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
					yield break;	
				}
				else
					result = cd.result as string;
			}
			// Debug.Log(result);
			// result = result.StartAfter(NetworkManager.DEBUG_INDICATOR);
			// Debug.Log(result);
			// result = result.Remove(result.LastIndexOf(NetworkManager.DEBUG_INDICATOR));
			// Debug.Log(result);
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().trs.parent.gameObject.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
			if (result.Contains("Player added"))
			{
				// GameManager.GetSingleton<VirtualKeyboard>().trs.parent.parent.GetChild(1).gameObject.SetActive(true);
				localAccountsData[LocalAccountNames.Length].username = newAccountData.username;
				foreach (AccountData accountData in localAccountsData)
					accountData.UpdateData ();
				LocalAccountNames = LocalAccountNames.Add_class(newAccountData.username);
				UpdateMenus ();
			}
			else
			{
				GameManager.GetSingleton<NetworkManager>().notificationText.text = "Player wasn't added. Error: Unknown";
				GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
			}
		}

		public virtual void TryToSetNewAccountPassword ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().TryToSetNewAccountPassword ();
				return;
			}
			StartCoroutine(TryToSetNewAccountPasswordRoutine ());
		}

		public virtual IEnumerator TryToSetNewAccountPasswordRoutine ()
		{
			GameManager.GetSingleton<VirtualKeyboard>().DisableInput ();
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(true);
			GameManager.GetSingleton<NetworkManager>().notificationText.text = "Loading...";
			newAccountData.password = GameManager.GetSingleton<VirtualKeyboard>().outputToInputField.text;
			CoroutineWithData cd = new CoroutineWithData(this, GetInfoRoutine ());
			string result = "";
			Exception exception;
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				exception = cd.result as Exception;
				if (exception != null)
				{
					GameManager.GetSingleton<NetworkManager>().notificationText.text = exception.Message;
					GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
					yield break;	
				}
				else
					result = cd.result as string;
			}
			if (result.Contains("username: " + newAccountData.username))
			{
				GameManager.GetSingleton<NetworkManager>().notificationText.text = "The username previously chosen can't be used. It has already been registered online by someone else.";
				GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
				GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
				yield break;
			}
			cd = new CoroutineWithData(this, AddNewPlayerRoutine ());
			result = "";
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				exception = cd.result as Exception;
				if (exception != null)
				{
					GameManager.GetSingleton<NetworkManager>().notificationText.text = exception.Message;
					GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
					yield break;	
				}
				else
					result = cd.result as string;
			}
			// Debug.Log(result);
			// result = result.StartAfter(NetworkManager.DEBUG_INDICATOR);
			// Debug.Log(result);
			// result = result.Remove(result.LastIndexOf(NetworkManager.DEBUG_INDICATOR));
			// Debug.Log(result);
			GameManager.GetSingleton<NetworkManager>().notificationTextObject.obj.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().trs.parent.gameObject.SetActive(false);
			GameManager.GetSingleton<VirtualKeyboard>().EnableInput ();
			if (result.Contains("Player added"))
			{
				localAccountsData[LocalAccountNames.Length].username = newAccountData.username;
				localAccountsData[LocalAccountNames.Length].password = newAccountData.password;
				foreach (AccountData accountData in localAccountsData)
					accountData.UpdateData ();
				LocalAccountNames = LocalAccountNames.Add_class(newAccountData.username);
				LocalAccountPasswords = LocalAccountPasswords.Add_class(newAccountData.password);
				UpdateMenus ();
			}
			else
			{
				GameManager.GetSingleton<NetworkManager>().notificationText.text = "Player wasn't added. Error: Unknown";
				GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
			}
		}

		public virtual IEnumerator AddNewPlayerRoutine ()
		{
			WWWForm form = NetworkManager.defaultDatabaseAccessForm;
			form.AddField("username", newAccountData.username);
			form.AddField("password", newAccountData.password);
			CoroutineWithData cd = new CoroutineWithData(this, GameManager.GetSingleton<NetworkManager>().PostFormToResource ("AddPlayer.php", form));
			string result = "";
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				if (cd.result.GetType() == typeof(Exception))
				{
					yield return cd.result;
					yield break;	
				}
				else
					result = cd.result as string;
			}
			// result = result.StartAfter(NetworkManager.DEBUG_INDICATOR);
			// result = result.Remove(result.LastIndexOf(NetworkManager.DEBUG_INDICATOR));
			yield return result;
		}

		public virtual void DeleteAccount ()
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().DeleteAccount ();
				return;
			}
			StartCoroutine(DeleteAccountRoutine ());
		}

		public virtual IEnumerator DeleteAccountRoutine ()
		{
			WWWForm form = NetworkManager.defaultDatabaseAccessForm;
			form.AddField("username", currentAccountToDelete.username);
			CoroutineWithData cd = new CoroutineWithData(this, GameManager.GetSingleton<NetworkManager>().PostFormToResource ("RemovePlayer.php", form));
			string result = "";
			Exception exception;
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				exception = cd.result as Exception;
				if (exception != null)
				{
					GameManager.GetSingleton<NetworkManager>().notificationText.text = exception.Message;
					GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
					yield break;	
				}
				else
					result = cd.result as string;
			}
			// result = result.StartAfter(NetworkManager.DEBUG_INDICATOR);
			// Debug.Log(result);
			// result = result.Remove(result.LastIndexOf(NetworkManager.DEBUG_INDICATOR));
			if (result.Contains("Player removed"))
			{
				int indexOfAccount = localAccountsData.IndexOf_class(currentAccountToDelete);
				foreach (string key in SaveAndLoadManager.data.Keys)
				{
					if (key.IndexOf(localAccountsData[indexOfAccount].username + VALUE_SEPARATOR) == 0)
						SaveAndLoadManager.data.Remove(key);
				}
				localAccountsData[indexOfAccount].Reset ();
				LocalAccountNames = LocalAccountNames.RemoveAt_class(indexOfAccount);
				// LocalAccountPasswords = LocalAccountPasswords.RemoveAt_class(indexOfAccount);
				foreach (AccountData accountData in localAccountsData)
					accountData.UpdateData ();
				UpdateMenus ();
			}
			else
			{
				GameManager.GetSingleton<NetworkManager>().notificationText.text = "Player wasn't removed. Error: Unknown";
				GameManager.GetSingleton<GameManager>().StartCoroutine(GameManager.GetSingleton<NetworkManager>().notificationTextObject.DisplayRoutine ());
			}
		}

		public virtual void UpdateAccountData (AccountData accountData)
		{
			if (GameManager.GetSingleton<ArchivesManager>() != this)
			{
				GameManager.GetSingleton<ArchivesManager>().UpdateAccountData (accountData);
				return;
			}
			StopAllCoroutines();
			StartCoroutine(UpdateAccountDataRoutine (accountData));
		}

		public virtual IEnumerator UpdateAccountDataRoutine (AccountData accountData)
		{
			WWWForm form = NetworkManager.defaultDatabaseAccessForm;
			form.AddField("username", accountData.username);
			form.AddField("data", SaveAndLoadManager.Serialize(accountData, typeof(AccountData)));
			CoroutineWithData cd = new CoroutineWithData(this, GameManager.GetSingleton<NetworkManager>().PostFormToResource ("UpdatePlayer.php", form));
			string result = "";
			Exception exception;
			while (string.IsNullOrEmpty(result))
			{
				yield return cd.coroutine;
				exception = cd.result as Exception;
				if (exception != null)
				{
					Debug.Log(exception.Message);
					yield break;
				}
				else
					result = cd.result as string;
			}
			if (result.Contains("Player updated"))
			{
			}
		}
	}
}