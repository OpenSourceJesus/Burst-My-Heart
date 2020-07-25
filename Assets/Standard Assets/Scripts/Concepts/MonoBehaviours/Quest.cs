﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	public class Quest : MonoBehaviour
	{
		public TMP_Text nameText;
		public string description;
		public TMP_Text descriptionText;
		public Quest[] activateQuestsOnComplete;
		public Timer failTimer;
		public bool IsActive
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>(name + " active", false);
			}
			set
			{
				if (!IsActive && value)
					OnActivate ();
				SaveAndLoadManager.SetValue(name + " active", value);
			}
		}
		public bool IsComplete
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>(name + " complete", false);
			}
			set
			{
				if (!IsComplete && value)
					OnComplete ();
				SaveAndLoadManager.SetValue(name + " complete", value);
			}
		}
		public bool CanBegin
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>(name + " can begin", false);
			}
			set
			{
				SaveAndLoadManager.SetValue(name + " can begin", value);
			}
		}
		public int moneyReward;
		public int skipsReward;
		public SpawnPoint startPoint;
		public Button beginButton;

		public virtual void Start ()
		{
			failTimer.onFinished += OnFail;
		}

		public virtual void Init ()
		{
			nameText.text = name;
			descriptionText.text = description;
			gameObject.SetActive(false);
		}

		public virtual void OnComplete ()
		{
			foreach (Quest quest in activateQuestsOnComplete)
			{
				if (!quest.IsComplete)
					quest.IsActive = true;
			}
			// GameManager.GetSingleton<Player>().AddMoney (moneyReward);
			// SkipManager.Skips += skipsReward;
			IsActive = false;
			GameManager.GetSingleton<GameManager>().DeactivateGoForever (gameObject);
			GameManager.GetSingleton<SaveAndLoadManager>().Save ();
		}

		public virtual void OnFail (params object[] args)
		{
			// GameManager.GetSingleton<QuestManager>().ShowRetryScreen ();
		}

		public virtual void OnDestroy ()
		{
			failTimer.onFinished -= OnFail;
		}

		public virtual void OnActivate ()
		{
			// QuestManager.activeQuests = QuestManager.activeQuests.Add(this);
			GameManager.GetSingleton<GameManager>().ActivateGoForever (gameObject);
		}

		public virtual void Begin ()
		{
			if (!CanBegin)
				return;
			// if (startPoint != null)
				// GameManager.GetSingleton<Player>().trs.position = GameManager.GetSingleton<Player>().unrotatedColliderRectOnOrigin.AnchorToPoint(startPoint.trs.position, startPoint.anchorPoint).center;
			failTimer.Reset ();
			failTimer.Start ();
			// QuestManager.currentQuest = this;
		}
	}
}