using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.SceneManagement;

namespace BMH
{
	public class AudioManager : SingletonMonoBehaviour<AudioManager>
	{
		public float Volume
		{
			get
			{
				return SaveAndLoadManager.GetValue<float>("Volume", 1);
			}
			set
			{
				SaveAndLoadManager.SetValue("Volume", value);
			}
		}
		public bool Mute
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>("Mute", false);
			}
			set
			{
				SaveAndLoadManager.SetValue("Mute", value);
			}
		}
		public SoundEffect soundEffectPrefab;
		public static SoundEffect[] soundEffects = new SoundEffect[0];
		public AnimationCurve soundEffectVolumeOverCount;
		public TextMesh muteToggleTextMesh;
		public Transform muteToggleTrs;

		public override void Awake ()
		{
			UpdateAudioListener ();
			UpdateMuteText ();
			if (AudioManager.Instance != null && AudioManager.Instance != this)
			{
				AudioManager.Instance.muteToggleTextMesh = muteToggleTextMesh;
				AudioManager.Instance.muteToggleTrs = muteToggleTrs;
			}
			else
			{
				base.Awake ();
				soundEffects = new SoundEffect[0];
			}
		}

		public virtual void UpdateAudioListener ()
		{
			AudioListener.pause = Mute;
			if (Mute)
				AudioListener.volume = 0;
			else
				AudioListener.volume = Volume;
		}

		public virtual void ToggleMute ()
		{
			if (AudioManager.Instance != this)
			{
				AudioManager.Instance.ToggleMute ();
				return;
			}
			Mute = !Mute;
			UpdateAudioListener ();
			UpdateMuteText ();
		}

		public virtual void UpdateMuteText ()
		{
			if (muteToggleTextMesh == null)
				return;
			if (Mute)
			{
				muteToggleTextMesh.text = "Unmute";
				muteToggleTrs.localScale = new Vector3(0.00025f, 0.00025f, 1);
			}
			else
			{
				muteToggleTextMesh.text = "Mute";
				muteToggleTrs.localScale = new Vector3(0.0004f, 0.0004f, 1);
			}
		}
		
		public virtual SoundEffect MakeSoundEffect (SoundEffect.Settings settings = null, Vector2 position = new Vector2())
		{
			if (Mute)
				return null;
			SoundEffect output = ObjectPool.Instance.SpawnComponent<SoundEffect>(soundEffectPrefab.prefabIndex, position);
			if (output == null)
				return null;
			soundEffects = soundEffects.Add(output);
			output.audio.volume = soundEffectVolumeOverCount.Evaluate(soundEffects.Length);
			if (settings != null)
			{
				output.audio.volume *= settings.volume;
				output.audio.pitch = settings.pitch;
			}
			return output;
		}
	}
}