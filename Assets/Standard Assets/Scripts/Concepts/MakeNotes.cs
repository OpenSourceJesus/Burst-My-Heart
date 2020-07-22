using UnityEngine;
using Extensions;

namespace BMH
{
	public class MakeNotes : SingletonMonoBehaviour<MakeNotes>, IUpdatable
	{
		public static NoteMaker[] noteMakers = new NoteMaker[0];
		public float worldDistanceBetweenNotes;
		public float noteLength = 1;
		public float addToSoundDespawnDelay = -0.1f;
		float note;
		Vector2 midPoint;
		public bool PauseWhileUnfocused
		{
			get
			{
				return GameManager.GetSingleton<OnlineBattle>() == null;
			}
		}

		public override void Awake ()
		{
			base.Awake ();
			noteLength /= GameManager.GetSingleton<AudioManager>().soundEffectPrefab.audio.pitch;
			addToSoundDespawnDelay /= GameManager.GetSingleton<AudioManager>().soundEffectPrefab.audio.pitch;
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
					SoundEffect soundEffect = GameManager.GetSingleton<AudioManager>().MakeSoundEffect (null, midPoint);
					if (soundEffect == null)
						return;
					note = MathfExtensions.SnapToInterval(soundEffect.audio.clip.length / noteLength * GetNoteFromPosition(midPoint), noteLength);
					soundEffect.audio.time = note;
					GameManager.GetSingleton<ObjectPool>().DelayDespawn (soundEffect.prefabIndex, soundEffect.gameObject, soundEffect.trs, noteLength + addToSoundDespawnDelay);
					noteMaker.positionOfPreviousNote = midPoint;
				}
			}
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
	}
}