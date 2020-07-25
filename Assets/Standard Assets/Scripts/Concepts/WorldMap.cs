using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Extensions;
#if UNITY_EDITOR
using UnityEditor;
#endif
// using DialogAndStory;

namespace BMH
{
	[ExecuteInEditMode]
	public class WorldMap : MonoBehaviour
	{
		// public Tilemap[] tilemaps = new Tilemap[0];
		// public Tilemap unexploredTilemap;
		Vector2Int cellPositions;
		public static HashSet<Vector2Int> exploredCellPositions = new HashSet<Vector2Int>();
		public static Vector2[] ExploredCellPositions
		{
			get
			{
				return SaveAndLoadManager.GetValue<Vector2[]>("Explored cells", new Vector2[0]);
			}
			set
			{
				SaveAndLoadManager.SetValue("Explored cells", value);
			}
		}
		HashSet<Vector2Int> exploredCellPositionsAtLastTimeOpened = new HashSet<Vector2Int>();
		Vector2Int cellPosition;
		Vector2Int minExploredCellPosition;
		Vector2Int maxExploredCellPosition;
		[HideInInspector]
		public Vector2Int minCellPosition;
		[HideInInspector]
		public Vector2Int maxCellPosition;
		Vector2Int previousMinCellPosition;
		public static WorldMapIcon[] worldMapIcons = new WorldMapIcon[0];
		public static bool isOpen;
		bool canControlCamera;
		public float cameraMoveSpeed;
		Vector2 moveInput;
		public float normalizedScreenBorder;
		Rect screenWithoutBorder;
		Obelisk fastTravelToObelisk;
		// public TileBase unexploredTile;
		public WorldMapCamera worldMapCamera;
		public static bool playerIsAtObelisk;
		public static bool playerJustFastTraveled;
#if UNITY_EDITOR
		public bool update;
		public bool startOver;
		public int x;
		public int y;
#endif

#if UNITY_EDITOR
		public virtual void Start ()
		{
			if (!Application.isPlaying)
			{
				EditorApplication.update += DoEditorUpdate;
				return;
			}
			else
				EditorApplication.update -= DoEditorUpdate;
		}

		public virtual void OnDestroy ()
		{
			if (Application.isPlaying)
				return;
			EditorApplication.update -= DoEditorUpdate;
		}

		public virtual void DoEditorUpdate ()
		{
			if (update)
			{
				if (startOver)
				{
					GameManager.GetSingleton<World>().update = true;
					GameManager.GetSingleton<World>().Update ();
					x = GameManager.GetSingleton<World>().cellBoundsRect.xMin;
					y = GameManager.GetSingleton<World>().cellBoundsRect.yMin;
					startOver = false;
				}
				// 	unexploredTilemap.SetTile(new Vector3Int(x, y, 0), unexploredTile);
				if (x > GameManager.GetSingleton<World>().cellBoundsRect.xMax + 1)
				{
					x = GameManager.GetSingleton<World>().cellBoundsRect.xMin;
					y ++;
					if (y > GameManager.GetSingleton<World>().cellBoundsRect.yMax + 1)
						update = false;
				}
				else
					x ++;
			}
		}
#endif

		public virtual void Init ()
		{
			// minCellPosition = unexploredTilemap.WorldToCell(GameManager.GetSingleton<GameCamera>().viewRect.min).ToVec2Int();
			// maxCellPosition = unexploredTilemap.WorldToCell(GameManager.GetSingleton<GameCamera>().viewRect.max).ToVec2Int();
			exploredCellPositions.Clear();
			Vector2Int cellPosition;
// #if UNITY_EDITOR
// 			foreach (Vector2 _cellPosition in ExploredCellPositions)
// 			{
// 				cellPosition = _cellPosition.ToVec2Int();
// 				exploredCellPositions.Add(cellPosition);
// 				minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, cellPosition);
// 				maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, cellPosition);
// 			}
// 			for (int x = minCellPosition.x; x <= maxCellPosition.x; x ++)
// 			{
// 				for (int y = minCellPosition.y; y <= maxCellPosition.y; y ++)
// 				{
// 					cellPosition = new Vector2Int(x, y);
// 					if (!exploredCellPositions.Contains(cellPosition))
// 					{
// 						exploredCellPositions.Add(cellPosition);
// 						minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, cellPosition);
// 						maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, cellPosition);
// 					}
// 				}
// 			}
// #endif
// #if !UNITY_EDITOR
			if (ExploredCellPositions.Length == 0)
			{
				for (int x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					for (int y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
						minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, cellPosition);
						maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, cellPosition);
					}
				}
			}
			else
			{
				foreach (Vector2 _cellPosition in ExploredCellPositions)
				{
					cellPosition = _cellPosition.ToVec2Int();
					exploredCellPositions.Add(cellPosition);
					minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, cellPosition);
					maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, cellPosition);
				}
				ExploredCellPositions = ExploredCellPositions;
			}
