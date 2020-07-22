using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Extensions;

namespace BMH
{
	public class TimeManager : SingletonMonoBehaviour<TimeManager>
	{
		public static float TotalGameplayDuration
		{
			get
			{
				return SaveAndLoadManager.GetValue<float>("Total Gameplay Duration", 0);
			}
			set
			{
				SaveAndLoadManager.SetValue("Total Gameplay Duration", value);
			}
		}
		public float defaultTimeScaleMultiplier;
		public float TimeScaleMultiplier
		{
			get
			{
				return SaveAndLoadManager.GetValue<float>("Time Scale", defaultTimeScaleMultiplier);
			}
			set
			{
				SaveAndLoadManager.SetValue("Time Scale", value);
			}
		}
		public static float UnscaledDeltaTime
		{
			get
			{
				if (Time.timeScale > 0)
					return Time.unscaledDeltaTime;
				return 0;
			}
		}
		public Text timerText;
		
		public override void Awake ()
		{
			base.Awake ();
			SetTimeScale (1);
		}
		
		public virtual void SetTimeScale (float timeScale)
		{
			Time.timeScale = timeScale * TimeScaleMultiplier;
			foreach (Rigidbody2D rigid in _Rigidbody2D.allInstances)
				rigid.simulated = Time.timeScale > 0;
		}
		
		public virtual void OnApplicationQuit ()
		{
			TotalGameplayDuration += Time.realtimeSinceStartup;
		}
		
		public virtual void SetTimerTextActive (bool active)
		{
			GameManager.GetSingleton<TimeManager>().timerText.gameObject.SetActive(active);
		}
	}
}