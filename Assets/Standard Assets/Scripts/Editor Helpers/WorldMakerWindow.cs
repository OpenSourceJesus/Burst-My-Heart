#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Extensions;
using UnityEditor.SceneManagement;
using System;

namespace BMH
{
	public class WorldMakerWindow : EditorWindow
	{
		public static WorldMakerWindow instance;
		public static bool makeTilemaps;
		public static bool makeWorldObjects;
		public static bool enableWorld;
		public static bool enablePieces;
		public static bool piecesAreActive;
		public static bool worldIsActive;
		public static bool showPieces;
		public static bool piecesAreShown;

		[MenuItem("Window/World")]
		public static void Init ()
		{
			instance = (WorldMakerWindow) EditorWindow.GetWindow(typeof(WorldMakerWindow));
			makeTilemaps = EditorPrefs.GetBool("Make tilemaps", false);
			makeWorldObjects = EditorPrefs.GetBool("Make objects", false);
			enableWorld = EditorPrefs.GetBool("Enable world", false);
			enablePieces = EditorPrefs.GetBool("Enable pieces", false);
			showPieces = EditorPrefs.GetBool("Show pieces", false);
			instance.Show();
		}

		public virtual void OnGUI ()
		{
			GUIContent guiContent = new GUIContent();
			guiContent.text = "Rebuild";
			bool rebuild = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (rebuild)
				Rebuild ();
			makeTilemaps = EditorGUILayout.Toggle("Make Tilemaps", makeTilemaps);
			EditorPrefsExtensions.SetBool("Make tilemaps", makeTilemaps);
			makeWorldObjects = EditorGUILayout.Toggle("Make Objects", makeWorldObjects);
			EditorPrefsExtensions.SetBool("Make objects", makeWorldObjects);
			guiContent.text = "Make Pieces";
			bool makePieces = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (makePieces)
				MakePieces ();
			guiContent.text = "Remove Pieces";
			bool removePieces = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (removePieces)
				RemovePieces ();
			enableWorld = EditorGUILayout.Toggle("Enable World", enableWorld);
			if (enableWorld != worldIsActive)
				SetWorldActive (enableWorld);
			EditorPrefsExtensions.SetBool("Enable world", enableWorld);
			enablePieces = EditorGUILayout.Toggle("Enable Pieces", enablePieces);
			if (enablePieces != piecesAreActive)
				SetPiecesActive (enablePieces);
			EditorPrefsExtensions.SetBool("Enable pieces", enablePieces);
			showPieces = EditorGUILayout.Toggle("Show Pieces", showPieces);
			if (showPieces != piecesAreShown)
				ShowPieces (showPieces);
			EditorPrefsExtensions.SetBool("Show pieces", showPieces);
			guiContent.text = "Use Enemy Battles Of World";
			bool useenemyGroupsOfWorld = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (useenemyGroupsOfWorld)
				UseEnemyGroupsOfWorld ();
			guiContent.text = "Use Enemy Battles Of Pieces";
			bool useenemyGroupsOfPieces = EditorGUILayout.DropdownButton(guiContent, FocusType.Passive);
			if (useenemyGroupsOfPieces)
				UseEnemyGroupsOfPieces ();
		}

		[MenuItem("World/Rebuild %&r")]
		public static void Rebuild ()
		{
			RemovePieces ();
			makeTilemaps = true;
			makeWorldObjects = true;
			MakePieces ();
			UseEnemyGroupsOfPieces ();
			enablePieces = true;
			SetPiecesActive (true);
			UpdateEnemyGroups ();
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneByName("Game"));
		}

		public static void ShowPieces (bool show)
		{
			GameManager.GetSingleton<World>().SetPieces ();
			foreach (WorldPiece worldPiece in GameManager.GetSingleton<World>().piecesDict.Values)
				worldPiece.gameObject.SetActive(show);
			piecesAreShown = show;
		}

		public static void SetWorldActive (bool active)
		{
			Transform sceneRoot = GameObject.Find("Test Scene").GetComponent<Transform>();
			for (int i = 0; i < sceneRoot.childCount; i ++)
			{
				// foreach (Tilemap tilemap in GameManager.GetSingleton<World>().tilemapsIncludedInPieces)
				// 	tilemap.gameObject.SetActive(active);
				ObjectInWorld worldObject = sceneRoot.GetChild(i).GetComponent<ObjectInWorld>();
				if (worldObject != null && worldObject.enabled)
					worldObject.go.SetActive(active);
			}
			worldIsActive = active;
		}

