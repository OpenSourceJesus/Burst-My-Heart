using System.Collections.Generic;
using UnityEngine;
using Extensions;
// using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BMH
{
	[ExecuteInEditMode]
	public class World : MonoBehaviour, IUpdatable
	{
		// public Tilemap[] tilemaps;
		// public Tilemap[] tilemapsIncludedInPieces;
		public ObjectInWorld[] worldObjects;
		// public MoveTile[] moveTiles;
		public Vector2Int sizeOfPieces;
		public WorldPiece piecePrefab;
		public Dictionary<Vector2Int, WorldPiece> piecesDict = new Dictionary<Vector2Int, WorldPiece>();
		public WorldPiece[,] pieces;
		public Vector2Int maxPieceLocation;
		public Transform piecesParent;
		public RectInt cellBoundsRect;
		public Rect worldBoundsRect;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}
		public Vector2Int loadPiecesRange;
		public List<WorldPiece> piecesNearPlayer = new List<WorldPiece>();
		public List<WorldPiece> activePieces = new List<WorldPiece>();
		WorldPiece[] surroundingPieces;
		Rect loadPieceRangeRect = new Rect();
		// public MonoBehaviour[] dontPreserveScripts = new MonoBehaviour[0];
		public EnemyGroup[] enemyGroups = new EnemyGroup[0];
#if UNITY_EDITOR
		public EnemyGroup enemyGroupPrefab;
		public bool update;
#endif

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			else
			{
				// WorldMakerWindow.SetWorldActive (false);
				// WorldMakerWindow.SetPiecesActive (true);
				// WorldMakerWindow.ShowPieces (false);
			}
#endif
			Init ();
			GameManager.updatables = GameManager.updatables.Add(this);
		}

#if UNITY_EDITOR
		public virtual void Update ()
		{
			if (!update)
				return;
			update = false;
			worldObjects = FindObjectsOfType<ObjectInWorld>();
			ObjectInWorld worldObject;
			for (int i = 0; i < worldObjects.Length; i ++)
			{
				worldObject = worldObjects[i];
				if (!worldObject.enabled || worldObject.trs.parent.GetComponent<ObjectInWorld>() != null)
				{
					worldObjects = worldObjects.RemoveAt(i);
					i --;
				}
			}
			// moveTiles = FindObjectsOfType<MoveTile>();
			// cellBoundsRect = new RectInt();
			// cellBoundsRect.min = Vector2Int.zero;
			// cellBoundsRect.max = Vector2Int.zero;
			// foreach (Tilemap tilemap in tilemaps)
			// 	cellBoundsRect.SetMinMax(cellBoundsRect.min.SetToMinComponents(tilemap.cellBounds.min.ToVec2Int()), cellBoundsRect.max.SetToMaxComponents(tilemap.cellBounds.max.ToVec2Int()));
			// Vector2 worldBoundsMin = tilemaps[0].GetCellCenterWorld(cellBoundsRect.min.ToVec3Int()) - (tilemaps[0].cellSize / 2);
			// Vector2 worldBoundsMax = tilemaps[0].GetCellCenterWorld(cellBoundsRect.max.ToVec3Int()) + (tilemaps[0].cellSize / 2);
			// worldBoundsRect = Rect.MinMaxRect(worldBoundsMin.x, worldBoundsMin.y, worldBoundsMax.x, worldBoundsMax.y);
		}
