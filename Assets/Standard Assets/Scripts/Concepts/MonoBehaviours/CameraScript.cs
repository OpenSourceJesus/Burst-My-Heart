using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	// [ExecuteAlways]
	[RequireComponent(typeof(Camera))]
	[DisallowMultipleComponent]
	public class CameraScript : SingletonMonoBehaviour<CameraScript>
	{
		public Transform trs;
		public new Camera camera;
		public Vector2 viewSize;
		protected Rect normalizedScreenViewRect;
		protected float screenAspect;
		[HideInInspector]
		public Rect viewRect;
		
		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (camera == null)
					camera = GetComponent<Camera>();
				return;
			}
#endif
			trs.SetParent(null);
			trs.localScale = Vector3.one;
			viewRect.size = viewSize;
			// HandlePosition ();
			// HandleViewSize ();
			if (camera == Camera.main)
			{
				GameManager.singletons.Remove(GetType());
				GameManager.singletons.Add(GetType(), this);
			}
		}
		
		public virtual void HandlePosition ()
		{
			viewRect.center = trs.position;
		}
		
		public virtual void HandleViewSize ()
		{
			screenAspect = Screen.width / Screen.height;
			camera.aspect = viewSize.x / viewSize.y;
			camera.orthographicSize = Mathf.Min(viewSize.x / 2 / camera.aspect, viewSize.y / 2);
			normalizedScreenViewRect = new Rect();
			normalizedScreenViewRect.size = new Vector2(camera.aspect / screenAspect, Mathf.Min(1, screenAspect / camera.aspect));
			normalizedScreenViewRect.center = Vector2.one / 2;
			camera.rect = normalizedScreenViewRect;
		}
		
		public virtual void FollowPath (WaypointPath path)
		{
			path.AddFollower (trs);
		}
		
		public virtual void StopFollowingPath (WaypointPath path)
		{
			path.RemoveFollower (trs);
		}
	}
}