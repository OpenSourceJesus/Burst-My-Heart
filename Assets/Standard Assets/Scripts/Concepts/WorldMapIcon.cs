using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

namespace BMH
{
	[ExecuteInEditMode]
	public class WorldMapIcon : MonoBehaviour
	{
		public Collider2D collider;
		public bool onlyMakeIfExplored;
		[HideInInspector]
		public RectInt cellBoundsRect;
		[HideInInspector]
		public bool isActive;
		public GameObjectEntry[] goEntries = new GameObjectEntry[0];

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (collider == null)
					collider = GetComponent<Collider2D>();
				if (goEntries.Length == 0)
					goEntries = goEntries.Add(new GameObjectEntry(gameObject));
				else
				{
					foreach (GameObjectEntry goEntry in goEntries)
					{
						if (goEntry.go != null)
							goEntry.layer = goEntry.go.layer;
					}
				}
				return;
			}
			else
			{
				cellBoundsRect = new RectInt();
				// cellBoundsRect.size = WorldMap.Instance.unexploredTilemap.WorldToCell(collider.bounds.max).ToVec2Int() - WorldMap.Instance.unexploredTilemap.WorldToCell(collider.bounds.min).ToVec2Int() + Vector2Int.one;
				// cellBoundsRect.position = new Vector2Int(WorldMap.Instance.unexploredTilemap.WorldToCell(collider.bounds.min).ToVec2Int().x, WorldMap.Instance.unexploredTilemap.WorldToCell(collider.bounds.max).ToVec2Int().y);
				if (goEntries.Length == 0)
					goEntries = goEntries.Add(new GameObjectEntry(gameObject));
				else
				{
					foreach (GameObjectEntry goEntry in goEntries)
					{
						if (goEntry.go != null)
							goEntry.layer = goEntry.go.layer;
					}
				}
			}
#endif
			WorldMap.worldMapIcons = WorldMap.worldMapIcons.Add(this);
		}

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			WorldMap.worldMapIcons = WorldMap.worldMapIcons.Remove(this);
		}

		public virtual void MakeIcon ()
		{
			isActive = true;
			foreach (GameObjectEntry goEntry in goEntries)
				goEntry.go.layer = LayerMask.NameToLayer("Map");
		}

		public virtual void DestroyIcon ()
		{
			foreach (GameObjectEntry goEntry in goEntries)
				goEntry.go.layer = goEntry.layer;
			// DO NOT UNCOMMENT THE NEXT LINE
			isActive = false;
		}

		public virtual void Highlight ()
		{
			foreach (GameObjectEntry goEntry in goEntries)
			{
				if (goEntry.highlightedIndicator != null)
					goEntry.highlightedIndicator.SetActive(true);
			}
		}

		public virtual void Unhighlight ()
		{
			foreach (GameObjectEntry goEntry in goEntries)
			{
				if (goEntry.highlightedIndicator != null)
					goEntry.highlightedIndicator.SetActive(false);
			}
		}

		[Serializable]
		public class GameObjectEntry
		{
			public GameObject go;
			public int layer;
			public GameObject highlightedIndicator;

			public GameObjectEntry (GameObject go)
			{
				this.go = go;
				layer = go.layer;
			}
		}
	}
}