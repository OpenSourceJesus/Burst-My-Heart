using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

namespace BMH
{
	public class SoundEffect : Spawnable
	{
		public AudioSource audio;

		public virtual void OnDisable ()
		{
			AudioManager.soundEffects = AudioManager.soundEffects.Remove(this);
		}

		[Serializable]
		public class Settings
		{
			public float volume = 1;
			public float pitch = 1;

			public Settings ()
			{
			}

			public Settings (float volume, float pitch)
			{
				this.volume = volume;
				this.pitch = pitch;
			}
		}
	}
}