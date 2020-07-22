using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	public class WaypointPath : MonoBehaviour
	{
		public Transform trs;
		public Transform[] waypoints = new Transform[0];
		public int[] wayPointCosts = new int[0];
		public float[] moveRates = new float[0];
		public float[] rotaRates = new float[0];
		public bool[] waitForRota = new bool[0];
		public Button invokeOnStart;
		public Button[] invokeAfterReachedWaypoint = new Button[0];
		public Follower[] followers = new Follower[0];
		public bool followClosestWaypoint;

		public virtual void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (waypoints == null || waypoints.Length == 0)
				{
					foreach (Transform waypoint in GetComponentsInChildren<Transform>())
					{
						if (waypoint != trs)
							waypoints.Add_class(waypoint);
					}
				}
				return;
			}
#endif
		}

		public virtual IEnumerator FollowRoutine (Follower follower)
		{
			while (true)
			{
				Vector3 heading = Vector3.RotateTowards(follower.trs.position, waypoints[follower.currentWaypoint].position, rotaRates[follower.currentWaypoint] * Time.deltaTime, moveRates[follower.currentWaypoint] * Time.deltaTime);
				follower.trs.position = heading;
				follower.trs.rotation = Quaternion.SlerpUnclamped(follower.trs.rotation, waypoints[follower.currentWaypoint].rotation, rotaRates[follower.currentWaypoint] * Time.deltaTime);
				if (follower.trs.position == waypoints[follower.currentWaypoint].position && ((waitForRota[follower.currentWaypoint] && follower.trs.rotation == waypoints[follower.currentWaypoint].rotation) || !waitForRota[follower.currentWaypoint]))
				{
					if (invokeAfterReachedWaypoint[follower.currentWaypoint] != null)
						invokeAfterReachedWaypoint[follower.currentWaypoint].onClick.Invoke ();
					if (follower.currentWaypoint == waypoints.Length - 1)
					{
						RemoveFollower (follower.trs);
						yield break;
					}
					if (followClosestWaypoint)
						follower.currentWaypoint = waypoints.IndexOf_class(GameManager.FindClosestTransform(waypoints, follower.trs));
					else
						follower.currentWaypoint ++;
				}
				yield return new WaitForEndOfFrame();
			}
		}

		public virtual void AddFollower (Transform followerTrs)
		{
			if (invokeOnStart != null)
				invokeOnStart.onClick.Invoke ();
			Follower follower = new Follower();
			follower.trs = followerTrs;
			follower.currentWaypoint = 0;
			if (followClosestWaypoint)
				follower.currentWaypoint = waypoints.IndexOf_class(GameManager.FindClosestTransform(waypoints, followerTrs));
			followers.Add_class(follower);
			StartCoroutine(FollowRoutine (follower));
		}

		public virtual void RemoveFollower (Transform followerTrs)
		{
			Follower follower = null;
			for (int i = 0; i < followers.Length; i ++)
			{
				follower = followers[i];
				if (follower.trs == followerTrs)
				{
					followers.RemoveAt_class(i);
					StopCoroutine(FollowRoutine (follower));
					return;
				}
			}
		}

		public virtual float CalculateTotalCost (Transform potentialFollowerTrs)
		{
			float totalCost = 0;
			Transform currentWaypoint = null;
			for (int i = 0; i < waypoints.Length; i ++)
			{
				if (followClosestWaypoint)
				{
					currentWaypoint = GameManager.FindClosestTransform (waypoints, currentWaypoint);
					totalCost += Vector2.Distance(currentWaypoint.position, potentialFollowerTrs.position) + wayPointCosts[waypoints.IndexOf_class(currentWaypoint)];
					if (currentWaypoint == waypoints[waypoints.Length - 1])
						return totalCost;
				}
				else
					totalCost += Vector2.Distance(waypoints[i].position, potentialFollowerTrs.position) + wayPointCosts[i];
			}
			return totalCost;
		}

		[Serializable]
		public class Follower
		{
			public Transform trs;
			public int currentWaypoint;
		}
	}
}