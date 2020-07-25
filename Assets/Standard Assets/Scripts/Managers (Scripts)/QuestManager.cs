using UnityEngine;
using BMH;
using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DialogAndStory
{
	public class QuestManager : SingletonMonoBehaviour<QuestManager>
	{
		public GameObject retryQuestScreen;
		public static Quest currentQuest;
		public static Quest[] activeQuests = new Quest[0];
		public GameObject questsScreen;
		public Quest[] quests = new Quest[0];

		public virtual void Start ()
		{
			foreach (Quest quest in quests)
				quest.Init ();
		}

		public virtual void RetryCurrentQuest ()
		{
			if (GameManager.GetSingleton<QuestManager>() != this)
			{
				GameManager.GetSingleton<QuestManager>().RetryCurrentQuest ();
				return;
			}
			retryQuestScreen.SetActive(false);
			currentQuest.Begin ();
		}
		
		public virtual void StopCurrentQuest ()
		{
			if (GameManager.GetSingleton<QuestManager>() != this)
			{
				GameManager.GetSingleton<QuestManager>().StopCurrentQuest ();
				return;
			}
			currentQuest = null;
			retryQuestScreen.SetActive(false);
			// GameManager.onGameScenesLoaded -= delegate { GameManager.GetSingleton<QuestManager>().ShowRetryScreen (); };
		}

		public virtual void ShowRetryScreen ()
		{
			if (GameManager.GetSingleton<QuestManager>() != this)
			{
				GameManager.GetSingleton<QuestManager>().ShowRetryScreen ();
				return;
			}
			retryQuestScreen.SetActive(true);
		}

		public virtual void ActivateQuest (Quest quest)
		{
			quest.IsActive = true;
		}

		public virtual void CompleteQuest (Quest quest)
		{
			quest.IsComplete = true;
		}

		public virtual void FailQuest (Quest quest)
		{
			quest.OnFail ();
		}

		public virtual void ShowQuestsScreen ()
		{
			if (GameManager.GetSingleton<QuestManager>() != this)
			{
				GameManager.GetSingleton<QuestManager>().ShowQuestsScreen ();
				return;
			}
			questsScreen.SetActive(true);
		}

		public virtual void HideQuestsScreen ()
		{
			if (GameManager.GetSingleton<QuestManager>() != this)
			{
				GameManager.GetSingleton<QuestManager>().HideQuestsScreen ();
				return;
			}
			questsScreen.SetActive(false);
		}
	}
}