using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class DialogManager : SingletonMonoBehaviour<DialogManager>
	{
		public static Conversation currentConversation;

		public virtual void StartDialog (Dialog dialog)
		{
			dialog.gameObject.SetActive(true);
		}
		
		public virtual void EndDialog (Dialog dialog)
		{
			dialog.gameObject.SetActive(false);
			if (dialog.activateOnEnd != null)
				dialog.activateOnEnd.onClick.Invoke();
		}
		
		public virtual void StartConversation (Conversation conversation)
		{
			conversation.updateRoutine = conversation.StartCoroutine(conversation.UpdateRoutine ());
		}
	}
}