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
				return SaveAndLoadManager.GetValue<float>("Total gameplay duration", 0);
			}
			set
			{
				SaveAndLoadManager.SetValue("Total gameplay duration", value);
			}
		}
		
		public override void Awake ()
		{
			base.Awake ();
			SetTimeScale (1);
		}
		
		public static void SetTimeScale (float timeScale)
		{
			Time.timeScale = timeScale;
			foreach (Rigidbody2D rigid in _Rigidbody2D.allInstances)
				rigid.simulated = Time.timeScale > 0;
		}
		
		public virtual void OnApplicationQuit ()
		{
			TotalGameplayDuration += Time.realtimeSinceStartup;
		}
	}
}