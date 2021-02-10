#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

namespace BMH
{
	//[ExecuteAlways]
	public class AreaPath : EditorScript
	{
		public AreaPathNode rootAreaPathNode;
		public Color nodeConnectorColor;
		public Color borderColor;
		public bool surroundWithTerrains;
		public int minPointsForTerrains;
		public int maxPointsForTerrains;
		public int minSeperationForTerrains;
		public int maxSeperationForTerrains;
		public int minSeperationForPoints;
		public int maxSeperationForPoints;
		public int distToNodeForFirstTerrain;
		public float normalizedTraverseRate;
		public Terrain terrainPrefab;
		public LineSegment2D[] borders = new LineSegment2D[0];
		public AreaPathNode[] areaPathNodes = new AreaPathNode[0];

		public virtual void OnDrawGizmos ()
		{
			Gizmos.matrix = Matrix4x4.identity;
			LineSegment2D[] _borders = new LineSegment2D[0];
			AreaPathNode[] _areaPathNodes = new AreaPathNode[0];
			AreaPathNode[] uncheckedAreaPathNodes = new AreaPathNode[1] { rootAreaPathNode };
			LineSegment2D[] borderPair;
			while (uncheckedAreaPathNodes.Length > 0)
			{
				if (uncheckedAreaPathNodes[0] != null)
				{
					foreach (AreaPathNode childAreaPathNode in uncheckedAreaPathNodes[0].children)
					{
						if (childAreaPathNode != null && childAreaPathNode.gameObject.activeInHierarchy)
						{
							if (_areaPathNodes.Contains(childAreaPathNode))
							{
								Debug.LogError("AreaPath " + name + " has a cycle");
								return;
							}
							borderPair = GetBorders(uncheckedAreaPathNodes[0], childAreaPathNode);
							_borders = _borders.AddRange(borderPair);
							Gizmos.color = borderColor;
							Gizmos.DrawLine(borderPair[0].start, borderPair[0].end);
							Gizmos.DrawLine(borderPair[1].start, borderPair[1].end);
							Gizmos.color = nodeConnectorColor;
							Gizmos.DrawLine(uncheckedAreaPathNodes[0].trs.position, childAreaPathNode.trs.position);
						}
					}
					if (uncheckedAreaPathNodes[0].gameObject.activeInHierarchy)
					{
						uncheckedAreaPathNodes = uncheckedAreaPathNodes.AddRange(uncheckedAreaPathNodes[0].children);
						_areaPathNodes = _areaPathNodes.Add(uncheckedAreaPathNodes[0]);
					}
				}
				uncheckedAreaPathNodes = uncheckedAreaPathNodes.RemoveAt(0);
			}
			borders = _borders;
			areaPathNodes = _areaPathNodes;
		}

		public virtual LineSegment2D[] GetBorders (AreaPathNode node1, AreaPathNode node2)
		{
			LineSegment2D[] output = new LineSegment2D[2];
			Vector2 lineDirection = (node2.trs.position - node1.trs.position).normalized.Rotate(90);
			LineSegment2D line1 = new LineSegment2D((Vector2) node1.trs.position + (lineDirection * node1.radius), (Vector2) node1.trs.position - (lineDirection * node1.radius));
			LineSegment2D line2 = new LineSegment2D((Vector2) node2.trs.position - (lineDirection * node2.radius), (Vector2) node2.trs.position + (lineDirection * node2.radius));
			output[0] = new LineSegment2D(line1.start, VectorExtensions.GetClosestPoint(line1.start, line2.start, line2.end));
			output[1] = new LineSegment2D(line1.end, VectorExtensions.GetClosestPoint(line1.end, line2.start, line2.end));
			return output;
		}

