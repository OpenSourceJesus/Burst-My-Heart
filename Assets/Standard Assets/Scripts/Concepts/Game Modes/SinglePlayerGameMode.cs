using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Extensions;

namespace BMH
{
	// [ExecuteAlways]
	[DisallowMultipleComponent]
    public class SinglePlayerGameMode : GameMode
    {
    	[HideInInspector]
		public float score;
		public Text scoreText;
		public uint Highscore
		{
			get
			{
				return SaveAndLoadManager.GetValue<uint>(ArchivesManager.ActivePlayerUsername + ArchivesManager.VALUE_SEPARATOR + SceneManager.GetActiveScene().name + " score (" + Player.CanSwitchPositions + ", " + Player.CanSwitchControl + ")", 0);
			}
			set
			{
				SaveAndLoadManager.SetValue(ArchivesManager.ActivePlayerUsername + ArchivesManager.VALUE_SEPARATOR + SceneManager.GetActiveScene().name + " score (" + Player.CanSwitchPositions + ", " + Player.CanSwitchControl + ")", value);
			}
		}

		public virtual void AddScore (float amount)
		{
			SetScore (score + amount);
		}

		public virtual void SetScore (float amount)
		{
			score = amount;
			scoreText.text = "" + (uint) score;
			if ((uint) score > Highscore)
			{
				Highscore = (uint) score;
				if (ArchivesManager.activeAccountData != null)
					GameManager.GetSingleton<ArchivesManager>().UpdateAccountData (ArchivesManager.activeAccountData);
			}
		}
    }
}
