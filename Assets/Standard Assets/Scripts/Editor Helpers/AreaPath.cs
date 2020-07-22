#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using Ferr;
using UnityEditor;

namespace BMH
{
	[ExecuteAlways]
	public class AreaPath : EditorHelper
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
		public Terrain terrainPrefab;
		public LineSegment2D[] borders = new LineSegment2D[0];
		public AreaPathNode[] areaPathNodes = new AreaPathNode[0];
		public Transform trs;

		public virtual void OnDrawGizmos ()
		{
			Debug.Log(Contains(trs.position));
			Gizmos.matrix = Matrix4x4.identity;
			LineSegment2D[] _borders = new LineSegment2D[0];
			AreaPathNode[] _areaPathNodes = new AreaPathNode[0];
			TreeNode<AreaPathNode> rootTreeNode = new TreeNode<AreaPathNode>(rootAreaPathNode);
			TreeNode<AreaPathNode>[] uncheckedTreeNodes = new TreeNode<AreaPathNode>[] { rootTreeNode };
			LineSegment2D[] borderPair = new LineSegment2D[2];
			while (uncheckedTreeNodes.Length > 0)
			{
				foreach (AreaPathNode childAreaPathNode in uncheckedTreeNodes[0].Value.children)
				{
					if (_areaPathNodes.Contains_class(childAreaPathNode))
					{
						Debug.LogError("There is a cycle in an AreaPath");
						return;
					}
					borderPair = GetBorders(uncheckedTreeNodes[0].Value, childAreaPathNode);
					_borders = _borders.AddRange_class(borderPair);
					Gizmos.color = borderColor;
					Gizmos.DrawLine(borderPair[0].start, borderPair[0].end);
					Gizmos.DrawLine(borderPair[1].start, borderPair[1].end);
					Gizmos.color = nodeConnectorColor;
					Gizmos.DrawLine(uncheckedTreeNodes[0].Value.trs.position, childAreaPathNode.trs.position);
				}
				uncheckedTreeNodes = uncheckedTreeNodes.AddRange_class(uncheckedTreeNodes[0].AddChildren(uncheckedTreeNodes[0].Value.children));
				_areaPathNodes = _areaPathNodes.Add_class(uncheckedTreeNodes[0].Value);
				uncheckedTreeNodes = uncheckedTreeNodes.RemoveAt_class(0);
			}
			borders = _borders;
			areaPathNodes = _areaPathNodes;
		}

		public virtual LineSegment2D[] GetBorders (AreaPathNode node, AreaPathNode node2)
		{
			LineSegment2D[] output = new LineSegment2D[2];
			LineSegment2D line = new LineSegment2D(node.trs.position, node2.trs.position);
			line = line.Move((Vector2) node.trs.position - line.GetMidpoint());
			line = line.GetPerpendicular();
			Vector2 lineDirection = line.GetDirection();
			line.start = (Vector2) node.trs.position + (lineDirection * node.radius);
			line.end = (Vector2) node.trs.position - (lineDirection * node.radius);
			LineSegment2D line2 = new LineSegment2D(node.trs.position, node2.trs.position);
			line2 = line2.Move((Vector2) node2.trs.position - line2.GetMidpoint());
			line2 = line2.GetPerpendicular();
			lineDirection = line2.GetDirection();
			line2.start = (Vector2) node2.trs.position + (lineDirection * node2.radius);
			line2.end = (Vector2) node2.trs.position - (lineDirection * node2.radius);
			output[0] = new LineSegment2D(line.start, VectorExtensions.GetClosestPoint(line.start, line2.start, line2.end));
			output[1] = new LineSegment2D(line.end, VectorExtensions.GetClosestPoint(line.end, line2.start, line2.end));
			if (Mathf.Abs(output[0].GetDirectedDistanceAlongParallel(node.trs.position) - output[1].GetDirectedDistanceAlongParallel(node.trs.position)) > 0.001f)
				output[0] = output[0].Flip();
			return output;
		}

		public virtual LineSegment2D[] GetBorders (Terrain terrain)
		{
			LineSegment2D[] output = new LineSegment2D[terrain.terrain.pathData._points.Count];
			for (int i = 0; i < terrain.terrain.pathData._points.Count - 1; i ++)
				output[i] = new LineSegment2D((Vector2) terrain.trs.position + terrain.terrain.pathData._points[i], (Vector2) terrain.trs.position + terrain.terrain.pathData._points[i + 1]);
			output[output.Length - 1] = new LineSegment2D((Vector2) terrain.trs.position + terrain.terrain.pathData._points[output.Length - 1], (Vector2) terrain.trs.position + terrain.terrain.pathData._points[0]);
			return output;
		}

