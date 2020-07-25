using UnityEngine;
using PlayerIOClient;
using System.Collections.Generic;
using Extensions;
using System.Collections;
using UnityEngine.Networking;
using System;

namespace BMH
{
	public class NetworkManager : MonoBehaviour
	{
		public static Connection connection;
		public static Client client;
		public TemporaryTextObject notificationTextObject;
		public static bool IsOnline
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Is online", false);
				// return false;
			}
			set
			{
				SaveAndLoadManager.SetValue("Is online", value);
			}
		}

		public virtual void DisplayNotification (string text)
		{
			StopCoroutine(notificationTextObject.DisplayRoutine ());
			notificationTextObject.text.text = text;
			StartCoroutine(notificationTextObject.DisplayRoutine ());
		}

		public virtual void Connect (Callback<Client> onSuccess, Callback<PlayerIOError> onFail)
		{
			PlayerIO.UseSecureApiRequests = true;
			PlayerIO.UnityInit(this);
			PlayerIO.Authenticate("burst-my-heart-dyhe2gzfzukop2uttnxokw",
				"public",
				new Dictionary<string, string> {
					{ "userId", "" },
				},
				null,
				onSuccess,
				onFail
			);
		}
	}
}