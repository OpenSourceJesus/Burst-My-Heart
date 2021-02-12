using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	//[ExecuteAlways]
	[DisallowMultipleComponent]
	public class Player : SingletonMonoBehaviour<Player>, IUpdatable
	{
		[Header("Player")]
		public float autoBalanceMoveSpeedMultiplier;
		public float defaultMoveSpeedMultiplier;
		public Transform trs;
		public Body body;
		public Weapon weapon;
		public LineRenderer lineRenderer;
		public Team owner;
		public float maxBodyToWeaponDistance = 15;
		[HideInInspector]
		public float maxBodyToWeaponDistanceSqr;
		public SpriteRenderer[] teamIndicators = new SpriteRenderer[0];
		public string playerName;
		public static Player[] players = new Player[0];
		public static bool CanSwitchDimensions
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Can switch dimensions", false);
			}
			set
			{
				SaveAndLoadManager.SetValue("Can switch dimensions", value);
			}
		}
		public static bool CanSwitchPositions
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Can switch positions", false);
			}
			set
			{
				SaveAndLoadManager.SetValue("Can switch positions", value);
			}
		}
		public static bool CanSwitchControl
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Can switch control", false);
			}
			set
			{
				SaveAndLoadManager.SetValue("Can switch control", value);
			}
		}
		public static bool AutoBalanceTeams
		{

			get
			{
				return SaveAndLoadManager.GetValue<bool>("Autobalance teams", true);
			}
			set
			{
				SaveAndLoadManager.SetValue("Autobalance teams", value);
			}
		}
		public virtual uint Score
		{
			get
			{
				return (uint) SaveAndLoadManager.GetValue<int>(playerName + " score", 0);
			}
			set
			{
				SaveAndLoadManager.SetValue(playerName + " score", (int) value);
			}
		}
		public int WinsInARow
		{
			get
			{
				return SaveAndLoadManager.GetValue<int>(playerName + " wins in a row", 0);
			}
			set
			{
				SaveAndLoadManager.SetValue(playerName + " wins in a row", value);
			}
		}
		public float MoveSpeedMultiplier
		{
			get
			{
				return SaveAndLoadManager.GetValue<float>(playerName + " move speed multiplier", defaultMoveSpeedMultiplier);
			}
			set
			{
				SaveAndLoadManager.SetValue(playerName + " move speed multiplier", value);
			}
		}
		public Transform lengthVisualizerTrs;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		[HideInInspector]
		public int kills;

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (body == null)
					body = GetComponentInChildren<Body>();
				if (weapon == null)
					weapon = GetComponentInChildren<Weapon>();
				return;
			}