		public virtual IEnumerator MakeRoutine ()
		{
			Terrain[] terrains = FindObjectsOfType<Terrain>();
			for (int i = 0; i < terrains.Length; i ++)
			{
				if (terrains[i].canBeErased && Contains(terrains[i]))
					terrains[i].gameObject.SetActive(false);
			}
			if (surroundWithTerrains)
			{
				Vector2 currentPoint;				
				do
				{
					currentPoint = (Vector2) rootAreaPathNode.trs.position + (Random.insideUnitCircle.normalized * (rootAreaPathNode.radius + distToNodeForFirstTerrain));
					yield return new WaitForEndOfFrame();
				} while (Contains(currentPoint));
				yield return EditorCoroutineUtility.StartCoroutine(MakeTerrainRoutine (currentPoint), this);
				AreaPathNode[] uncheckedAreaPathNodes = new AreaPathNode[1] { rootAreaPathNode };
				AreaPathNode[] deadEndAreaPathNodes = new AreaPathNode[0];
				while (uncheckedAreaPathNodes.Length > 0)
				{
					if (uncheckedAreaPathNodes[0].gameObject.activeInHierarchy)
					{
						uncheckedAreaPathNodes = uncheckedAreaPathNodes.AddRange(uncheckedAreaPathNodes[0].children);
						if (uncheckedAreaPathNodes[0].children.Length == 0)
							deadEndAreaPathNodes = deadEndAreaPathNodes.Add(uncheckedAreaPathNodes[0]);
					}
					uncheckedAreaPathNodes = uncheckedAreaPathNodes.RemoveAt(0);
				}
				float normalizedTraversedDistance = 0;
				foreach (AreaPathNode deadEndAreaPathNode in deadEndAreaPathNodes)
				{
					do
					{
						currentPoint = GetPointAlongCenterPath(deadEndAreaPathNode, normalizedTraversedDistance + normalizedTraverseRate);
						DebugExtensions.DrawPoint(currentPoint, 10, Color.red, .1f);
						normalizedTraversedDistance = GetNormalizedDistanceOfClosestPointAlongCenterPath(deadEndAreaPathNode, currentPoint);
						yield return new WaitForEndOfFrame();
					} while (normalizedTraversedDistance < 1);					
				}
			}
		}

		public virtual bool Contains (Terrain terrain)
		{
			LineSegment2D[] terrainBorders = terrain.GetBorders();
			foreach (LineSegment2D terrainBorder in terrainBorders)
			{
				if (Contains(terrainBorder))
					return true;
			}
			return false;
		}

		public virtual bool Contains (LineSegment2D lineSegment)
		{
			foreach (AreaPathNode areaPathNode in areaPathNodes)
			{
				if (lineSegment.DoIIntersectWithCircle(areaPathNode.trs.position, areaPathNode.radius))
					return true;
			}
			foreach (LineSegment2D border in borders)
			{
				if (lineSegment.DoIIntersectWith(border, true))
					return true;
			}
			if (Contains(lineSegment.start) || Contains(lineSegment.end))
				return true;
			return false;
		}

		public virtual bool Contains (Vector2 point)
		{
			foreach (AreaPathNode areaPathNode in areaPathNodes)
			{
				if (Vector2.Distance(areaPathNode.trs.position, point) <= areaPathNode.radius)
					return true;
			}
			for (int i = 0; i < borders.Length; i += 2)
			{
				LineSegment2D line1 = new LineSegment2D(borders[i].start, borders[i + 1].start);
				LineSegment2D line2 = new LineSegment2D(borders[i].end, borders[i + 1].end);
				float lengthDifference = line1.GetLength() - line2.GetLength();
				if (lengthDifference > 0)
				{
					line2.start -= line2.GetDirection() * Mathf.Abs(lengthDifference) / 2;
					line2.end += line2.GetDirection() * Mathf.Abs(lengthDifference) / 2;
				}
				else
				{
					line1.start -= line1.GetDirection() * Mathf.Abs(lengthDifference) / 2;
					line1.end += line1.GetDirection() * Mathf.Abs(lengthDifference) / 2;
				}
				Vector2 pivotPoint = point;
				float rotate = -line1.GetFacingAngle();
				line1 = line1.Rotate(pivotPoint, rotate);
				line2 = line2.Rotate(pivotPoint, rotate);
				Rect rect = Rect.MinMaxRect(Mathf.Min(line1.start.x, line2.end.x), Mathf.Min(line1.start.y, line2.end.y), Mathf.Max(line1.start.x, line2.end.x), Mathf.Max(line1.start.y, line2.end.y));
				point = point.Rotate(pivotPoint, rotate);
				if (point.x >= rect.xMin && point.x <= rect.xMax && point.y >= rect.yMin && point.y <= rect.yMax)
				{
					LineSegment2D rotatedBorder1 = borders[i].Rotate(pivotPoint, rotate);
					LineSegment2D rotatedBorder2 = borders[i + 1].Rotate(pivotPoint, rotate);
					LineSegment2D line = new LineSegment2D(rotatedBorder1.start, rotatedBorder2.start);
					if (lengthDifference > 0)
						line = new LineSegment2D(rotatedBorder1.end, rotatedBorder2.end);
					if (!VectorExtensions.IsInTriangle(point, line1.start, line2.start, line.start) && !VectorExtensions.IsInTriangle(point, line1.end, line2.end, line.end))
						return true;
				}
			}
			return false;
		}

