using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	[RequireComponent(typeof(Camera))]
	[DisallowMultipleComponent]
	public class CameraScript : SingletonMonoBehaviour<CameraScript>
	{
		public Transform trs;
		public new Camera camera;
		public Vector2 viewSize;
		protected Rect normalizedScreenViewRect;
		protected float screenAspect;
		
		public override void Awake ()
		{
			base.Awake ();
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
			HandlePosition ();
			HandleViewSize ();
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
		}
		
		public virtual void LateUpdate ()
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			#endif
			HandlePosition ();
			HandleViewSize ();
		}
		
		public virtual void HandlePosition ()
		{
		}
		
		public virtual void HandleViewSize ()
		{
			screenAspect = (float) Screen.width / Screen.height;
			camera.aspect = viewSize.x / viewSize.y;
			camera.orthographicSize = Mathf.Max(viewSize.x / 2 / camera.aspect, viewSize.y / 2);
			normalizedScreenViewRect = new Rect();
			normalizedScreenViewRect.size = new Vector2(camera.aspect / screenAspect, Mathf.Min(1, screenAspect / camera.aspect));
			normalizedScreenViewRect.center = Vector2.one / 2;
			camera.rect = normalizedScreenViewRect;
		}
		
		public virtual void FollowPath (WaypointPath path)
		{
			path.AddFollower (trs);
		}
	}
}