		public virtual bool IsPolygon (Terrain terrain)
		{
			LineSegment2D[] borders = GetBorders(terrain);
			foreach (LineSegment2D border in borders)
			{
				foreach (LineSegment2D otherBorder in borders)
				{
					if (border != otherBorder && border.DoIIntersectWith(otherBorder, false))
						return false;
				}
			}
			return true;
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
				while (true)
				{
					do
					{
						currentPoint = (Vector2) rootAreaPathNode.trs.position + (Random.insideUnitCircle.normalized * (rootAreaPathNode.radius + distToNodeForFirstTerrain));
						yield return new WaitForEndOfFrame();
					} while (Contains(currentPoint));
					yield return StartCoroutine(MakeTerrainRoutine (currentPoint));
				}
			}
		}

		public virtual bool Contains (Terrain terrain)
		{
			LineSegment2D[] terrainBorders = GetBorders(terrain);
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
					line2.start -= line1.GetDirection() * Mathf.Abs(lengthDifference) / 2;
					line2.end += line1.GetDirection() * Mathf.Abs(lengthDifference) / 2;
				}
				else
				{
					line1.start -= line1.GetDirection() * Mathf.Abs(lengthDifference) / 2;
					line1.end += line1.GetDirection() * Mathf.Abs(lengthDifference) / 2;
				}
				float rotate = -line1.GetFacingAngle();
				Vector2 pivotPoint = new LineSegment2D(line1.GetMidpoint(), line2.GetMidpoint()).GetMidpoint();
				// Debug.DrawLine(line1.start, line1.end, Color.green.SetAlpha(0.25f * Time.deltaTime), .1f);
				// Debug.DrawLine(line2.start, line2.end, Color.red.SetAlpha(0.25f * Time.deltaTime), .1f);
				line1 = line1.Rotate(pivotPoint, rotate);
				line2 = line2.Rotate(pivotPoint, rotate);
				// Debug.DrawLine(line1.start, line1.end, Color.green, .1f);
				// Debug.DrawLine(line2.start, line2.end, Color.red, .1f);
				DebugExtensions.DrawPoint(pivotPoint, 10, Color.blue, .1f);
				// DebugExtensions.DrawPoint(point, 10, Color.yellow, .1f);
				Rect rect = Rect.MinMaxRect(Mathf.Min(line1.start.x, line2.end.x), Mathf.Min(line1.start.y, line2.end.y), Mathf.Max(line1.start.x, line2.end.x), Mathf.Max(line1.start.y, line2.end.y));
				point = point.Rotate(pivotPoint, rotate);
				DebugExtensions.DrawRect(rect, Color.black, .1f);
				DebugExtensions.DrawPoint(point, 10, Color.yellow, .1f);
				Debug.DrawLine(pivotPoint, point, Color.white, .1f);
				if (point.x >= rect.xMin && point.x <= rect.xMax && point.y >= rect.yMin && point.y <= rect.yMax)
				{
					LineSegment2D rotatedBorder1 = borders[i].Rotate(pivotPoint, rotate);
					LineSegment2D rotatedBorder2 = borders[i + 1].Rotate(pivotPoint, rotate);
					LineSegment2D line = new LineSegment2D(rotatedBorder1.start, rotatedBorder2.start);
					if (lengthDifference > 0)
						line = new LineSegment2D(rotatedBorder1.end, rotatedBorder2.end);
					// DebugExtensions.DrawPoint(line1.start, 10, Color.green, .1f);
					// DebugExtensions.DrawPoint(line2.start, 10, Color.red, .1f);
					// DebugExtensions.DrawPoint(line.start, 10, Color.blue, .1f);
					// DebugExtensions.DrawPoint(line1.end, 10, Color.yellow, .1f);
					// DebugExtensions.DrawPoint(line2.end, 10, Color.magenta, .1f);
					// DebugExtensions.DrawPoint(line.end, 10, Color.cyan, .1f);
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
					if ((i >= 3 && !IsPolygon(output)) || Contains(output))
						output.terrain.RemovePoint(pointIndex);
					else
						break;
				}
			}
			output.terrain.Build ();
		}
	}
}
#endif