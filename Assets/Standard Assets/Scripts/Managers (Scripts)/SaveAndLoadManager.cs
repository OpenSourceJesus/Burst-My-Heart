using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System.Reflection;
using UnityEngine.UI;
using System.IO;
using System;
using Utf8Json;

namespace BMH
{
	//[ExecuteAlways]
	public class SaveAndLoadManager : MonoBehaviour
	{
		[HideInInspector]
		public SaveAndLoadObject[] saveAndLoadObjects = new SaveAndLoadObject[0];
		public static SavedObjectEntry[] savedObjectEntries = new SavedObjectEntry[0];
		public static Dictionary<string, SaveAndLoadObject> saveAndLoadObjectTypeDict = new Dictionary<string, SaveAndLoadObject>();
		public TemporaryDisplayObject displayOnSave;
		public Text saveText;
		public string saveFileFullPath;
		public static Dictionary<string, string> data = new Dictionary<string, string>();
		public const string saveFileName = "Saved Data.txt";
		public const string DATA_SEPARATOR = "☒";
		public bool usePlayerPrefs;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				saveAndLoadObjects = FindObjectsOfType<SaveAndLoadObject>();
				for (int i = 0; i < saveAndLoadObjects.Length; i ++)
				{
					if (!saveAndLoadObjects[i].enabled)
					{
						saveAndLoadObjects = saveAndLoadObjects.RemoveAt(i);
						i --;
					}
				}
				return;
			}
			Debug.Log(Application.persistentDataPath);
#endif
			if (saveFileFullPath.IndexOf(Path.DirectorySeparatorChar) == 0)
				saveFileFullPath = Application.persistentDataPath + saveFileFullPath;
			else
				saveFileFullPath = Application.persistentDataPath + Path.DirectorySeparatorChar + saveFileFullPath;
			Directory.CreateDirectory(saveFileFullPath);
			if (saveFileFullPath.LastIndexOf(Path.DirectorySeparatorChar) == saveFileFullPath.Length - 1)
				saveFileFullPath += saveFileName;
			else
				saveFileFullPath += Path.DirectorySeparatorChar + saveFileName;
			if (!File.Exists(saveFileFullPath))
			{
				StreamWriter writer = File.CreateText(saveFileFullPath);
				writer.Close();
				writer.Dispose();
				return;
			}
		}

		public static string Serialize (object value, Type type)
		{
			return JsonSerializer.NonGeneric.ToJsonString(type, value);
		}

		public static object Deserialize (string serializedState, Type type)
		{
			return JsonSerializer.NonGeneric.Deserialize(type, serializedState);
		}

		public virtual void _SetValue (string key, object value)
		{
			SetValue (key, value);
		}

		public static void SetValue (string key, object value)
		{
			if (data.ContainsKey(key))
				data[key] = Serialize(value, value.GetType());
			else
				data.Add(key, Serialize(value, value.GetType()));
#if UNITY_WEBGL
			GameManager.GetSingleton<SaveAndLoadManager>().Save ();
#endif
		}

		public static T GetValue<T> (string key, T defaultValue = default(T))
		{
			if (data.ContainsKey(key))
				return (T) Deserialize(data[key], typeof(T));
			else
				return defaultValue;
		}

		public virtual void _RemoveData (string key)
		{
			RemoveData (key);
		}

		public static void RemoveData (string key)
		{
			data.Remove(key);
		}
		
		public virtual void Save ()
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().Save ();
				return;
			}
			for (int i = 0; i < savedObjectEntries.Length; i ++)
				savedObjectEntries[i].Save ();
			string saveFileText = "";
			foreach (KeyValuePair<string, string> keyValuePair in data)
				saveFileText += keyValuePair.Key + DATA_SEPARATOR + keyValuePair.Value + DATA_SEPARATOR;
			if (usePlayerPrefs)
				PlayerPrefs.SetString(saveFileFullPath, saveFileText);
			else
				File.WriteAllText(saveFileFullPath, saveFileText);
			StartCoroutine(displayOnSave.DisplayRoutine ());
		}
		
		public virtual void Load ()
		{
			if (GameManager.GetSingleton<SaveAndLoadManager>() != this)
			{
				GameManager.GetSingleton<SaveAndLoadManager>().Load ();
				return;
			}
			saveAndLoadObjectTypeDict.Clear();
			SaveAndLoadObject saveAndLoadObject;
			savedObjectEntries = new SavedObjectEntry[0];
			for (int i = 0; i < saveAndLoadObjects.Length; i ++)
			{
				saveAndLoadObject = saveAndLoadObjects[i];
				if (saveAndLoadObject != null)
				{
					saveAndLoadObject.Init ();
					savedObjectEntries = savedObjectEntries.AddRange(saveAndLoadObject.saveEntries);
				}
			}
			data.Clear();
			string saveFileText;
			if (usePlayerPrefs)
				saveFileText = PlayerPrefs.GetString(saveFileFullPath, "");
			else
				saveFileText = File.ReadAllText(saveFileFullPath);
			if (string.IsNullOrEmpty(saveFileText))
				return;
			string[] saveFileData = saveFileText.Split(new string[] { DATA_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 1; i < saveFileData.Length; i += 2)
				data.Add(saveFileData[i - 1], saveFileData[i]);
			for (int i = 0; i < savedObjectEntries.Length; i ++)
				savedObjectEntries[i].Load ();
			GameManager.GetSingleton<GameManager>().SetGosActive ();
			GameManager.GetSingleton<AudioManager>().Awake ();
			foreach (AccountData accountData in GameManager.GetSingleton<ArchivesManager>().localAccountsData)
				accountData.UpdateData ();
			GameManager.GetSingleton<ArchivesManager>().UpdateMenus ();
		}
		
		public class SavedObjectEntry
		{
			public SaveAndLoadObject saveAndLoadObject;
			public ISavableAndLoadable saveableAndLoadable;
			public MemberInfo[] members = new MemberInfo[0];
			public const string INFO_SEPARATOR = "↕";
			
			public SavedObjectEntry ()
			{
			}

			public virtual string GetKeyForMember (MemberInfo member)
			{
				return "" + saveAndLoadObject.uniqueId + INFO_SEPARATOR + saveableAndLoadable.UniqueId + INFO_SEPARATOR + member.Name;
			}
			
			public virtual void Save ()
			{
				foreach (MemberInfo member in members)
				{
					PropertyInfo property = member as PropertyInfo;
					if (property != null)
						SetValue (GetKeyForMember(member), property.GetValue(saveableAndLoadable, null));
					else
					{
						FieldInfo field = member as FieldInfo;
						if (field != null)
							SetValue (GetKeyForMember(member), field.GetValue(saveableAndLoadable));
					}
				}
			}
			
			public virtual void Load ()
			{
				string valueString = "";
				object value;
				foreach (MemberInfo member in members)
				{
					PropertyInfo property = member as PropertyInfo;
					if (property != null)
					{
						if (data.TryGetValue(GetKeyForMember(property), out valueString))
						{
							value = Deserialize(valueString, property.PropertyType);
							property.SetValue(saveableAndLoadable, value, null);
						}
					}
					else
					{
						FieldInfo field = member as FieldInfo;
						if (field != null)
						{
							if (data.TryGetValue(GetKeyForMember(field), out valueString))
							{
								value = Deserialize(valueString, field.FieldType);
								field.SetValue(saveableAndLoadable, value);
							}
						}
					}
				}
			}
		}
	}
}