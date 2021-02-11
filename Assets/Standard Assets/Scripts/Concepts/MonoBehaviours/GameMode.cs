using UnityEngine;
using Extensions;

namespace BMH
{
	// //[ExecuteAlways]
	[DisallowMultipleComponent]
	public class GameMode : SingletonMonoBehaviour<GameMode>, IUpdatable
	{
		public GameObject pauseInstructionsObj;
		public bool HasPaused
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Has paused", false);
			}
			set
			{
				SaveAndLoadManager.SetValue("Has paused", value);
				if (value && pauseInstructionsObj != null)
					pauseInstructionsObj.SetActive(false);
			}
		}
		public Event[] eventPrefabs = new Event[0];
		public bool PauseWhileUnfocused
		{
			get
			{
				return OnlineBattle.localPlayer == null;
			}
		}

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
			Player.players = FindObjectsOfType<Player>();
			pauseInstructionsObj.SetActive(!HasPaused);
			enabled = false;
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual Event MakeEvent (Event eventPrefab, Vector2 spawnPosition = new Vector2(), float spawnRotation = 0)
		{
			return Instantiate(eventPrefab, spawnPosition, Quaternion.LookRotation(Vector3.forward, VectorExtensions.FromFacingAngle(spawnRotation)));
		}

		public virtual void DoUpdate ()
		{
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}
	}
}