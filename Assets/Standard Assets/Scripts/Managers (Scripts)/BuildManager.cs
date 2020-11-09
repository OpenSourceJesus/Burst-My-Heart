using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using BMH.Analytics;
using System.IO;
using UnityEngine.UI;
#endif

namespace BMH
{
	//[ExecuteAlways]
	public class BuildManager : SingletonMonoBehaviour<BuildManager>
	{
#if UNITY_EDITOR
		public BuildAction[] buildActions;
		static BuildPlayerOptions buildOptions;
		public Text versionNumberText;
#endif
		public int versionIndex;
		public string versionNumberPrefix;
		public bool clearDataOnFirstStartup;
		public static bool IsFirstStartup
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("1st startup", true);
			}
			set
			{
				SaveAndLoadManager.SetValue("1st startup", value);
			}
		}
		
#if UNITY_EDITOR
		public static string[] GetScenePathsInBuild ()
		{
			List<string> scenePathsInBuild = new List<string>();
			string scenePath = null;
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i ++)
			{
				scenePath = EditorBuildSettings.scenes[i].path;
				if (EditorBuildSettings.scenes[i].enabled)
					scenePathsInBuild.Add(scenePath);
			}
			return scenePathsInBuild.ToArray();
		}

		public static string[] GetAllScenePaths ()
		{
			List<string> scenePathsInBuild = new List<string>();
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i ++)
				scenePathsInBuild.Add(EditorBuildSettings.scenes[i].path);
			return scenePathsInBuild.ToArray();
		}
		
		[MenuItem("Build/Make Builds")]
		public static void Build ()
		{
			BuildManager.Instance._Build ();
		}

		public virtual void _Build ()
		{
			BuildManager.Instance.versionIndex ++;
			foreach (BuildAction buildAction in buildActions)
			{
				if (buildAction.enabled)
					buildAction.Do ();
			}
		}
		
		[Serializable]
		public class BuildAction
		{
			public string name;
			public bool enabled;
			public BuildTarget target;
			public string locationPath;
			public BuildOptions[] options;
			public bool makeZip;
			public bool moveCrashHandler;
			public string directoryToZip;
			public string zipLocationPath;
			public bool clearDataOnFirstStartup;
			
			public virtual void Do ()
			{
				BuildManager.Instance.clearDataOnFirstStartup = clearDataOnFirstStartup;
				if (BuildManager.Instance.versionNumberText != null)
					BuildManager.Instance.versionNumberText.text = BuildManager.Instance.versionNumberPrefix + DateTime.Now.Date.ToString("MMdd");
				if (ConfigurationManager.Instance != null)
					ConfigurationManager.Instance.canvas.gameObject.SetActive(false);
				EditorSceneManager.MarkAllScenesDirty();
				EditorSceneManager.SaveOpenScenes();
				buildOptions = new BuildPlayerOptions();
				buildOptions.scenes = GetScenePathsInBuild();
				buildOptions.target = target;
				buildOptions.locationPathName = locationPath;
				foreach (BuildOptions option in options)
					buildOptions.options |= option;
				BuildPipeline.BuildPlayer(buildOptions);
				if (ConfigurationManager.Instance != null)
					ConfigurationManager.Instance.canvas.gameObject.SetActive(true);
				AssetDatabase.Refresh();
				if (moveCrashHandler)
				{
					string extrasPath = locationPath + "/Extras";
					string crashHandlerFileName = "UnityCrashHandler64.exe";
					if (!Directory.Exists(extrasPath))
						Directory.CreateDirectory(extrasPath);
					if (File.Exists(extrasPath + "/" + crashHandlerFileName))
						File.Delete(extrasPath + "/" + crashHandlerFileName);
					else
					{
						crashHandlerFileName = "UnityCrashHandler32.exe";
						if (File.Exists(extrasPath + "/" + crashHandlerFileName))
							File.Delete(extrasPath + "/" + crashHandlerFileName);
					}
					File.Move(locationPath + "/" + crashHandlerFileName, extrasPath + "/" + crashHandlerFileName);
				}
				if (makeZip)
				{
					File.Delete(zipLocationPath);
					DirectoryCompressionOperations.CompressDirectory (directoryToZip, zipLocationPath);
				}
			}
		}
#endif
	}
}