		public static void SetPiecesActive (bool active)
		{
			GameManager.GetSingleton<World>().piecesParent.gameObject.SetActive(active);
			piecesAreActive = active;
		}

		public static void UseEnemyGroupsOfWorld ()
		{
			for (int i = 0; i < GameManager.GetSingleton<World>().enemyGroups.Length; i ++)
			{
				for (int i2 = 0; i2 < GameManager.GetSingleton<World>().enemyGroups[i].enemies.Length; i2 ++)
				{
					if (GameManager.GetSingleton<World>().enemyGroups[i].enemies[i2].GetComponent<ObjectInWorld>().IsInPieces)
						GameManager.GetSingleton<World>().enemyGroups[i].enemies[i2] = GameManager.GetSingleton<World>().enemyGroups[i].enemies[i2].GetComponent<ObjectInWorld>().duplicateGo.GetComponent<Enemy_Follow>();
				}
				// for (int i2 = 0; i2 < GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies.Length; i2 ++)
				// {
				// 	if (GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies[i2].GetComponent<ObjectInWorld>().IsInPieces)
				// 		GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies[i2] = GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies[i2].GetComponent<ObjectInWorld>().duplicateGo.GetComponent<AwakableEnemy>();
				// }
			}
		}

		public static void UseEnemyGroupsOfPieces ()
		{
			for (int i = 0; i < GameManager.GetSingleton<World>().enemyGroups.Length; i ++)
			{
				for (int i2 = 0; i2 < GameManager.GetSingleton<World>().enemyGroups[i].enemies.Length; i2 ++)
				{
					if (!GameManager.GetSingleton<World>().enemyGroups[i].enemies[i2].GetComponent<ObjectInWorld>().IsInPieces)
						GameManager.GetSingleton<World>().enemyGroups[i].enemies[i2] = GameManager.GetSingleton<World>().enemyGroups[i].enemies[i2].GetComponent<ObjectInWorld>().duplicateGo.GetComponent<Enemy_Follow>();
				}
				// for (int i2 = 0; i2 < GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies.Length; i2 ++)
				// {
				// 	if (!GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies[i2].GetComponent<ObjectInWorld>().IsInPieces)
				// 		GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies[i2] = GameManager.GetSingleton<World>().enemyGroups[i].awakableEnemies[i2].GetComponent<ObjectInWorld>().duplicateGo.GetComponent<AwakableEnemy>();
				// }
			}
		}

		[MenuItem("World/Update enemy battles")]
		public static void UpdateEnemyGroups ()
		{
			GameManager.GetSingleton<World>().enemyGroups = FindObjectsOfType<EnemyGroup>();
			// foreach (EnemyGroup enemyGroup in GameManager.GetSingleton<World>().enemyGroups)
			// {
				// foreach (Enemy enemy in enemyGroup.enemies)
					// enemy.battleIAmPartOf = enemyBattle;
			// }
		}
		
