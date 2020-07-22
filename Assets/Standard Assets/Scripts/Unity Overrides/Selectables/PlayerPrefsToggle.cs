using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.UI;

namespace BMH
{
	[RequireComponent(typeof(Toggle))]
	public class PlayerPrefsToggle : _Selectable
	{
		public Toggle toggle;
		public string playerPrefsKey;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			toggle.isOn = SaveAndLoadManager.GetValue<bool>(playerPrefsKey, toggle.isOn);
		}

#if UNITY_EDITOR
		public override void Update ()
		{
			base.Update ();
#endif
#if !UNITY_EDITOR
		public virtual void Update ()
		{
#endif
			SaveAndLoadManager.SetValue(playerPrefsKey, toggle.isOn);
		}
	}
}
