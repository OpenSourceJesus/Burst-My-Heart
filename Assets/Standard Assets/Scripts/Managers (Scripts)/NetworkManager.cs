using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using CoroutineWithData = ThreadingUtilities.CoroutineWithData;
using System;
using Random = UnityEngine.Random;
using Extensions;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

namespace BMH
{
	public class NetworkManager : SingletonMonoBehaviour<NetworkManager>, IConnectionCallbacks, IOnEventCallback, IMatchmakingCallbacks
	{
		public string multiplayerVersion;
		public OnlinePlayer playerPrefab;
		public static Dictionary<uint, OnlinePlayer> playerIdsDict = new Dictionary<uint, OnlinePlayer>();
		public string websiteUri;
		public Text notificationText;
		public TemporaryTextObject notificationTextObject;
		public string serverName;
		public string serverUsername;
		public string serverPassword;
		public string databaseName;
		public const string DEBUG_INDICATOR = "﬩";
		public static bool IsOnline
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Is online", false);
			}
			set
			{
				SaveAndLoadManager.SetValue("Is online", value);
			}
		}
		public static WWWForm defaultDatabaseAccessForm;
		public const float NO_SPAWN_BORDER = 10;
		public const float MIN_SPAWN_RANGE_FROM_ENEMY = 50;
		public const byte EVENT_TYPE_COUNT = 2;
		public const uint EVENT_RADIUS = 100;
		public const float EVENT_AREA = 31415.93f;
		public const float EVENTS_FRACTION_OF_ARENA = 0.2f;
		public const uint MIN_EVENT_INTERVAL = 180;
		public const uint MAX_EVENT_INTERVAL = 360;
		public const float BODY_RADIUS = 2;
		public const int DATA_PER_EVENT = 4;
		public Timer makeEventsTimer;

		public override void Awake ()
		{
			base.Awake ();
			defaultDatabaseAccessForm = new WWWForm();
			defaultDatabaseAccessForm.AddField("serverName", serverName);
			defaultDatabaseAccessForm.AddField("serverUsername", serverUsername);
			defaultDatabaseAccessForm.AddField("serverPassword", serverPassword);
			defaultDatabaseAccessForm.AddField("databaseName", databaseName);
			PhotonNetwork.GameVersion = multiplayerVersion;
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.AddCallbackTarget(this);
		}

		public virtual void OnDestroy ()
		{
			IsOnline = false;
			makeEventsTimer.onFinished -= SendMakeEventsEvent;
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		public virtual void Connect ()
		{
			PhotonNetwork.ConnectUsingSettings();
		}

		public virtual void Disconnect ()
		{
			PhotonNetwork.Disconnect();
		}

		public virtual void OnConnected ()
		{
		}

		public virtual void OnConnectedToMaster ()
		{
			PhotonNetwork.JoinOrCreateRoom("Default", new RoomOptions(), TypedLobby.Default);
			Debug.Log("OnConnectedToMaster()");
		}

		public virtual void OnDisconnected (DisconnectCause cause)
		{
			OnDisconnected ();
		}

		public virtual void OnRegionListReceived (RegionHandler regionHandler)
		{
		}

		public virtual void OnCustomAuthenticationResponse (Dictionary<string, object> data)
		{
		}

		public virtual void OnCustomAuthenticationFailed (string debugMessage)
		{
		}

		public virtual void OnFriendListUpdate (List<FriendInfo> friendList)
		{
		}

		public virtual void OnCreatedRoom ()
		{
		}

		public virtual void OnCreateRoomFailed (short returnCode, string message)
		{
		}

		public virtual void OnJoinedRoom ()
		{
			Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
			StartCoroutine(JoinOnlineGameRoutine ());
		}

		public virtual void OnJoinRoomFailed (short returnCode, string message)
		{
			OnDisconnected ();
		}

		public virtual void OnDisconnected ()
		{
			SaveAndLoadManager.RemoveData ("Is online");
			Application.runInBackground = false;
		}

		public virtual void OnJoinRandomFailed (short returnCode, string message)
		{
		}

		public virtual void OnLeftRoom ()
		{
		}

		public virtual IEnumerator JoinOnlineGameRoutine ()
		{
			PhotonNetwork.LoadLevel("Online");
			yield return new WaitUntil(() => (PhotonNetwork.LevelLoadingProgress == 1));
			Debug.Log("ActorNumber:" + PhotonNetwork.LocalPlayer.ActorNumber);
			if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
			{
				makeEventsTimer.onFinished += SendMakeEventsEvent;
				makeEventsTimer.duration = Random.Range(MIN_EVENT_INTERVAL, MAX_EVENT_INTERVAL);
				makeEventsTimer.Reset ();
				makeEventsTimer.Start ();
				yield break;
			}
			object[] spawnPlayerData = new object[1];
			spawnPlayerData[0] = PhotonNetwork.LocalPlayer.ActorNumber - 2;
			PhotonNetwork.RaiseEvent(NetworkTags2.SpawnPlayer, spawnPlayerData, new RaiseEventOptions(), new SendOptions());
		}

		public virtual void OnSpawnPlayerEventRecieved (object[] data)
		{
			if (data.Length <= 1)
			{
				if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
				{
					float maxSpawnDistanceFromOrigin = Arena.instance.desiredRadius - NO_SPAWN_BORDER;
					Vector2 spawnPosition = new Vector2();
					Vector2 enemyMidPoint;
					bool isValidSpawnPosition = false;
					while (!isValidSpawnPosition)
					{
						spawnPosition = Random.insideUnitCircle * maxSpawnDistanceFromOrigin;
						isValidSpawnPosition = true;
						if (MIN_SPAWN_RANGE_FROM_ENEMY < maxSpawnDistanceFromOrigin)
						{
							foreach (OnlinePlayer enemy in playerIdsDict.Values)
							{
								enemyMidPoint = (enemy.body.trs.position + enemy.weapon.trs.position) / 2;
								if (Vector2.Distance(spawnPosition, enemyMidPoint) < MIN_SPAWN_RANGE_FROM_ENEMY)
								{
									isValidSpawnPosition = false;
									break;
								}
							}
						}
					}
					float spawnRotation = Random.value * 360;
					uint playerId = (uint) (int) data[0];
					SendSpawnPlayerEvent (playerId, spawnPosition, spawnRotation);
					Vector2 otherSpawnPosition;
					float otherSpawnRotation;
					foreach (OnlinePlayer player in playerIdsDict.Values)
					{
						otherSpawnPosition = (player.body.trs.position + player.weapon.trs.position) / 2;
						otherSpawnRotation = (player.weapon.trs.position - player.body.trs.position).GetFacingAngle();
						SendSpawnPlayerEvent (player.playerId, otherSpawnPosition, otherSpawnRotation, player.Score);
					}
					StartCoroutine(SpawnPlayer (playerId, spawnPosition, spawnRotation));
				}
			}
			else
			{
				uint playerId = (uint) (int) data[0];
				Vector2 spawnPosition = new Vector2((float) data[1], (float) data[2]);
				float spawnRotation = (float) data[3];
				uint score = (uint) (int) data[4];
				StartCoroutine(SpawnPlayer (playerId, spawnPosition, spawnRotation, score));
			}
		}

		public virtual void SendChangeScoreEvent (uint playerId, uint amount)
		{
			object[] changeScoreData = new object[0];
			changeScoreData[0] = (int) playerId;
			changeScoreData[1] = (int) amount;
			PhotonNetwork.RaiseEvent(NetworkTags2.ChangeScore, changeScoreData, new RaiseEventOptions(), new SendOptions());
		}

		public virtual void OnRemovePlayerEventRecieved (object[] data)
		{
			uint playerId = 0;
			if (data.Length > 0)
				playerId = (uint) (int) data[0];
			Destroy(playerIdsDict[playerId].gameObject);
			playerIdsDict.Remove(playerId);
			Arena.instance.SetSize ((uint) playerIdsDict.Count);
		}

		public virtual void OnChangeScoreEventRecieved (object[] data)
		{
		}

		public virtual void OnMakeEventsEventRecieved (object[] data)
		{
			byte eventType;
			Vector2 spawnPosition;
			float spawnRotation;
			for (int i = 0; i < data.Length; i += DATA_PER_EVENT)
			{
				eventType = (byte) data[i];
				spawnPosition = new Vector2((float) data[i + 1], (float) data[i + 2]);
				spawnRotation = (float) data[i + 3];
				GameManager.GetSingleton<OnlineBattle>().MakeEvent (GameManager.GetSingleton<OnlineBattle>().eventPrefabs[eventType], spawnPosition, spawnRotation);
			}
		}

		public virtual void OnMoveTransformEventRecieved (object[] data)
		{
			uint syncTransformId = 0;
			Vector2 position = new Vector2((float) data[0], (float) data[1]);
			if (data.Length > 2)
				syncTransformId = (uint) (int) data[2];
			SyncTransform.syncTransformDict[syncTransformId].trs.position = position;
		}

		public virtual void SendMoveTransformEvent (Vector2 position, uint syncTransformId)
		{
			object[] moveTransformData = new object[2];
			moveTransformData[0] = position.x;
			moveTransformData[1] = position.y;
			if (syncTransformId > 0)
				moveTransformData = moveTransformData.Add((int) syncTransformId);
			SendOptions sendOptions = new SendOptions();
			sendOptions.Reliability = false;
			PhotonNetwork.RaiseEvent(NetworkTags2.MoveTransform, moveTransformData, new RaiseEventOptions(), sendOptions);
		}

		public virtual void SendSpawnPlayerEvent (uint playerId, Vector2 position, float rotation, uint score = 0)
		{
			object[] spawnPlayerData = new object[5];
			spawnPlayerData[0] = (int) playerId;
			spawnPlayerData[1] = position.x;
			spawnPlayerData[2] = position.y;
			spawnPlayerData[3] = rotation;
			spawnPlayerData[4] = (int) score;
			PhotonNetwork.RaiseEvent(NetworkTags2.SpawnPlayer, spawnPlayerData, new RaiseEventOptions(), new SendOptions());
		}

		public virtual void SendRemovePlayerEvent (uint playerId)
		{
			object[] removePlayerData = new object[0];
			if (playerId > 0)
				removePlayerData = removePlayerData.Add((int) playerId);
			PhotonNetwork.RaiseEvent(NetworkTags2.RemovePlayer, removePlayerData, new RaiseEventOptions(), new SendOptions());
		}

		public virtual IEnumerator SpawnPlayer (uint playerId, Vector2 position, float rotation, uint score = 0)
		{
			if (playerIdsDict.ContainsKey(playerId))
				yield break;
			if (Arena.instance.desiredRadius * 2 > Arena.instance.trs.localScale.x)
				yield return new WaitUntil(() => (Arena.instance.trs.localScale.x == Arena.instance.desiredRadius * 2));
			OnlinePlayer player = Instantiate(playerPrefab, position, Quaternion.Euler(Vector3.forward * rotation));
			player.playerId = playerId;
			playerIdsDict.Add(playerId, player);
			if (playerId != PhotonNetwork.LocalPlayer.ActorNumber - 2)
			{
				player.enabled = false;
				GameManager.updatables = GameManager.updatables.Remove(player);
				OnlineBattle.nonLocalPlayers = OnlineBattle.nonLocalPlayers.Add(player);
			}
			else
			{
				GameManager.singletons.Remove(typeof(OnlinePlayer));
				GameManager.singletons.Add(typeof(OnlinePlayer), player);
				player.owner = GameManager.GetSingleton<GameManager>().teams[0];
				player.SetColor (player.owner.color);
				GameManager.updatables = GameManager.updatables.Add(GameManager.GetSingleton<OnlineBattle>());
				GameManager.GetSingleton<OnlineBattle>().loadingText.enabled = false;
				Application.runInBackground = true;
				if (!GameManager.GetSingleton<OnlineBattle>().HasPaused)
					GameManager.GetSingleton<OnlineBattle>().pauseInstructionsObj.SetActive(true);
			}
			player.ChangeScore (score);
			player.Init ();
			Arena.instance.SetSize ((uint) playerIdsDict.Count);
		}

		public virtual void SendMakeEventsEvent (params object[] args)
		{
			makeEventsTimer.duration = Random.Range(MIN_EVENT_INTERVAL, MAX_EVENT_INTERVAL);
			makeEventsTimer.Reset ();
			makeEventsTimer.Start ();
			int eventCount = (int) (EVENTS_FRACTION_OF_ARENA * Arena.instance.desiredRadius / EVENT_AREA);
			if (eventCount < 1)
				eventCount = 1;
			byte eventType;
			Vector2[] spawnPositions = new Vector2[eventCount];
			Vector2 spawnPosition;
			bool isValidSpawnPosition;
			float spawnRotation;
			object[] makeEventsData = new object[eventCount * DATA_PER_EVENT];
			for (int i = 0; i < eventCount; i ++)
			{
				eventType = (byte) (int) (Random.value * EVENT_TYPE_COUNT);
				while (true)
				{
					spawnPosition = Random.insideUnitCircle * (Arena.instance.desiredRadius - EVENT_RADIUS);
					isValidSpawnPosition = true;
					for (int i2 = 0; i2 < i; i2 ++)
					{
						if (Vector2.Distance(spawnPositions[i2], spawnPosition) < EVENT_RADIUS * 2 + BODY_RADIUS * 2)
						{
							isValidSpawnPosition = false;
							break;
						}
					}
					if (isValidSpawnPosition)
						break;
				}
				spawnPositions[i] = spawnPosition;
				spawnRotation = Random.value * 360;
				makeEventsData[i * DATA_PER_EVENT] = eventType;
				makeEventsData[i * DATA_PER_EVENT + 1] = spawnPosition.x;
				makeEventsData[i * DATA_PER_EVENT + 2] = spawnPosition.y;
				makeEventsData[i * DATA_PER_EVENT + 3] = spawnRotation;
			}
			PhotonNetwork.RaiseEvent(NetworkTags2.MakeEvents, makeEventsData, new RaiseEventOptions(), new SendOptions());
		}

		public virtual void OnEvent (EventData photonEvent)
		{
			byte eventCode = photonEvent.Code;
			object[] data = (object[]) photonEvent.CustomData;
			string debug = "OnEnter:" + eventCode + "; " + data.ToString(", ");
			Debug.Log(debug);
			if (eventCode == NetworkTags2.SpawnPlayer)
				OnSpawnPlayerEventRecieved (data);
			else if (eventCode == NetworkTags2.MoveTransform)
				OnMoveTransformEventRecieved (data);
			else if (eventCode == NetworkTags2.RemovePlayer)
				OnRemovePlayerEventRecieved (data);
			else if (eventCode == NetworkTags2.ChangeScore)
				OnChangeScoreEventRecieved (data);
			else if (eventCode == NetworkTags2.MakeEvents)
				OnMakeEventsEventRecieved (data);
		}

		public virtual IEnumerator PostFormToResource (string resourceName, WWWForm form)
		{
			using (UnityWebRequest webRequest = UnityWebRequest.Post(websiteUri + "/" + resourceName + "?", form))
			{
				yield return webRequest.SendWebRequest();
				if (webRequest.isHttpError || webRequest.isNetworkError)
				{
					notificationText.text = webRequest.error;
					GameManager.GetSingleton<GameManager>().StartCoroutine(notificationTextObject.DisplayRoutine ());
					yield return new Exception(notificationText.text);
					yield break;
				}
				else
				{
					yield return webRequest.downloadHandler.text;
					yield break;
				}
				webRequest.Dispose();
			}
			notificationText.text = "Unknown error";
			GameManager.GetSingleton<GameManager>().StartCoroutine(notificationTextObject.DisplayRoutine ());
			yield return new Exception(notificationText.text);
		}
	}

	public static class NetworkTags2
	{
		public static readonly byte SpawnPlayer = 0;
		public static readonly byte MoveTransform = 1;
		public static readonly byte RemovePlayer = 2;
		public static readonly byte ChangeScore = 3;
		public static readonly byte MakeEvents = 4;
	}
}
