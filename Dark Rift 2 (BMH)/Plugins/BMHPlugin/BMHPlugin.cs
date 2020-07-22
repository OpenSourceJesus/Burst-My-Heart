using System;
using System.Collections.Generic;
using System.Windows;
using DarkRift.Server;
using DarkRift;
using DarkRift.Client;
using System.Threading.Tasks;

namespace BMHPlugin
{
	public class BMHPlugin : Plugin
	{
		public override bool ThreadSafe => false;
		public override Version Version => new Version(1, 0, 0);
		public const float AREA_PER_PLAYER = 3000;
		public const float NO_SPAWN_BORDER = 10;
		public const float MAX_BODY_TO_WEAPON_DIST = 15;
		public const float MIN_SPAWN_RANGE_FROM_ENEMY = 50;
		public const byte EVENT_TYPE_COUNT = 2;
		public const int EVENT_RADIUS = 100;
		public const float EVENT_AREA = 31415.93f;
		public const float EVENTS_FRACTION_OF_ARENA = 0.2f;
		public const int MIN_EVENT_INTERVAL = 180;
		public const int MAX_EVENT_INTERVAL = 360;
		public const float BODY_RADIUS = 2;
		public Dictionary<ushort, Player> playerIdsDict = new Dictionary<ushort, Player>();
		public static Random random = new Random();
		public static BMHPlugin instance;
		public float DesiredArenaRadius
		{
			get
			{
				return (float) Math.Sqrt(AREA_PER_PLAYER * playerIdsDict.Count / Math.PI);
			}
		}

		public BMHPlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
		{
			ClientManager.ClientConnected += OnClientConnected;
			ClientManager.ClientDisconnected += OnClientDisconnected;
		}

		public virtual void OnClientConnected (object sender, ClientConnectedEventArgs e)
		{
			foreach (Player player in playerIdsDict.Values)
				player.SendSpawnPlayerMessageToAll ();
			float angleToSpawnPosition;
			float maxSpawnDistanceFromOrigin;
			float spawnDistanceFromOrigin = 0;
			Vector spawnPosition = new Vector();
			Vector enemyMidPoint;
			bool isValidSpawnPosition = false;
			while (!isValidSpawnPosition)
			{
				angleToSpawnPosition = (float) random.NextDouble() * 2 * (float) Math.PI;
				maxSpawnDistanceFromOrigin = DesiredArenaRadius - NO_SPAWN_BORDER;
				spawnDistanceFromOrigin = (float) random.NextDouble() * maxSpawnDistanceFromOrigin;
				spawnPosition = new Vector(Math.Cos(angleToSpawnPosition), Math.Sin(angleToSpawnPosition));
				spawnPosition.Normalize();
				spawnPosition *= spawnDistanceFromOrigin;
				isValidSpawnPosition = true;
				if (MIN_SPAWN_RANGE_FROM_ENEMY < maxSpawnDistanceFromOrigin)
				{
					foreach (Player enemy in playerIdsDict.Values)
					{
						enemyMidPoint = (enemy.bodyOrientation.position + enemy.weaponOrientation.position) / 2;
						if (VectorExtensions.Distance(spawnPosition, enemyMidPoint) < MIN_SPAWN_RANGE_FROM_ENEMY)
						{
							isValidSpawnPosition = false;
							break;
						}
					}
				}
			}
			float spawnRotation = (float) random.NextDouble() * 360;
			Player newPlayer = new Player();
			newPlayer.playerId = e.Client.ID;
			newPlayer.bodyOrientation = new EntityOrientation(spawnPosition - VectorExtensions.FromFacingAngle(spawnRotation) * MAX_BODY_TO_WEAPON_DIST / 2, spawnRotation);
			newPlayer.weaponOrientation = new EntityOrientation(spawnPosition + VectorExtensions.FromFacingAngle(spawnRotation) * MAX_BODY_TO_WEAPON_DIST / 2, spawnRotation);
			newPlayer.SendSpawnPlayerMessageToAll (spawnPosition, spawnRotation);
			playerIdsDict.Add(newPlayer.playerId, newPlayer);
			if (playerIdsDict.Count == 1)
				Task.Run(() => MakeEventsTask ());
			e.Client.MessageReceived += OnMoveTransform;
			e.Client.MessageReceived += OnScoreChanged;
		}

		public virtual void OnClientDisconnected (object sender, ClientDisconnectedEventArgs e)
		{
			playerIdsDict.Remove(e.Client.ID);
			using (DarkRiftWriter removePlayerWriter = DarkRiftWriter.Create())
			{
				removePlayerWriter.Write(e.Client.ID);
				using (Message removePlayerMessage = Message.Create(NetworkTags.RemovePlayer, removePlayerWriter))
				{
					foreach (IClient client in ClientManager.GetAllClients())
						client.SendMessage(removePlayerMessage, SendMode.Reliable);
					removePlayerMessage.Dispose();
				}
				removePlayerWriter.Dispose();
			}
		}

		public virtual async Task MakeEventsTask ()
		{
			int delayTime;
			while (true)
			{
				delayTime = (int) (random.NextDouble() * (MAX_EVENT_INTERVAL - MIN_EVENT_INTERVAL) + MIN_EVENT_INTERVAL) * 1000;
				await Task.Delay(delayTime);
				MakeEvents ();
			}
		}

