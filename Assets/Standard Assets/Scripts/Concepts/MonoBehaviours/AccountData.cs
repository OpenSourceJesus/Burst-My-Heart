using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
using Random = UnityEngine.Random;
using System.Reflection;
// using Utf8Json;

namespace BMH
{
	//[ExecuteAlways]
	public class AccountData : MonoBehaviour, ISavableAndLoadable
	{
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		public int uniqueId;
		public int UniqueId
		{
			get
			{
				return uniqueId;
			}
			set
			{
				uniqueId = value;
			}
		}
		[SaveAndLoadValue(false)]
		public string username;
		[SaveAndLoadValue(false)]
		public string password;
		[SaveAndLoadValue(false)]
		public OfflineData offlineData = new OfflineData();
		[SaveAndLoadValue(false)]
		public OnlineData onlineData = new OnlineData();

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (uniqueId == 0)
				{
					do
					{
						uniqueId = Random.Range(int.MinValue, int.MaxValue);
					} while (GameManager.UniqueIds.Contains(uniqueId));
					GameManager.UniqueIds = GameManager.UniqueIds.Add(uniqueId);
				}
				Reset ();
				UpdateData ();
				return;
			}
#endif
		}

		public virtual void Reset ()
		{
			username = "";
			password = "";
			offlineData = new OfflineData();
			onlineData = new OnlineData();
			for (int i = 0; i < ArchivesManager.MAX_ACCOUNTS; i ++)
			{
				offlineData.battleMode.versusHuman.data[i] = new OfflineBattleDataEntry();
				offlineData.battleMode.versusHuman.noSwitchingVariant.data[i] = new OfflineBattleDataEntry();
				offlineData.battleMode.versusHuman.switchPositionsVariant.data[i] = new OfflineBattleDataEntry();
				offlineData.battleMode.versusHuman.switchControlVariant.data[i] = new OfflineBattleDataEntry();
			}
		}

		public virtual void UpdateData ()
		{
			string opponentNameString;
			for (int i = 0; i < ArchivesManager.MAX_ACCOUNTS; i ++)
			{
				offlineData.battleMode.versusHuman.data[i].accountName = username;
				offlineData.battleMode.versusHuman.noSwitchingVariant.data[i].accountName = username;
				offlineData.battleMode.versusHuman.switchPositionsVariant.data[i].accountName = username;
				offlineData.battleMode.versusHuman.switchControlVariant.data[i].accountName = username;
				if (i == ArchivesManager.Instance.localAccountsData.IndexOf(this))
					opponentNameString = "a human";
				else
					opponentNameString = ArchivesManager.Instance.localAccountsData[i].username;
				offlineData.battleMode.versusHuman.data[i].opponentNameString = opponentNameString;
				offlineData.battleMode.versusHuman.noSwitchingVariant.data[i].opponentNameString = opponentNameString;
				offlineData.battleMode.versusHuman.switchPositionsVariant.data[i].opponentNameString = opponentNameString;
				offlineData.battleMode.versusHuman.switchControlVariant.data[i].opponentNameString = opponentNameString;
			}
			offlineData.battleMode.versusAI.data.accountName = username;
			offlineData.battleMode.versusAI.noSwitchingVariant.data.accountName = username;
			offlineData.battleMode.versusAI.switchPositionsVariant.data.accountName = username;
			offlineData.battleMode.versusAI.switchControlVariant.data.accountName = username;
			opponentNameString = "the AI";
			offlineData.battleMode.versusAI.data.opponentNameString = opponentNameString;
			offlineData.battleMode.versusAI.noSwitchingVariant.data.opponentNameString = opponentNameString;
			offlineData.battleMode.versusAI.switchPositionsVariant.data.opponentNameString = opponentNameString;
			offlineData.battleMode.versusAI.switchControlVariant.data.opponentNameString = opponentNameString;
			onlineData.accountName = username;
		}

		public override string ToString ()
		{
			return offlineData.ToString() + onlineData.ToString();
		}

		[Serializable]
		public class OfflineData
		{
			public BattleMode battleMode = new BattleMode();

			public override string ToString ()
			{
				return "-----Offline-----\n" + battleMode.ToString();
			}

			[Serializable]
			public class BattleMode
			{
				public VersusHuman versusHuman = new VersusHuman();
				public VersusAI versusAI = new VersusAI();

				public override string ToString ()
				{
					return versusHuman.ToString() + versusAI.ToString();
				}

				[Serializable]
				public class VersusHuman
				{
					public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];
					public NoSwitchingVariantVersusHuman noSwitchingVariant = new NoSwitchingVariantVersusHuman();
					public SwitchPositionsVariantVersusHuman switchPositionsVariant = new SwitchPositionsVariantVersusHuman();
					public SwitchControlVariantVersusHuman switchControlVariant = new SwitchControlVariantVersusHuman();

					public override string ToString ()
					{
						string output = "---Versus Human\n";
						foreach (OfflineBattleDataEntry dataPiece in data)
							output += dataPiece.ToString();
						output += noSwitchingVariant.ToString() + switchPositionsVariant.ToString() + switchControlVariant.ToString();
						return output;
					}

					[Serializable]
					public class NoSwitchingVariantVersusHuman
					{
						public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];

						public override string ToString ()
						{
							string output = "- No Switching\n";
							foreach (OfflineBattleDataEntry dataPiece in data)
								output += dataPiece.ToString();
							return output;
						}
					}

					[Serializable]
					public class SwitchPositionsVariantVersusHuman
					{
						public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];

						public override string ToString ()
						{
							string output = "- Switch Positions\n";
							foreach (OfflineBattleDataEntry dataPiece in data)
								output += dataPiece.ToString();
							return output;
						}
					}

					[Serializable]
					public class SwitchControlVariantVersusHuman
					{
						public OfflineBattleDataEntry[] data = new OfflineBattleDataEntry[ArchivesManager.MAX_ACCOUNTS];

						public override string ToString ()
						{
							string output = "- Switch Control\n";
							foreach (OfflineBattleDataEntry dataPiece in data)
								output += dataPiece.ToString();
							return output;
						}
					}
				}

				[Serializable]
				public class VersusAI
				{
					public OfflineBattleDataEntry data = new OfflineBattleDataEntry();
					public NoSwitchingVariantVersusAI noSwitchingVariant = new NoSwitchingVariantVersusAI();
					public SwitchPositionsVariantVersusAI switchPositionsVariant = new SwitchPositionsVariantVersusAI();
					public SwitchControlVariantVersusAI switchControlVariant = new SwitchControlVariantVersusAI();

					public override string ToString ()
					{
						return "---Versus AI\n" + data.ToString() + noSwitchingVariant.ToString() + switchPositionsVariant.ToString() + switchControlVariant.ToString();
					}

					[Serializable]
					public class NoSwitchingVariantVersusAI
					{
						public OfflineBattleDataEntry data = new OfflineBattleDataEntry();

						public override string ToString ()
						{
							return "- No Switching\n" + data.ToString();
						}
					}

					[Serializable]
					public class SwitchPositionsVariantVersusAI
					{
						public OfflineBattleDataEntry data = new OfflineBattleDataEntry();

						public override string ToString ()
						{
							return "- Switch Positions\n" + data.ToString();
						}
					}

					[Serializable]
					public class SwitchControlVariantVersusAI
					{
						public OfflineBattleDataEntry data = new OfflineBattleDataEntry();

						public override string ToString ()
						{
							return "- Switch Control\n" + data.ToString();
						}
					}
				}
			}
		}

		[Serializable]
		public class OnlineData
		{
			public string accountName;
			public uint kills;
			public uint deaths;
			public float killDeathRatio;
			public uint highscore;

			public override string ToString ()
			{
				string output = "-----Online-----\n";
				output += accountName + " has " + kills + " kills and " + deaths + " deaths.";
				output += " This means that " + accountName + " has a kill-death-ratio of " + killDeathRatio + ".";
				output += " The highscore of " + accountName + " is " + highscore + ".";
				return output;
			}
		}

		[Serializable]
		public class OfflineBattleDataEntry
		{
			public string opponentNameString;
			public string accountName;
			public uint totalFairKills;
			public uint totalFairDeaths;
			public float fairKillDeathRatio;
			public uint maxSpeedDisadvantage;

			public override string ToString ()
			{
				string output = "";
				if (!string.IsNullOrEmpty(opponentNameString))
				{
					output += accountName + " has killed " + opponentNameString + " " + totalFairKills + " times fairly (their move speeds were equal)";
					output += " and " + accountName + " has been killed by " + opponentNameString + " " + totalFairDeaths + " times fairly.";
					output += " This means that in fair fights against " + opponentNameString + ", " + accountName + " has a kill-death-ratio of " + fairKillDeathRatio + ".";
					output += " The highest number of speed disadvantages applied simultaneously to " + accountName + " in unfair fights against " + opponentNameString + " is " + maxSpeedDisadvantage + ".\n";
				}
				return output;
			}
		}
	}
}