		public virtual IEnumerator MakeTerrainRoutine (Vector2 position)
		{
			Terrain output = Instantiate(terrainPrefab, position, Quaternion.identity);
			Selection.activeObject = output;
			output.trs.position = position.SetZ(Random.Range(0f, -5f));
			output.terrain.ClearPoints();
			int pointCount = Random.Range(minPointsForTerrains, maxPointsForTerrains + 1);
			Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(minSeperationForPoints, maxSeperationForPoints);
			output.terrain.AddPoint(Vector2.zero);
			int pointIndex;
			for (int i = 1; i < pointCount; i ++)
			{
				while (true)
				{
					offset = Random.insideUnitCircle.normalized * Random.Range(minSeperationForPoints, maxSeperationForPoints);
					pointIndex = output.terrain.AddAutoPoint(output.terrain.pathData._points[i - 1] + offset);
					yield return new WaitForEndOfFrame();
					if ((i >= 3 && !output.IsPolygon()) || Contains(output))
						output.terrain.RemovePoint(pointIndex);
					else
						break;
				}
			}
			output.terrain.Build ();
			output.randomTerrainColor.Do ();
		}

		public virtual Vector2 GetPointAlongCenterPath (AreaPathNode endAreaPathNode, float normalizedDistance)
		{
			Vector2 output = VectorExtensions.NULL2;
			float centerPathLength = 0;
			List<LineSegment2D> centerPathSegments = new List<LineSegment2D>();
			List<AreaPathNode> _areaPathNodes = new List<AreaPathNode>();
			_areaPathNodes.AddRange(areaPathNodes);
			AreaPathNode checkAreaPathNode;
			LineSegment2D centerPathSegment;
			while (endAreaPathNode != rootAreaPathNode)
			{
				for (int i = 0; i < _areaPathNodes.Count; i ++)
				{
					checkAreaPathNode = _areaPathNodes[i];
					if (checkAreaPathNode.children.Contains(endAreaPathNode))
					{
						centerPathSegment = new LineSegment2D(checkAreaPathNode.trs.position, endAreaPathNode.trs.position);
						centerPathSegments.Insert(0, centerPathSegment);
						centerPathLength += Vector2.Distance(endAreaPathNode.trs.position, checkAreaPathNode.trs.position);
						endAreaPathNode = checkAreaPathNode;
						normalizedDistance -= centerPathSegment.GetLength() / centerPathLength;
						if (normalizedDistance <= 0)
						{
							output = centerPathSegment.GetPointWithDirectedDistance(-normalizedDistance * centerPathLength);
							break;
						}
						break;
					}
				}
			}
			return output;
		}

		public virtual float GetNormalizedDistanceOfClosestPointAlongCenterPath (AreaPathNode endAreaPathNode, Vector2 point)
		{
			float output = MathfExtensions.NULL_FLOAT;
			float centerPathLength = 0;
			List<LineSegment2D> centerPathSegments = new List<LineSegment2D>();
			List<AreaPathNode> _areaPathNodes = new List<AreaPathNode>();
			_areaPathNodes.AddRange(areaPathNodes);
			AreaPathNode checkAreaPathNode;
			Vector2 closestPoint;
			float distanceToClosestPoint = Mathf.Infinity;
			float checkDistance;
			LineSegment2D centerPathSegment;
			float currentNormalizedDistance = 0;
			Vector2 checkPoint;
			while (endAreaPathNode != rootAreaPathNode)
			{
				for (int i = 0; i < _areaPathNodes.Count; i ++)
				{
					checkAreaPathNode = _areaPathNodes[i];
					if (checkAreaPathNode.children.Contains(endAreaPathNode))
					{
						centerPathSegment = new LineSegment2D(checkAreaPathNode.trs.position, endAreaPathNode.trs.position);
						centerPathSegments.Insert(0, centerPathSegment);
						centerPathLength += Vector2.Distance(endAreaPathNode.trs.position, checkAreaPathNode.trs.position);
						endAreaPathNode = checkAreaPathNode;
						checkPoint = centerPathSegment.ClosestPoint(point);
						checkDistance = Vector2.Distance(checkPoint, point);
						if (checkDistance < distanceToClosestPoint)
						{
							distanceToClosestPoint = checkDistance;
							closestPoint = checkPoint;
							output = currentNormalizedDistance + centerPathSegment.GetDirectedDistanceAlongParallel(closestPoint);
						}
						currentNormalizedDistance += centerPathSegment.GetLength() / centerPathLength;
						break;
					}
				}
			}
			return output;
		}

		public virtual float GetWidthAtNormalizedDistanceAlongCenterPath (AreaPathNode endAreaPathNode, float normalizedDistance)
		{
			return MathfExtensions.NULL_FLOAT;
		}
	}
}
#endif