		[MenuItem("World/Make pieces")]
		public static void MakePieces ()
		{
			WorldPiece piece;
			// TileBase[] tileBases;
			ObjectInWorld originalWorldObject;
			ObjectInWorld newWorldObject;
			Vector2Int pieceLocation = new Vector2Int();
			Vector2Int cellBoundsMin;
			Vector2Int cellBoundsMax;
			Vector2 worldBoundsMin;
			Vector2 worldBoundsMax;
			List<ObjectInWorld> worldObjects = new List<ObjectInWorld>();
			worldObjects.AddRange(GameManager.GetSingleton<World>().worldObjects);
			for (int x = GameManager.GetSingleton<World>().cellBoundsRect.xMin; x < GameManager.GetSingleton<World>().cellBoundsRect.xMax; x += GameManager.GetSingleton<World>().sizeOfPieces.x)
			{
				pieceLocation.y = 0;
				for (int y = GameManager.GetSingleton<World>().cellBoundsRect.yMin; y < GameManager.GetSingleton<World>().cellBoundsRect.yMax; y += GameManager.GetSingleton<World>().sizeOfPieces.y)
				{
					piece = (WorldPiece) PrefabUtility.InstantiatePrefab(GameManager.GetSingleton<World>().piecePrefab);
					piece.trs.SetParent(GameManager.GetSingleton<World>().piecesParent);
					piece.location = pieceLocation;
					piece.name += "[" + pieceLocation + "]";
					cellBoundsMin = new Vector2Int(x, y);
					cellBoundsMax = new Vector2Int(Mathf.Clamp(x + GameManager.GetSingleton<World>().sizeOfPieces.x, GameManager.GetSingleton<World>().cellBoundsRect.xMin, GameManager.GetSingleton<World>().cellBoundsRect.xMax), Mathf.Clamp(y + GameManager.GetSingleton<World>().sizeOfPieces.y, GameManager.GetSingleton<World>().cellBoundsRect.yMin, GameManager.GetSingleton<World>().cellBoundsRect.yMax));
					worldBoundsMin = cellBoundsMin;
					worldBoundsMax = cellBoundsMax;
					piece.cellBoundsRect = new RectInt();
					piece.cellBoundsRect.SetMinMax(cellBoundsMin, cellBoundsMax);
					// worldBoundsMin = GameManager.GetSingleton<World>().tilemapsIncludedInPieces[0].GetCellCenterWorld(cellBoundsMin.ToVec3Int()) - (GameManager.GetSingleton<World>().tilemapsIncludedInPieces[0].cellSize / 2);
					// worldBoundsMax = GameManager.GetSingleton<World>().tilemapsIncludedInPieces[0].GetCellCenterWorld(cellBoundsMax.ToVec3Int()) + (GameManager.GetSingleton<World>().tilemapsIncludedInPieces[0].cellSize / 2);
					piece.worldBoundsRect = Rect.MinMaxRect(worldBoundsMin.x, worldBoundsMin.y, worldBoundsMax.x, worldBoundsMax.y);
					// if (makeTilemaps)
					// {
					// 	piece.tilemaps = new Tilemap[GameManager.GetSingleton<World>().tilemapsIncludedInPieces.Length];
					// 	for (int i = 0; i < piece.tilemaps.Length; i ++)
					// 	{
					// 		GameObject newTilemapGo = (GameObject) PrefabUtility.InstantiatePrefab(GameManager.GetSingleton<GameManager>().emptyGoPrefab);
					// 		Tilemap worldTilemap = GameManager.GetSingleton<World>().tilemapsIncludedInPieces[i];
					// 		GameObject worldTilemapGo = worldTilemap.gameObject;
					// 		List<AddedGameObject> addedGos = PrefabUtility.GetAddedGameObjects(worldTilemapGo);
					// 		List<AddedComponent> addedComponents = PrefabUtility.GetAddedComponents(worldTilemapGo);
					// 		PropertyModification[] propertyModifications = PrefabUtility.GetPropertyModifications(worldTilemapGo);
					// 		Transform worldTilemapTrs = worldTilemapGo.GetComponent<Transform>();
					// 		Transform newTilemapTrs = newTilemapGo.GetComponent<Transform>();
					// 		Transform addedTrs;
					// 		foreach (AddedGameObject addedGo in addedGos)
					// 		{
					// 			addedTrs = addedGo.instanceGameObject.GetComponent<Transform>();
					// 			GameObject addedGoClone = (GameObject) GameManager.Clone (addedGo.instanceGameObject, TransformExtensions.FindEquivalentChild(worldTilemapTrs, addedTrs, newTilemapTrs));
					// 			addedGoClone.GetComponent<Transform>().localScale = addedTrs.localScale;
					// 		}
					// 		piece.tilemaps[i] = newTilemapGo.AddComponent<Tilemap>();
					// 		BoundsInt pieceBounds = piece.cellBoundsRect.Move(-piece.cellBoundsRect.size / 2).ToBoundsInt();
					// 		pieceBounds.size = pieceBounds.size.SetZ(1);
					// 		tileBases = worldTilemap.GetTilesBlock(pieceBounds);
					// 		piece.tilemaps[i].SetTilesBlock(pieceBounds, tileBases);
					// 		foreach (Vector3Int cellPositionInPiece in pieceBounds.allPositionsWithin)
					// 			piece.tilemaps[i].SetTransformMatrix(cellPositionInPiece, worldTilemap.GetTransformMatrix(cellPositionInPiece));
					// 		piece.tilemaps[i].color = worldTilemap.color;
					// 		foreach (AddedComponent addedComponent in addedComponents)
					// 		{
					// 			if (addedComponent.instanceComponent.GetType() != typeof(Tilemap))
					// 			{
					// 				var newComponent = newTilemapGo.AddComponent(addedComponent.instanceComponent.GetType());
					// 				ICopyable copyable = newComponent as ICopyable;
					// 				if (copyable != null)
					// 					copyable.Copy (worldTilemapGo.GetComponent(addedComponent.instanceComponent.GetType()));
					// 			}
					// 		}
					// 		TilemapRenderer worldTilemapRenderer = worldTilemapGo.GetComponent<TilemapRenderer>();
					// 		TilemapRenderer newTilemapRenderer = newTilemapGo.GetComponent<TilemapRenderer>();
					// 		newTilemapRenderer.sortingOrder = worldTilemapRenderer.sortingOrder;
					// 		newTilemapRenderer.sortingLayerName = worldTilemapRenderer.sortingLayerName;
					// 		PropertyModification propertyModification;
					// 		for (int i2 = 0; i2 < propertyModifications.Length; i2 ++)
					// 		{
					// 			propertyModification = propertyModifications[i2];
					// 			if (propertyModification.objectReference == null || propertyModification.target == null)
					// 			{
					// 				propertyModifications = propertyModifications.RemoveAt(i2);
					// 				i2 --;
					// 			}
					// 		}
					// 		if (propertyModifications.Length > 0)
					// 			PrefabUtility.SetPropertyModifications(newTilemapGo, propertyModifications);
					// 		// foreach (MonoBehaviour dontPreserveScript in GameManager.GetSingleton<World>().dontPreserveScripts)
					// 		// {
					// 		//     if (piece.tilemaps[i].GetComponent(dontPreserveScript.name) != null)
					// 		//         DestroyImmediate(piece.tilemaps[i].GetComponent(dontPreserveScript.name));
					// 		// }
					// 		newTilemapTrs.SetParent(piece.tilemapsParent);
					// 		newTilemapTrs.localPosition = Vector2.zero;
					// 		newTilemapTrs.name = worldTilemap.name;
					// 		newTilemapGo.layer = worldTilemapGo.layer;
					// 	}
					// }
					if (makeWorldObjects)
					{
						for (int i = 0; i < worldObjects.Count; i ++)
						{
							originalWorldObject = worldObjects[i];
							if (piece.worldBoundsRect.Contains(originalWorldObject.trs.position.ToVec2Int()))
							{
								// if (PrefabUtility.GetPrefabInstanceStatus(originalWorldObject) == PrefabInstanceStatus.NotAPrefab)
									newWorldObject = Instantiate(originalWorldObject);
								// else
								// 	newWorldObject = PrefabUtilityExtensions.ClonePrefabInstance(originalWorldObject.gameObject).GetComponent<ObjectInWorld>();
								newWorldObject.trs.SetParent(piece.worldObjectsParent);
								newWorldObject.trs.position = originalWorldObject.trs.position;
								newWorldObject.trs.rotation = originalWorldObject.trs.rotation;
								newWorldObject.trs.localScale = originalWorldObject.trs.localScale;
								newWorldObject.duplicateTrs = originalWorldObject.trs;
								newWorldObject.duplicateGo = originalWorldObject.go;
								newWorldObject.duplicateWorldObject = originalWorldObject;
								newWorldObject.pieceIAmIn = piece;
								originalWorldObject.duplicateTrs = newWorldObject.trs;
								originalWorldObject.duplicateGo = newWorldObject.go;
								originalWorldObject.duplicateWorldObject = newWorldObject;
								// foreach (MonoBehaviour dontPreserveScript in GameManager.GetSingleton<World>().dontPreserveScripts)
								// {
								//     if (newWorldObject.GetComponent(dontPreserveScript.name) != null)
								//         DestroyImmediate(newWorldObject.GetComponent(dontPreserveScript.name));
								// }
								newWorldObject.name = originalWorldObject.name;
								newWorldObject.enabled = false;
								Enemy_Follow enemy = newWorldObject.GetComponent<Enemy_Follow>();
								if (enemy != null)
									enemy.worldObject = newWorldObject;
								ICopyable[] originalCopyables = originalWorldObject.GetComponents<ICopyable>();
								ICopyable[] newCopyables = newWorldObject.GetComponents<ICopyable>();
								for (int i2 = 0; i2 < newCopyables.Length; i2 ++)
									newCopyables[i2].Copy (originalCopyables[i2]);
								worldObjects.RemoveAt(i);
								i --;
							}
						}
					}
					piece.gameObject.SetActive(false);
					pieceLocation.y ++;
				}
				pieceLocation.x ++;
			}
			ObjectInWorld firstEnemyWorldObject;
			ObjectInWorld otherEnemyWorldObject;
			foreach (EnemyGroup enemyBattle in GameManager.GetSingleton<World>().enemyGroups)
			{
				firstEnemyWorldObject = enemyBattle.enemies[0].worldObject;
				if (!firstEnemyWorldObject.IsInPieces && firstEnemyWorldObject.duplicateWorldObject != null)
					firstEnemyWorldObject = firstEnemyWorldObject.duplicateWorldObject;
				for (int i = 1; i < enemyBattle.enemies.Length; i ++)
				{
					otherEnemyWorldObject = enemyBattle.enemies[i].worldObject;
					if (!otherEnemyWorldObject.IsInPieces && otherEnemyWorldObject.duplicateWorldObject != null)
						otherEnemyWorldObject = otherEnemyWorldObject.duplicateWorldObject;
					otherEnemyWorldObject.trs.SetParent(firstEnemyWorldObject.trs.parent);
					otherEnemyWorldObject.pieceIAmIn = firstEnemyWorldObject.pieceIAmIn;
					if (firstEnemyWorldObject.IsInPieces)
						firstEnemyWorldObject.pieceIAmIn.worldBoundsRect = firstEnemyWorldObject.pieceIAmIn.worldBoundsRect.GrowToPoint(otherEnemyWorldObject.trs.position);
				}
			}
			ObjectInWorld worldObject;
			// foreach (MoveTile moveTile in GameManager.GetSingleton<World>().moveTiles)
			// {
			// 	worldObject = moveTile.worldObject;
			// 	if (!moveTile.worldObject.IsInPieces)
			// 		worldObject = worldObject.duplicateWorldObject;
			// 	foreach (Transform wayPoint in moveTile.wayPoints)
			// 		worldObject.pieceIAmIn.worldBoundsRect = worldObject.pieceIAmIn.worldBoundsRect.GrowToPoint(wayPoint.position);
			// }
			// foreach (Quest quest in GameManager.GetSingleton<QuestManager>().quests)
			// {
			// 	KillingQuest killingQuest = quest as KillingQuest;
			// 	if (killingQuest != null)
			// 	{
			// 		for (int i = 0; i < killingQuest.enemiesToKill.Length; i ++)
			// 		{
			// 			worldObject = killingQuest.enemiesToKill[i].worldObject;
			// 			if (!worldObject.IsInPieces)
			// 				worldObject = worldObject.duplicateWorldObject;
			// 			killingQuest.enemiesToKill[i] = worldObject.GetComponent<Enemy>();
			// 		}
			// 	}
			// }
			GameManager.GetSingleton<World>().maxPieceLocation = pieceLocation - Vector2Int.one;
		}

		[MenuItem("World/Remove pieces")]
		public static void RemovePieces ()
		{
			UseEnemyGroupsOfWorld ();
			ObjectInWorld enemyWorldObject;
			// foreach (Quest quest in GameManager.GetSingleton<QuestManager>().quests)
			// {
			// 	KillingQuest killingQuest = quest as KillingQuest;
			// 	if (killingQuest != null)
			// 	{
			// 		for (int i = 0; i < killingQuest.enemiesToKill.Length; i ++)
			// 		{
			// 			enemyWorldObject = killingQuest.enemiesToKill[i].worldObject;
			// 			if (enemyWorldObject.IsInPieces)
			// 				killingQuest.enemiesToKill[i] = enemyWorldObject.duplicateGo.GetComponent<Enemy>();
			// 		}
			// 	}
			// }
			WorldPiece piece;
			for (int i = 0; i < GameManager.GetSingleton<World>().piecesParent.childCount; i ++)
			{
				piece = GameManager.GetSingleton<World>().piecesParent.GetChild(i).GetComponent<WorldPiece>();
				DestroyImmediate(piece.gameObject);
				i --;
			}
		}
	}
}
#endif