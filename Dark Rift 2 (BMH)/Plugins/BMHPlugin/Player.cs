using System.Windows;
using DarkRift;
using DarkRift.Server;

namespace BMHPlugin
{
	public class Player
	{
		public ushort playerId;
		public EntityOrientation bodyOrientation;
		public EntityOrientation weaponOrientation;
		public uint score;

		public virtual void SendSpawnPlayerMessageToAll ()
		{
			Vector spawnPosition = (bodyOrientation.position + weaponOrientation.position) / 2;
			float spawnRotation = VectorExtensions.GetFacingAngle(weaponOrientation.position - bodyOrientation.position);
			SendSpawnPlayerMessageToAll (spawnPosition, spawnRotation);
		}

		public virtual void SendSpawnPlayerMessageToAll (Vector spawnPosition, float spawnRotation)
		{
			using (DarkRiftWriter spawnPlayerWriter = DarkRiftWriter.Create())
			{
				spawnPlayerWriter.Write(playerId);
				spawnPlayerWriter.Write((float) spawnPosition.X);
				spawnPlayerWriter.Write((float) spawnPosition.Y);
				spawnPlayerWriter.Write(spawnRotation);
				spawnPlayerWriter.Write(score);
				using (Message spawnPlayerMessage = Message.Create(NetworkTags.SpawnPlayer, spawnPlayerWriter))
				{
					foreach (IClient client in BMHPlugin.instance.ClientManager.GetAllClients())
						client.SendMessage(spawnPlayerMessage, SendMode.Reliable);
					spawnPlayerMessage.Dispose();
				}
				spawnPlayerWriter.Dispose();
			}
		}
	}
}