#endif
			GameManager.updatables = GameManager.updatables.Add(this);
			owner.representative = this;
			owner.representatives = owner.representatives.Add(this);
			SetColor (owner.color);
			maxBodyToWeaponDistance *= trs.localScale.x;
			maxBodyToWeaponDistanceSqr = maxBodyToWeaponDistance * maxBodyToWeaponDistance;
			body.moveSpeed *= MoveSpeedMultiplier * trs.localScale.x;
			body.distanceJoint.distance = maxBodyToWeaponDistance;
			weapon.moveSpeed *= MoveSpeedMultiplier * trs.localScale.x;
			weapon.distanceJoint.distance = maxBodyToWeaponDistance;
			if (lengthVisualizerTrs != null)
				lengthVisualizerTrs.SetWorldScale(Vector3.one * maxBodyToWeaponDistance / trs.localScale.x);
		}

		public virtual void SetColor (Color color)
		{
			lineRenderer.startColor = color.SetAlpha(lineRenderer.endColor.a);
			lineRenderer.endColor = color.SetAlpha(lineRenderer.endColor.a);
			foreach (SpriteRenderer teamIndicator in teamIndicators)
				teamIndicator.color = color.SetAlpha(teamIndicator.color.a);
		}

		public virtual void DoUpdate ()
		{
			UpdateGraphics ();
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			owner.representatives = owner.representatives.Remove(this);			
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void UpdateGraphics ()
		{
			lineRenderer.SetPosition(0, body.trs.position);
			lineRenderer.SetPosition(1, weapon.trs.position);
			lengthVisualizerTrs.position = (body.trs.position + weapon.trs.position) / 2;
		}

		public virtual void SwitchPositions ()
		{
			gameObject.SetActive(false);
			Vector2 previousBodyPosition = body.trs.position;
			body.trs.position = weapon.trs.position;
			weapon.trs.position = previousBodyPosition;
			gameObject.SetActive(true);
		}

		public virtual float GetFaceForwardAmount ()
		{
			return 1f - (Vector2.Angle(weapon.trs.position - body.trs.position, owner.opponent.representative.body.trs.position - weapon.trs.position) / 180);
		}
		
		public virtual float GetFaceForwardAmountAfterMove (Vector2 bodyMove, Vector2 weaponMove)
		{
			Vector2 playerBodyPos = (Vector2) body.trs.position + (Vector2.ClampMagnitude(bodyMove, 1) * body.moveSpeed * Time.deltaTime);
			Vector2 playerWeaponPos = (Vector2) weapon.trs.position + (Vector2.ClampMagnitude(weaponMove, 1) * weapon.moveSpeed * Time.deltaTime);
			return 1f - (Vector2.Angle(playerWeaponPos - playerBodyPos, (Vector2) owner.opponent.representative.body.trs.position - playerWeaponPos) / 180);
		}

		public virtual float GetFaceForwardAmountAfterSwitchPositions ()
		{
			return 1f - (Vector2.Angle(body.trs.position - weapon.trs.position, owner.opponent.representative.body.trs.position - body.trs.position) / 180);
		}

		public virtual float GetFaceForwardAmountAfterOpponentSwitchPositions ()
		{
			return 1f - (Vector2.Angle(weapon.trs.position - body.trs.position, owner.opponent.representative.weapon.trs.position - weapon.trs.position) / 180);
		}

		public virtual float GetFaceForwardAmountAfterBothSwitchPositions ()
		{
			return 1f - (Vector2.Angle(body.trs.position - weapon.trs.position, owner.opponent.representative.weapon.trs.position - body.trs.position) / 180);
		}

		public virtual float GetWinDistance ()
		{
			return Vector2.Distance(weapon.trs.position, owner.opponent.representative.body.trs.position);
		}

		public virtual float GetWinDistanceAfterMove (Vector2 weaponMove)
		{
			Vector2 weaponPos = (Vector2) weapon.trs.position + (Vector2.ClampMagnitude(weaponMove, 1) * weapon.moveSpeed * Time.deltaTime);
			return Vector2.Distance(weaponPos, owner.opponent.representative.body.trs.position);
		}

		public virtual float GetWinDistanceAfterSwitchPositions ()
		{
			return Vector2.Distance(body.trs.position, owner.opponent.representative.body.trs.position);
		}

		public virtual float GetWinDistanceAfterOpponentSwitchPositions ()
		{
			return Vector2.Distance(weapon.trs.position, owner.opponent.representative.weapon.trs.position);
		}

		public virtual float GetWinDistanceAfterBothSwitchPositions ()
		{
			return Vector2.Distance(body.trs.position, owner.opponent.representative.weapon.trs.position);
		}

		public virtual float GetLoseDistance ()
		{
			return Vector2.Distance(owner.opponent.representative.weapon.trs.position, body.trs.position);
		}

		public virtual float GetLoseDistanceAfterMove (Vector2 bodyMove)
		{
			Vector2 bodyPos = (Vector2) body.trs.position + (Vector2.ClampMagnitude(bodyMove, 1) * body.moveSpeed * Time.deltaTime);
			return Vector2.Distance(owner.opponent.representative.weapon.trs.position, bodyPos);
		}

		public virtual void ChangeWinsInARow (bool win)
		{
			if (win)
			{
				WinsInARow = Mathf.Clamp(WinsInARow + 1, 1, int.MaxValue);
				kills ++;
			}
			else
				WinsInARow = Mathf.Clamp(WinsInARow - 1, int.MinValue, -1);
			int playerNumber = GameManager.Instance.teams.IndexOf(owner);
			AccountData accountData = null;
			AccountData opponentAccountData = null;
			if (playerNumber == 0)
			{
				accountData = ArchivesManager.player1AccountData;
				opponentAccountData = ArchivesManager.player2AccountData;
			}
			else if (AI.Instance == null)
			{
				accountData = ArchivesManager.player2AccountData;
				opponentAccountData = ArchivesManager.player1AccountData;
			}
			else
				return;
			int indexOfAccountData = ArchivesManager.Instance.localAccountsData.IndexOf(accountData);
			int indexOfOpponentAccountData = ArchivesManager.Instance.localAccountsData.IndexOf(opponentAccountData);
			if (accountData != null)
			{
				if (AI.Instance != null)
				{
					if (MoveSpeedMultiplier == owner.opponent.representative.MoveSpeedMultiplier)
					{
						if (win)
							accountData.offlineData.battleMode.versusAI.data.totalFairKills ++;
						else
							accountData.offlineData.battleMode.versusAI.data.totalFairDeaths --;
						accountData.offlineData.battleMode.versusAI.data.fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusAI.data.totalFairKills / accountData.offlineData.battleMode.versusAI.data.totalFairDeaths;
						if (!CanSwitchPositions && !CanSwitchControl)
						{
							if (win)
								accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.totalFairKills ++;
							else
								accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.totalFairDeaths --;
							accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.totalFairKills / accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.totalFairDeaths;
						}
						else if (CanSwitchPositions)
						{
							if (win)
								accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.totalFairKills ++;
							else
								accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.totalFairDeaths --;
							accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.totalFairKills / accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.totalFairDeaths;
						}
						else
						{
							if (win)
								accountData.offlineData.battleMode.versusAI.switchControlVariant.data.totalFairKills ++;
							else
								accountData.offlineData.battleMode.versusAI.switchControlVariant.data.totalFairDeaths --;
							accountData.offlineData.battleMode.versusAI.switchControlVariant.data.fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusAI.switchControlVariant.data.totalFairKills / accountData.offlineData.battleMode.versusAI.switchControlVariant.data.totalFairDeaths;
						}
					}
					else
					{
						int speedDisadvantage = (int) ((owner.opponent.representative.MoveSpeedMultiplier - MoveSpeedMultiplier) / autoBalanceMoveSpeedMultiplier);
						if (speedDisadvantage > accountData.offlineData.battleMode.versusAI.data.maxSpeedDisadvantage)
							accountData.offlineData.battleMode.versusAI.data.maxSpeedDisadvantage ++;
						if (!CanSwitchPositions && !CanSwitchControl)
						{
							if (speedDisadvantage > accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusAI.noSwitchingVariant.data.maxSpeedDisadvantage ++;
						}
						else if (CanSwitchPositions)
						{
							if (speedDisadvantage > accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusAI.switchPositionsVariant.data.maxSpeedDisadvantage ++;
						}
						else
						{
							if (speedDisadvantage > accountData.offlineData.battleMode.versusAI.switchControlVariant.data.maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusAI.switchControlVariant.data.maxSpeedDisadvantage ++;
						}
					}
				}
				else
				{
					if (MoveSpeedMultiplier == owner.opponent.representative.MoveSpeedMultiplier)
					{
						if (win)
						{
							accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].totalFairKills ++;
							accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].totalFairKills ++;
						}
						else
						{
							accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].totalFairDeaths --;
							accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].totalFairDeaths --;
						}
						accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].totalFairDeaths;
						accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].totalFairDeaths;
						if (!CanSwitchPositions && !CanSwitchControl)
						{
							if (win)
							{
								accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].totalFairKills ++;
								accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].totalFairKills ++;
							}
							else
							{
								accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].totalFairDeaths --;
								accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].totalFairDeaths --;
							}
							accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].totalFairDeaths;
							accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].totalFairDeaths;
						}
						else if (CanSwitchPositions)
						{
							if (win)
							{
								accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfAccountData].totalFairKills ++;
								accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].totalFairKills ++;
							}
							else
							{
								accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].totalFairDeaths --;
							}
							accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfAccountData].totalFairDeaths;
							accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].totalFairDeaths;
						}
						else
						{
							if (win)
							{
								accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].totalFairKills ++;
								accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].totalFairKills ++;
							}
							else
							{
								accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].totalFairDeaths --;
								accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].totalFairDeaths --;
							}
							accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].totalFairDeaths;
							accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].fairKillDeathRatio = (float) accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].totalFairKills / accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].totalFairDeaths;
						}
					}
					else
					{
						int speedDisadvantage = (int) ((owner.opponent.representative.MoveSpeedMultiplier - MoveSpeedMultiplier) / autoBalanceMoveSpeedMultiplier);
						if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].maxSpeedDisadvantage)
							accountData.offlineData.battleMode.versusHuman.data[indexOfAccountData].maxSpeedDisadvantage ++;
						if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].maxSpeedDisadvantage)
							accountData.offlineData.battleMode.versusHuman.data[indexOfOpponentAccountData].maxSpeedDisadvantage ++;
						if (!CanSwitchPositions && !CanSwitchControl)
						{
							if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfAccountData].maxSpeedDisadvantage ++;
							if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusHuman.noSwitchingVariant.data[indexOfOpponentAccountData].maxSpeedDisadvantage ++;
						}
						else if (CanSwitchPositions)
						{
							if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfAccountData].maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfAccountData].maxSpeedDisadvantage ++;
							if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusHuman.switchPositionsVariant.data[indexOfOpponentAccountData].maxSpeedDisadvantage ++;
						}
						else
						{
							if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfAccountData].maxSpeedDisadvantage ++;
							if (speedDisadvantage > accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].maxSpeedDisadvantage)
								accountData.offlineData.battleMode.versusHuman.switchControlVariant.data[indexOfOpponentAccountData].maxSpeedDisadvantage ++;
						}
					}
				}
				ArchivesManager.Instance.UpdateAccountData (accountData);
			}
		}
	}
}