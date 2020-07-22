using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	[ExecuteAlways]
	public class Conversation : MonoBehaviour
	{
		public Transform trs;
		public Dialog[] dialogs;
		public Coroutine updateRoutine;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				return;
			}
#endif
			if (dialogs != null)
			{
				foreach (Transform child in trs)
				{
					if (child.gameObject.activeSelf)
						child.gameObject.SetActive(false);
					else
						Destroy(child.gameObject);
				}
			}
		}
		
		public virtual IEnumerator UpdateRoutine ()
		{
			if (DialogManager.currentConversation != null)
				yield return DialogManager.currentConversation.updateRoutine;
			DialogManager.currentConversation = this;
			foreach (Dialog dialog in dialogs)
			{
				GameManager.GetSingleton<DialogManager>().StartDialog (dialog);
				yield return new WaitUntil(() => (!dialog.IsActive));
				GameManager.GetSingleton<DialogManager>().EndDialog (dialog);
			}
			yield break;
		}
		
		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			StopAllCoroutines ();
			foreach (Dialog dialog in dialogs)
				dialog.gameObject.SetActive(true);
		}
		
#if UNITY_EDITOR
		public virtual void Update ()
		{
			if (Application.isPlaying)
				return;
			dialogs = GetComponentsInChildren<Dialog>();
			foreach (Dialog dialog in dialogs)
				dialog.conversation = this;
		}
#endif
	}
}