#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

namespace BMH
{
	public class SetScore : EditorScript
	{
		public int score;

		public virtual void Update ()
		{
			GameManager.GetSingleton<SinglePlayerGameMode>().SetScore (score);
		}
	}
}
#endif