#endif

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			GameManager.updatables = GameManager.updatables.Remove(this);
		}

		public virtual void SetPieces ()
		{
			pieces = new WorldPiece[maxPieceLocation.x + 1, maxPieceLocation.y + 1];
			piecesDict.Clear();
			WorldPiece piece;
			for (int i = 0; i < piecesParent.childCount; i ++)
			{
				piece = piecesParent.GetChild(i).GetComponent<WorldPiece>();
				piecesDict.Add(piece.location, piece);
				pieces[piece.location.x, piece.location.y] = piece;
			}
		}

		public virtual void Init ()
		{
			SetPieces ();
			piecesNearPlayer.Clear();
			activePieces.Clear();
			WorldPiece _piece;
			for (int x = 0; x <= maxPieceLocation.x; x ++)
			{
				for (int y = 0; y <= maxPieceLocation.y; y ++)
				{
					_piece = pieces[x, y];
					if (_piece.worldBoundsRect.Contains(GameManager.GetSingleton<HumanPlayer>().body.trs.position.ToVec2Int()))
					{
						surroundingPieces = GetSurroundingPieces(_piece);
						foreach (WorldPiece surroundingPiece in surroundingPieces)
						{
							loadPieceRangeRect = surroundingPiece.worldBoundsRect.Expand(loadPiecesRange * 2);
							if (GameManager.GetSingleton<GameCamera>().viewRect.IsIntersecting(loadPieceRangeRect))
							{
								piecesNearPlayer.Add(surroundingPiece);
								activePieces.Add(surroundingPiece);
								surroundingPiece.gameObject.SetActive(true);
							}
						}
						piecesNearPlayer.Add(_piece);
						activePieces.Add(_piece);
						_piece.gameObject.SetActive(true);
						piecesNearPlayer.AddRange(GetSurroundingPieces(activePieces.ToArray()));
						return;
					}
				}
			}
		}

		bool pieceShouldBeActive;
		WorldPiece pieceNearPlayer;
		public virtual void DoUpdate ()
		{
			for (int i = 0; i < piecesNearPlayer.Count; i ++)
			{
				pieceNearPlayer = piecesNearPlayer[i];
				loadPieceRangeRect = pieceNearPlayer.worldBoundsRect.Expand(loadPiecesRange * 2);
				pieceShouldBeActive = GameManager.GetSingleton<GameCamera>().viewRect.IsIntersecting(loadPieceRangeRect);
				if (pieceShouldBeActive)
				{
					if (!pieceNearPlayer.gameObject.activeSelf)
					{
						pieceNearPlayer.gameObject.SetActive(true);
						activePieces.Add(pieceNearPlayer);
						surroundingPieces = GetSurroundingPieces(pieceNearPlayer);
						foreach (WorldPiece surroundingPiece in surroundingPieces)
						{
							if (!piecesNearPlayer.Contains(surroundingPiece))
								piecesNearPlayer.Add(surroundingPiece);
						}
					}
				}
				else if (pieceNearPlayer.gameObject.activeSelf)
				{
					pieceNearPlayer.gameObject.SetActive(false);
					activePieces.Remove(pieceNearPlayer);
				}
				else if (!GetSurroundingPieces(activePieces.ToArray()).Contains(pieceNearPlayer))
				{
					piecesNearPlayer.RemoveAt(i);
					i --;
				}
			}
		}

		public virtual WorldPiece[] GetSurroundingPieces (params WorldPiece[] innerPieces)
		{
			List<WorldPiece> output = new List<WorldPiece>();
			foreach (WorldPiece piece in innerPieces)
			{
				bool hasPieceRight = piece.location.x < maxPieceLocation.x;
				bool hasPieceLeft = piece.location.x > 0;
				bool hasPieceUp = piece.location.y < maxPieceLocation.y;
				bool hasPieceDown = piece.location.y > 0;
				if (hasPieceUp)
				{
					output.Add(pieces[piece.location.x, piece.location.y + 1]);
					if (hasPieceRight)
						output.Add(pieces[piece.location.x + 1, piece.location.y + 1]);
					if (hasPieceLeft)
						output.Add(pieces[piece.location.x - 1, piece.location.y + 1]);
				}
				if (hasPieceDown)
				{
					output.Add(pieces[piece.location.x, piece.location.y - 1]);
					if (hasPieceRight)
						output.Add(pieces[piece.location.x + 1, piece.location.y - 1]);
					if (hasPieceLeft)
						output.Add(pieces[piece.location.x - 1, piece.location.y - 1]);
				}
				if (hasPieceRight)
					output.Add(pieces[piece.location.x + 1, piece.location.y]);
				if (hasPieceLeft)
					output.Add(pieces[piece.location.x - 1, piece.location.y]);
				foreach (WorldPiece piece2 in innerPieces)
					output.Remove(piece2);
			}
			return output.ToArray();
		}
	}
}