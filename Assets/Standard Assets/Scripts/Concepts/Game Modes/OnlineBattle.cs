using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.UI;

namespace BMH
{
	[ExecuteAlways]
	public class OnlineBattle : GameMode
	{
		public float bountyMultiplier = 1;
		public static OnlinePlayer[] nonLocalPlayers = new OnlinePlayer[0];
		public Text loadingText;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
			GameManager.updatables = GameManager.updatables.Remove(this);
			NetworkManager.playerIdsDict.Clear();
			SyncTransform.syncTransformDict.Clear();
		}

		public override void DoUpdate ()
		{
			NetworkManager.IsOnline = true;
			GameManager.GetSingleton<CameraScript>().trs.position = GameManager.GetSingleton<OnlinePlayer>().lengthVisualizerTrs.position.SetZ(GameManager.GetSingleton<CameraScript>().trs.position.z);
			foreach (OnlinePlayer player in nonLocalPlayers)
				player.UpdateGraphics ();
		}
	}
}
