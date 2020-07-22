using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	public class SyncTransform : MonoBehaviour, IUpdatable
	{
		public Transform trs;
		Vector2 syncedPosition;
		public float moveAmountTilSync;
		public OnlinePlayer owner;
		public uint id;
		public static Dictionary<uint, SyncTransform> syncTransformDict = new Dictionary<uint, SyncTransform>();
		public bool PauseWhileUnfocused
		{
			get
			{
				return false;
			}
		}

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				return;
			}
#endif
			syncedPosition = trs.position;
		}

		public virtual void Init ()
		{
			syncTransformDict.Add(id, this);
		}

		public virtual void DoUpdate ()
		{
			if (Vector2.Distance(syncedPosition, trs.position) > moveAmountTilSync)
			{
				syncedPosition = trs.position;
				GameManager.GetSingleton<NetworkManager>().SendMoveTransformEvent (trs.position, id);
			}
		}
	}
}