		public virtual void MakeEvents ()
		{
			int eventCount = (int) (EVENTS_FRACTION_OF_ARENA * DesiredArenaRadius / EVENT_AREA);
			if (eventCount < 1)
				eventCount = 1;
			List<Vector> otherSpawnPositions = new List<Vector>();
			Vector spawnPosition;
			bool isValidSpawnPosition;
			for (int i = 0; i < eventCount; i ++)
			{
				isValidSpawnPosition = true;
				while (true)
				{
					spawnPosition = VectorExtensions.FromFacingAngle((float) random.NextDouble() * 360) * (DesiredArenaRadius - EVENT_RADIUS) * random.NextDouble();
					foreach (Vector otherSpawnPosition in otherSpawnPositions)
					{
						if (VectorExtensions.Distance(otherSpawnPosition, spawnPosition) < EVENT_RADIUS * 2 + BODY_RADIUS * 2)
						{
							isValidSpawnPosition = false;
							break;
						}
					}
					if (isValidSpawnPosition)
						break;
				}
				otherSpawnPositions.Add(spawnPosition);
				SendMakeEventMessageToAll ((byte) (random.NextDouble() * EVENT_TYPE_COUNT), spawnPosition, (float) random.NextDouble() * 360);
			}
		}

		public virtual void OnMoveTransform (object sender, DarkRift.Server.MessageReceivedEventArgs e)
		{
			using (Message message = e.GetMessage())
			{
				using (DarkRiftReader reader = message.GetReader())
				{
					if (message.Tag == NetworkTags.MoveTransform)
					{
						//WriteEvent("OnMoveTransform " + e.Client.ID, LogType.Info);
						//if (reader.Length % 17 != 0)
						//{
						//    WriteEvent("Received malformed data packet for MoveTransform", LogType.Warning);
						//    return;
						//}

						while (reader.Position < reader.Length)
						{
							ushort id = reader.ReadUInt16();
							Vector position = new Vector(reader.ReadSingle(), reader.ReadSingle());
							if (id % 2 == 0)
								playerIdsDict[(ushort) (id / 2)].bodyOrientation.position = position;
							else
								playerIdsDict[(ushort) (id / 2)].weaponOrientation.position = position;
							using (DarkRiftWriter moveTransformWriter = DarkRiftWriter.Create())
							{
								moveTransformWriter.Write(id);
								moveTransformWriter.Write((float) position.X);
								moveTransformWriter.Write((float) position.Y);
								using (Message moveTransformMessage = Message.Create(NetworkTags.MoveTransform, moveTransformWriter))
								{
									foreach (IClient client in ClientManager.GetAllClients())
										client.SendMessage(moveTransformMessage, SendMode.Unreliable);
									moveTransformMessage.Dispose();
								}
								moveTransformWriter.Dispose();
							}
						}
					}
					reader.Dispose();
				}
				message.Dispose();
			}
		}

		public virtual void OnScoreChanged (object sender, DarkRift.Server.MessageReceivedEventArgs e)
		{
			using (Message message = e.GetMessage())
			{
				using (DarkRiftReader reader = message.GetReader())
				{
					if (message.Tag == NetworkTags.ChangeScore)
					{
						//WriteEvent("OnScoreChanged " + e.Client.ID, LogType.Info);
						//if (reader.Length % 17 != 0)
						//{
						//    WriteEvent("Received malformed data packet for ChangeScore", LogType.Warning);
						//    return;
						//}

						while (reader.Position < reader.Length)
						{
							ushort id = reader.ReadUInt16();
							ushort amount = reader.ReadUInt16();
							playerIdsDict[id].score += amount;
							using (DarkRiftWriter changeScoreWriter = DarkRiftWriter.Create())
							{
								changeScoreWriter.Write(id);
								changeScoreWriter.Write(amount);
								using (Message changeScoreMessage = Message.Create(NetworkTags.ChangeScore, changeScoreWriter))
								{
									foreach (IClient client in ClientManager.GetAllClients())
										client.SendMessage(changeScoreMessage, SendMode.Reliable);
									changeScoreMessage.Dispose();
								}
								changeScoreWriter.Dispose();
							}
						}
					}
					reader.Dispose();
				}
				message.Dispose();
			}
		}

		public virtual void SendMakeEventMessageToAll (byte eventTypeIndex, Vector position, float rotation)
		{
			using (DarkRiftWriter makeEventWriter = DarkRiftWriter.Create())
			{
				makeEventWriter.Write(eventTypeIndex);
				makeEventWriter.Write((float) position.X);
				makeEventWriter.Write((float) position.Y);
				makeEventWriter.Write(rotation);
				using (Message makeEventMessage = Message.Create(NetworkTags.MakeEvent, makeEventWriter))
				{
					foreach (IClient client in ClientManager.GetAllClients())
						client.SendMessage(makeEventMessage, SendMode.Reliable);
					makeEventMessage.Dispose();
				}
				makeEventWriter.Dispose();
			}
		}
	}

	public static class NetworkTags
	{
		public static readonly ushort SpawnPlayer = 0;
		public static readonly ushort MoveTransform = 1;
		public static readonly ushort RemovePlayer = 2;
		public static readonly ushort ChangeScore = 3;
		public static readonly ushort MakeEvent = 4;
	}
}