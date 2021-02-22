using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using Extensions;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BMH
{
	public class GameManager : SingletonMonoBehaviour<GameManager>, ISavableAndLoadable
	{
		public static bool paused;
		public GameObject[] registeredGos = new GameObject[0];
		[SaveAndLoadValue]
		static string enabledGosString = "";
		[SaveAndLoadValue]
		static string disabledGosString = "";
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
		public const string STRING_SEPERATOR = "|";
		public float timeScale;
		public static int currentPauses;
		public Team[] teams;
		public static IUpdatable[] updatables = new IUpdatable[0];
		public static IUpdatable[] pausedUpdatables = new IUpdatable[0];
		public static Dictionary<Type, object> singletons = new Dictionary<Type, object>();
		public static bool doEditorUpdates = false;
		public const char UNIQUE_ID_SEPERATOR = ',';
#if UNITY_EDITOR
		public static int[] UniqueIds
		{
			get
			{
				int[] output = new int[0];
				string[] uniqueIdsString = EditorPrefs.GetString("Unique ids").Split(UNIQUE_ID_SEPERATOR);
				int uniqueIdParsed;
				foreach (string uniqueIdString in uniqueIdsString)
				{
					if (int.TryParse(uniqueIdString, out uniqueIdParsed))
						output = output.Add(uniqueIdParsed);
				}
				return output;
			}
			set
			{
				string uniqueIdString = "";
				foreach (int uniqueId in value)
					uniqueIdString += uniqueId + UNIQUE_ID_SEPERATOR;
				EditorPrefs.SetString("Unique ids", uniqueIdString);
			}
		}
#endif
		public static int framesSinceLoadedScene;
		public const int LAG_FRAMES_AFTER_LOAD_SCENE = 2;
		public static float UnscaledDeltaTime
		{
			get
			{
				if (paused || framesSinceLoadedScene <= LAG_FRAMES_AFTER_LOAD_SCENE)
					return 0;
				else
					return Time.unscaledDeltaTime;
			}
		}
		public static bool initialized;
		public CursorEntry[] cursorEntries = new CursorEntry[0];
		public static Dictionary<string, CursorEntry> cursorEntriesDict = new Dictionary<string, CursorEntry>();
		public static CursorEntry activeCursorEntry;
		public static float cursorMoveSpeed = 0.05f;
		public static Vector2Int windowSize;
		public Color firstDimensionColor;
		public Color secondDimensionColor;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Transform[] transforms = FindObjectsOfType<Transform>();
				IIdentifiable[] identifiables = new IIdentifiable[0];
				foreach (Transform trs in transforms)
				{
					identifiables = trs.GetComponents<IIdentifiable>();
					foreach (IIdentifiable identifiable in identifiables)
					{
						if (!UniqueIds.Contains(identifiable.UniqueId))
							UniqueIds = UniqueIds.Add(identifiable.UniqueId);
					}
				}
				return;
			}
#endif
			if (!initialized)
			{
				windowSize = new Vector2Int(Screen.width, Screen.height);
				ClearPlayerStats ();
				SaveAndLoadManager.RemoveData ("Has paused");
				initialized = true;
			}
			if (cursorEntries.Length > 0)
			{
				activeCursorEntry = null;
				cursorEntriesDict.Clear();
				foreach (CursorEntry cursorEntry in cursorEntries)
				{
					cursorEntriesDict.Add(cursorEntry.name, cursorEntry);
					cursorEntry.rectTrs.gameObject.SetActive(false);
				}
				Cursor.visible = false;
				cursorEntriesDict["Default"].SetAsActive ();
			}
			base.Awake ();
		}

		public virtual void Update ()
		{
			// try
			// {
				// if (!paused && framesSinceLoadedScene > LAG_FRAMES_AFTER_LOAD_SCENE)
					Physics2D.Simulate(Time.deltaTime);
				foreach (IUpdatable updatable in updatables)
					updatable.DoUpdate ();
				if (GetSingleton<ObjectPool>() != null && GetSingleton<ObjectPool>().enabled)
					GetSingleton<ObjectPool>().DoUpdate ();
				if (GetSingleton<GameCamera>() != null)
					GetSingleton<GameCamera>().DoUpdate ();
				framesSinceLoadedScene ++;
			// }
			// catch (Exception e)
			// {
			// 	Debug.Log(e.Message + "\n" + e.StackTrace);
			// }
		}

		public virtual void LoadScene (string name)
		{
			if (GetSingleton<GameManager>() != this)
			{
				GetSingleton<GameManager>().LoadScene (name);
				return;
			}
			if (name == "Title")
				ClearPlayerStats ();
			Boss.initialized = false;
			framesSinceLoadedScene = 0;
			SceneManager.LoadScene(name);
		}

		public virtual void ClearPlayerStats ()
		{
			SaveAndLoadManager.RemoveData ("1 score");
			SaveAndLoadManager.RemoveData ("2 score");
			SaveAndLoadManager.RemoveData ("1 wins in a row");
			SaveAndLoadManager.RemoveData ("2 wins in a row");
			SaveAndLoadManager.RemoveData ("1 move speed multiplier");
			SaveAndLoadManager.RemoveData ("2 move speed multiplier");
		}

		public virtual void LoadScene (int index)
		{
			LoadScene (SceneManager.GetSceneByBuildIndex(index).name);
		}

		public virtual void ReloadActiveScene ()
		{
			LoadScene (SceneManager.GetActiveScene().name);
		}

		public virtual void PauseGame (int pauses)
		{
			currentPauses = Mathf.Clamp(currentPauses + pauses, 0, int.MaxValue);
			paused = currentPauses > 0;
			Time.timeScale = timeScale * (1 - paused.GetHashCode());
			AudioListener.pause = paused;
			if (paused)
				AudioListener.volume = 0;
			else
				GetSingleton<AudioManager>().UpdateAudioListener ();
		}

		public virtual void Quit ()
		{
			Application.Quit();
		}

		public virtual void OnApplicationQuit ()
		{
			ClearPlayerStats ();
			SaveAndLoadManager.RemoveData ("Has paused");
			SaveAndLoadManager.RemoveData ("Is online");
			GetSingleton<SaveAndLoadManager>().Save ();
		}

		public virtual void OnApplicationFocus (bool isFocused)
		{
			if (isFocused)
			{
				foreach (IUpdatable pausedUpdatable in pausedUpdatables)
					updatables = updatables.Add(pausedUpdatable);
				pausedUpdatables = new IUpdatable[0];
				foreach (Timer runningTimer in Timer.runningInstances)
					runningTimer.pauseIfCan = false;
				foreach (TemporaryDisplayObject displayedObject in TemporaryDisplayObject.displayedInstances)
					StartCoroutine(displayedObject.DisplayRoutine ());
			}
			else
			{
				IUpdatable updatable;
				for (int i = 0; i < updatables.Length; i ++)
				{
					updatable = updatables[i];
					if (updatable.PauseWhileUnfocused)
					{
						pausedUpdatables = pausedUpdatables.Add(updatable);
						updatables = updatables.RemoveAt(i);
						i --;
					}
				}
				foreach (Timer runningTimer in Timer.runningInstances)
					runningTimer.pauseIfCan = true;
				foreach (TemporaryDisplayObject displayedObject in TemporaryDisplayObject.displayedInstances)
					StopCoroutine(displayedObject.DisplayRoutine ());
			}
		}

		public virtual void SetGosActive ()
		{
			if (GetSingleton<GameManager>() != this)
			{
				GetSingleton<GameManager>().SetGosActive ();
				return;
			}
			string[] stringSeperators = { STRING_SEPERATOR };
			string[] enabledGos = enabledGosString.Split(stringSeperators, StringSplitOptions.None);
			foreach (string goName in enabledGos)
			{
				for (int i = 0; i < registeredGos.Length; i ++)
				{
					if (goName == registeredGos[i].name)
					{
						registeredGos[i].SetActive(true);
						break;
					}
				}
			}
			string[] disabledGos = disabledGosString.Split(stringSeperators, StringSplitOptions.None);
			foreach (string goName in disabledGos)
			{
				GameObject go = GameObject.Find(goName);
				if (go != null)
					go.SetActive(false);
			}
		}
		
		public virtual void ActivateGoForever (GameObject go)
		{
			go.SetActive(true);
			ActivateGoForever (go.name);
		}
		
		public virtual void DeactivateGoForever (GameObject go)
		{
			go.SetActive(false);
			DeactivateGoForever (go.name);
		}
		
		public virtual void ActivateGoForever (string goName)
		{
			disabledGosString = disabledGosString.Replace(STRING_SEPERATOR + goName, "");
			if (!enabledGosString.Contains(goName))
				enabledGosString += STRING_SEPERATOR + goName;
		}
		
		public virtual void DeactivateGoForever (string goName)
		{
			enabledGosString = enabledGosString.Replace(STRING_SEPERATOR + goName, "");
			if (!disabledGosString.Contains(goName))
				disabledGosString += STRING_SEPERATOR + goName;
		}

		public static float ClampAngle (float ang, float min, float max)
		{
			ang = WrapAngle(ang);
			min = WrapAngle(min);
			max = WrapAngle(max);
			float minDist = Mathf.Min(Mathf.DeltaAngle(ang, min), Mathf.DeltaAngle(ang, max));
			if (WrapAngle(ang + Mathf.DeltaAngle(ang, minDist)) == min)
				return min;
			else if (WrapAngle(ang + Mathf.DeltaAngle(ang, minDist)) == max)
				return max;
			else
				return ang;
		}

		public static float WrapAngle (float ang)
		{
			if (ang < 0)
				ang += 360;
			else if (ang > 360)
				ang = 360 - ang;
			return ang;
		}

		public static void SetGameObjectActive (string name)
		{
			GameObject.Find(name).SetActive(true);
		}

		public static void SetGameObjectInactive (string name)
		{
			GameObject.Find(name).SetActive(false);
		}

		public virtual void OnDestroy ()
		{
			StopAllCoroutines();
			if (GetSingleton<GameManager>() == this)
				OnApplicationQuit ();
		}

		public static void _Debug (object o)
		{
			Debug.LogError(o);
		}

		public static Object Clone (Object obj)
		{
			return Instantiate(obj);
		}

		public static Object Clone (Object obj, Vector3 position, Quaternion rotation)
		{
			return Instantiate(obj, position, rotation);
		}

		public static T GetSingleton<T> ()
		{
			object obj = null;
			if (!singletons.TryGetValue(typeof(T), out obj))
				obj = GetSingleton<T>(FindObjectsOfType<Object>());
			return (T) obj;
		}

		public static T GetSingleton<T> (Object[] objects)
		{
			if (typeof(T).IsSubclassOf(typeof(Object)))
			{
				for (int i = 0; i < objects.Length; i ++)
				{
					Object obj = objects[i];
					if (obj is T)
					{
						if (singletons.ContainsKey(typeof(T)))
							singletons[typeof(T)] = obj;
						else
							singletons.Add(typeof(T), obj);
						return (T) (object) obj;
					}
				}
			}
			return (T) (object) null;
		}

		public static T GetSingletonIncludingAssets<T> ()
		{
			object obj = null;
			if (!singletons.TryGetValue(typeof(T), out obj))
				obj = GetSingletonIncludingAssets<T>(Object.FindObjectsOfTypeIncludingAssets(typeof(T)));
			return (T) obj;
		}

		public static T GetSingletonIncludingAssets<T> (Object[] objects)
		{
			if (typeof(T).IsSubclassOf(typeof(Object)))
			{
				for (int i = 0; i < objects.Length; i ++)
				{
					Object obj = objects[i];
					if (obj is T)
					{
						if (singletons.ContainsKey(typeof(T)))
							singletons[typeof(T)] = obj;
						else
							singletons.Add(typeof(T), obj);
						return (T) (object) obj;
					}
				}
			}
			return (T) (object) null;
		}

		[Serializable]
		public class CursorEntry
		{
			public string name;
			public RectTransform rectTrs;

			public virtual void SetAsActive ()
			{
				if (activeCursorEntry != null)
					activeCursorEntry.rectTrs.gameObject.SetActive(false);
				rectTrs.gameObject.SetActive(true);
				activeCursorEntry = this;
			}
		}
	}
}