// #endif
			previousMinCellPosition = minCellPosition;
			screenWithoutBorder = new Rect();
			screenWithoutBorder.size = new Vector2(Screen.width - normalizedScreenBorder * Screen.width, Screen.height - normalizedScreenBorder * Screen.height);
			screenWithoutBorder.center = new Vector2(Screen.width / 2, Screen.height / 2);
			worldMapCamera.HandleViewSize ();
		}
		
		public virtual void DoUpdate ()
		{
			if (!isOpen)
				UpdateExplored ();
			else
			{
				worldMapCamera.DoUpdate ();
				moveInput = InputManager.GetAxis2D("Move Horizontal", "Move Vertical");// + InputManager.GetAxis2D("Aim Horizontal", "Aim Vertical");
				GameManager.activeCursorEntry.rectTrs.gameObject.SetActive(true);
				if (InputManager.UsingGamepad)
				{
					GameManager.activeCursorEntry.rectTrs.position += (Vector3) moveInput * GameManager.cursorMoveSpeed * Time.unscaledDeltaTime;
					GameManager.activeCursorEntry.rectTrs.position = GameManager.activeCursorEntry.rectTrs.position.ClampComponents(Vector3.zero, new Vector2(Screen.width, Screen.height));
				}
				if (!screenWithoutBorder.Contains(GameManager.activeCursorEntry.rectTrs.position))
				{
					moveInput = (Vector2) GameManager.activeCursorEntry.rectTrs.position - new Vector2(Screen.width / 2, Screen.height / 2);
					moveInput /= new Vector2(Screen.width / 2, Screen.height / 2).magnitude;
					if (canControlCamera)
						worldMapCamera.trs.position += (Vector3) moveInput * cameraMoveSpeed * Time.unscaledDeltaTime;
					if (GameManager.activeCursorEntry.name != "Arrow")
					{
						GameManager.cursorEntriesDict["Arrow"].SetAsActive ();
						GameManager.activeCursorEntry.rectTrs.position = GameManager.cursorEntriesDict["Default"].rectTrs.position;
					}
					GameManager.activeCursorEntry.rectTrs.up = moveInput;
					// if (GameManager.GetSingleton<GameManager>().worldMapTutorialConversation.currentDialog == GameManager.GetSingleton<GameManager>().worldMapMoveViewTutorialDialog)
					// 	GameManager.GetSingleton<DialogManager>().EndDialog (GameManager.GetSingleton<GameManager>().worldMapMoveViewTutorialDialog);
				}
				else if (GameManager.activeCursorEntry.name != "Default")
				{
					GameManager.cursorEntriesDict["Default"].SetAsActive ();
					GameManager.activeCursorEntry.rectTrs.position = GameManager.cursorEntriesDict["Arrow"].rectTrs.position;
				}
				if (WorldMap.playerIsAtObelisk)
					HandleFastTravel ();
			}
		}

		public virtual void HandleFastTravel ()
		{
			foreach (Obelisk obelisk in Obelisk.instances)
			{
				if (obelisk.Found)
				{
					if (fastTravelToObelisk != obelisk && obelisk.worldMapIcon.collider.bounds.ToRect().Contains(worldMapCamera.camera.ScreenToWorldPoint(GameManager.activeCursorEntry.rectTrs.position)))
					{
						if (fastTravelToObelisk != null)
							fastTravelToObelisk.worldMapIcon.Unhighlight ();
						fastTravelToObelisk = obelisk;
						fastTravelToObelisk.worldMapIcon.Highlight ();
						break;
					}
				}
			}
			if (fastTravelToObelisk != null && InputManager.GetButtonDown("Interact"))
			{
				WorldMap.playerJustFastTraveled = true;
				GameManager.GetSingleton<HumanPlayer>().BodyPosition = fastTravelToObelisk.worldMapIcon.collider.bounds.center;
				GameManager.GetSingleton<HumanPlayer>().WeaponPosition = fastTravelToObelisk.worldMapIcon.collider.bounds.center;
				GameManager.GetSingleton<SaveAndLoadManager>().Save ();
				Close ();
				isOpen = false;
				GameManager.GetSingleton<GameManager>().ReloadActiveScene ();
			}
		}

		public virtual IEnumerator OpenRoutine ()
		{
			canControlCamera = false;
			isOpen = true;
			GameManager.GetSingleton<GameManager>().PauseGame (1000000);
			foreach (WorldMapIcon worldMapIcon in worldMapIcons)
			{
				foreach (Vector2Int position in worldMapIcon.cellBoundsRect.allPositionsWithin)
				{
					if (!worldMapIcon.onlyMakeIfExplored || exploredCellPositions.Contains(position))
						worldMapIcon.MakeIcon ();
				}
			}
			// Tilemap worldMapTilemap;
			// Tilemap worldTilemap;
			// TileBase tile;
			Vector2Int cellPosition;
			// unexploredTilemap.gameObject.SetActive(true);
			HashSet<Vector2Int> exploredCellPositionsSinceLastTimeOpened = new HashSet<Vector2Int>();
			foreach (Vector2Int exploredCellPosition in exploredCellPositions)
				exploredCellPositionsSinceLastTimeOpened.Add(exploredCellPosition);
			foreach (Vector2Int exploredCellPositionAtLastTimeOpened in exploredCellPositionsAtLastTimeOpened)
				exploredCellPositionsSinceLastTimeOpened.Remove(exploredCellPositionAtLastTimeOpened);
			// for (int i = 0; i < tilemaps.Length; i ++)
			// {
			// 	worldTilemap = GameManager.GetSingleton<World>().tilemaps[i];
			// 	worldMapTilemap = tilemaps[i];
			// 	worldMapTilemap.gameObject.SetActive(true);
			// 	foreach (Vector2Int exploredCellPositionSinceLastTimeOpened in exploredCellPositionsSinceLastTimeOpened)
			// 	{
			// 		tile = worldTilemap.GetTile(exploredCellPositionSinceLastTimeOpened.ToVec3Int());
			// 		if (tile != null)
			// 		{
			// 			worldMapTilemap.SetTile(exploredCellPositionSinceLastTimeOpened.ToVec3Int(), tile);
			// 			worldMapTilemap.SetTransformMatrix(exploredCellPositionSinceLastTimeOpened.ToVec3Int(), worldTilemap.GetTransformMatrix(exploredCellPositionSinceLastTimeOpened.ToVec3Int()));
			// 		}
			// 		else
			// 			unexploredTilemap.SetTile(exploredCellPositionSinceLastTimeOpened.ToVec3Int(), null);
			// 	}
			// }
			worldMapCamera.trs.position = GameManager.GetSingleton<Player>().trs.position.SetZ(worldMapCamera.trs.position.z);
			worldMapCamera.gameObject.SetActive(true);
			// if (GameManager.GetSingleton<GameManager>().worldMapTutorialConversation.gameObject.activeSelf && GameManager.GetSingleton<GameManager>().worldMapTutorialConversation.updateRoutine == null)
			// {
			// 	GameManager.GetSingleton<DialogManager>().StartConversation (GameManager.GetSingleton<GameManager>().worldMapTutorialConversation);
			// 	foreach (Dialog dialog in GameManager.GetSingleton<GameManager>().worldMapTutorialConversation.dialogs)
			// 		dialog.canvas.worldCamera = worldMapCamera.camera;
			// }
			if (InputManager.UsingGamepad)
			{
				GameManager.cursorEntriesDict["Default"].SetAsActive ();
				GameManager.activeCursorEntry.rectTrs.localPosition = Vector2.zero;
			}
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			canControlCamera = true;
		}

		public virtual void Open ()
		{
			if (GameManager.GetSingleton<WorldMap>() != this)
			{
				GameManager.GetSingleton<WorldMap>().Open ();
				return;
			}
			StopAllCoroutines();
			StartCoroutine(OpenRoutine ());
		}

		public virtual void Close ()
		{
			if (GameManager.GetSingleton<WorldMap>() != this)
			{
				GameManager.GetSingleton<WorldMap>().Close ();
				return;
			}
			isOpen = false;
			exploredCellPositionsAtLastTimeOpened.Clear();
			foreach (Vector2Int exploredCellPosition in exploredCellPositions)
				exploredCellPositionsAtLastTimeOpened.Add(exploredCellPosition);
			foreach (WorldMapIcon worldMapIcon in worldMapIcons)
			{
				worldMapIcon.DestroyIcon ();
				worldMapIcon.Unhighlight ();
			}
			// Tilemap worldMapTilemap;
			// for (int i = 0; i < tilemaps.Length; i ++)
			// {
			// 	worldMapTilemap = tilemaps[i];
			// 	worldMapTilemap.gameObject.SetActive(false);
			// }
			worldMapCamera.gameObject.SetActive(false);
			// if (!GameManager.GetSingleton<PauseMenu>().gameObject.activeSelf)
				GameManager.GetSingleton<GameManager>().PauseGame (-1000000);
			if (!InputManager.UsingGamepad)
				GameManager.cursorEntriesDict["Default"].SetAsActive ();
			else
				GameManager.activeCursorEntry.rectTrs.gameObject.SetActive(false);
		}
		
		public virtual void UpdateExplored ()
		{
			int x;
			int y;
			// minCellPosition = unexploredTilemap.WorldToCell(GameManager.GetSingleton<GameCamera>().viewRect.min).ToVec2Int();
			// maxCellPosition = unexploredTilemap.WorldToCell(GameManager.GetSingleton<GameCamera>().viewRect.max).ToVec2Int();
			// if (TeleportArrow.justTeleported)
			// {
			// 	TeleportArrow.justTeleported = false;
			// 	minExploredCellPosition = VectorExtensions.SetToMinComponents(minExploredCellPosition, minCellPosition);
			// 	maxExploredCellPosition = VectorExtensions.SetToMaxComponents(maxExploredCellPosition, maxCellPosition);
			// 	for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
			// 	{
			// 		for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
			// 		{
			// 			cellPosition = new Vector2Int(x, y);
			// 			exploredCellPositions.Add(cellPosition);
			// 		}
			// 	}
			// 	previousMinCellPosition = minCellPosition;
			// 	return;
			// }
			if (minCellPosition.x > previousMinCellPosition.x)
			{
				x = maxCellPosition.x;
				if (maxCellPosition.x > maxExploredCellPosition.x)
					maxExploredCellPosition.x = maxCellPosition.x;
				for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.y > previousMinCellPosition.y)
				{
					y = maxCellPosition.y;
					if (maxCellPosition.y > maxExploredCellPosition.y)
						maxExploredCellPosition.y = maxCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.y < previousMinCellPosition.y)
				{
					y = minCellPosition.y;
					if (minCellPosition.y < minExploredCellPosition.y)
						minExploredCellPosition.y = minCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			else if (minCellPosition.x < previousMinCellPosition.x)
			{
				x = minCellPosition.x;
				if (minCellPosition.x < minExploredCellPosition.x)
					minExploredCellPosition.x = minCellPosition.x;
				for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.y > previousMinCellPosition.y)
				{
					y = maxCellPosition.y;
					if (maxCellPosition.y > maxExploredCellPosition.y)
						maxExploredCellPosition.y = maxCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.y < previousMinCellPosition.y)
				{
					y = minCellPosition.y;
					if (minCellPosition.y < minExploredCellPosition.y)
						minExploredCellPosition.y = minCellPosition.y;
					for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			else if (minCellPosition.y > previousMinCellPosition.y)
			{
				y = maxCellPosition.y;
				if (maxCellPosition.y > maxExploredCellPosition.y)
					maxExploredCellPosition.y = maxCellPosition.y;
				for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.x > previousMinCellPosition.x)
				{
					x = maxCellPosition.x;
					if (maxCellPosition.x > maxExploredCellPosition.x)
						maxExploredCellPosition.x = maxCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.x < previousMinCellPosition.x)
				{
					x = minCellPosition.x;
					if (minCellPosition.x < minExploredCellPosition.x)
						minExploredCellPosition.x = minCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			else if (minCellPosition.y < previousMinCellPosition.y)
			{
				y = minCellPosition.y;
				if (minCellPosition.y < minExploredCellPosition.y)
					minExploredCellPosition.y = minCellPosition.y;
				for (x = minCellPosition.x; x <= maxCellPosition.x; x ++)
				{
					cellPosition = new Vector2Int(x, y);
					exploredCellPositions.Add(cellPosition);
				}
				if (minCellPosition.x > previousMinCellPosition.x)
				{
					x = maxCellPosition.x;
					if (maxCellPosition.x > maxExploredCellPosition.x)
						maxExploredCellPosition.x = maxCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
				else if (minCellPosition.x < previousMinCellPosition.x)
				{
					x = minCellPosition.x;
					if (minCellPosition.x < minExploredCellPosition.x)
						minExploredCellPosition.x = minCellPosition.x;
					for (y = minCellPosition.y; y <= maxCellPosition.y; y ++)
					{
						cellPosition = new Vector2Int(x, y);
						exploredCellPositions.Add(cellPosition);
					}
				}
			}
			previousMinCellPosition = minCellPosition;
		}
	}
}