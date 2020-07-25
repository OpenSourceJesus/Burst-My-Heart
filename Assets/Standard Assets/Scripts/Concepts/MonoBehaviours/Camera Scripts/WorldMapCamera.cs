using UnityEngine;

namespace BMH
{
	public class WorldMapCamera : CameraScript
	{
		[SerializeField]
		Vector2 initViewSize;
		Rect viewRectWhenStartedZoom;
		Rect previousViewRect;
		Vector2 mousePositionWhenStartedZoom;
		Vector2 previousMousePosition;
		public float sizeMultiplier;
		public float zoomRate;
		public FloatRange sizeMultiplierRange;

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				initViewSize = viewSize;
				return;
			}
#endif
		}

		public virtual void DoUpdate ()
		{
			HandlePosition ();
			HandleViewSize ();
			previousMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
		}

		public override void HandlePosition ()
		{
			if (InputManager.GetAxis("Zoom") != 0)
				// trs.position = (Vector3) (previousViewRect.min + viewRect.size.Multiply(InputManager.GetWorldMousePosition() - viewRectWhenStartedZoom.center)).SetZ(trs.position.z);
				trs.position += (Vector3) (viewRect.center - (Vector2) camera.ScreenToWorldPoint(Input.mousePosition) - (previousViewRect.center - previousMousePosition));
			else
			{
				viewRectWhenStartedZoom = viewRect;
				mousePositionWhenStartedZoom = camera.ScreenToWorldPoint(Input.mousePosition);
			}
		}

		public override void HandleViewSize ()
		{
			previousViewRect = viewRect;
			sizeMultiplier = Mathf.Clamp(sizeMultiplier + InputManager.GetAxis("Zoom") * zoomRate * Time.unscaledDeltaTime, sizeMultiplierRange.min, sizeMultiplierRange.max);
			viewSize = initViewSize * sizeMultiplier;
			base.HandleViewSize ();
		}
	}
}