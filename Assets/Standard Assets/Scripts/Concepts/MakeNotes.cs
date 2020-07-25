using UnityEngine;
using Extensions;
using System;
using Random = UnityEngine.Random;

namespace BMH
{
	[RequireComponent(typeof(AudioSource))]
	public class MakeNotes : SingletonMonoBehaviour<MakeNotes>, IUpdatable
	{
		public static NoteMaker[] noteMakers = new NoteMaker[0];
		public float worldDistanceBetweenNotes;
		float noteFrequency;
		Vector2 midPoint;
		public bool PauseWhileUnfocused
		{
			get
			{
				return GameManager.GetSingleton<OnlineBattle>() == null;
			}
		}
		public NoteGroup[] noteGroups = new NoteGroup[0];
		NoteGroup noteGroup;
		public float multiplyNoteFrequency;
		public AnimationCurve noteNormalizedFrequencyOverTime;
		public AnimationCurve noteVolumeOverTime;
		float timeOfLastNote;
		public AudioSource audioSource;
		float targetFrequency;
		float frequency;
		int position;
		public bool stream;
		public int sampleRate;
		public float frequencyLerpRate;

		public virtual void OnAudioRead (float[] data)
		{
			int count = 0;
			while (count < data.Length)
			{
				data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / sampleRate);
				position ++;
				count ++;
			}
		}

		public virtual void OnAudioSetPosition (int newPosition)
		{
			position = newPosition;
		}

		public override void Awake ()
		{
			base.Awake ();
			noteGroup = noteGroups[Random.Range(0, noteGroups.Length)];
			print(noteGroup.name);
			if (stream)
			{
				AudioClip audioClip = AudioClip.Create("Note", int.MaxValue, 1, sampleRate, true, OnAudioRead, OnAudioSetPosition);
				audioSource.clip = audioClip;
				audioSource.Play();
			}
			GameManager.updatables = GameManager.updatables.Add(this);
		}

		public virtual void DoUpdate ()
		{
			foreach (NoteMaker noteMaker in noteMakers)
			{
				midPoint = new Vector2();
				foreach (Transform trs in noteMaker.transforms)
					midPoint += (Vector2) trs.position;
				midPoint /= noteMaker.transforms.Length;
				if (Vector2.Distance(noteMaker.positionOfPreviousNote, midPoint) > worldDistanceBetweenNotes)
				{
					noteFrequency = GetNoteFromPosition(midPoint);
					noteFrequency = Mathf.Lerp(noteGroup.noteFrequencies[0], noteGroup.noteFrequencies[noteGroup.noteFrequencies.Length - 1], noteFrequency);
					noteFrequency = MathfExtensions.GetClosestNumber(noteFrequency, noteGroup.noteFrequencies) * multiplyNoteFrequency;
					noteMaker.positionOfPreviousNote = midPoint;
					audioSource.volume = 0;
					if (!stream)
					{
						AudioClip audioClip = AudioClip.Create("Note", (int) (sampleRate * noteNormalizedFrequencyOverTime.keys[noteNormalizedFrequencyOverTime.keys.Length - 1].time), 1, sampleRate, false, OnAudioRead, OnAudioSetPosition);
						audioSource.clip = audioClip;
						audioSource.Play();
					}
					timeOfLastNote = Time.time;
				}
			}
			audioSource.volume = noteVolumeOverTime.Evaluate(Time.time - timeOfLastNote);
			targetFrequency = noteFrequency * noteNormalizedFrequencyOverTime.Evaluate(Time.time - timeOfLastNote);
			frequency = Mathf.Lerp(frequency, targetFrequency, frequencyLerpRate * Time.deltaTime);
		}

		public virtual float GetNoteFromPosition (Vector2 position)
		{
			float output;
			Vector2 normalizedPosition = (Vector2) GameManager.GetSingleton<Area>().colorGradient2DRenderer.bounds.ToRect().ToNormalizedPosition(position);
			Color color = GameManager.GetSingleton<Area>().colorGradient2D.Evaluate(normalizedPosition);
			output = color.r + color.g + color.b;
			output /= 3;
			return output;
		}

		public virtual void OnDestroy ()
		{
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		[Serializable]
		public class NoteGroup
		{
			public string name;
			public float[] noteFrequencies = new float[0];
		}
	}
}