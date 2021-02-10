using UnityEngine;
using System.Collections;

namespace BMH
{
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class FaceCamera : UpdateWhileEnabled
	{
		public Transform trs;
		public int xMultiplier;
		public int yMultiplier;
		public int zMultiplier;

		void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				return;
			}
#endif
		}

		public override void DoUpdate ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			Vector3 toCamera = CameraScript.Instance.trs.position - trs.position;
			trs.rotation = Quaternion.LookRotation(trs.forward, new Vector3(toCamera.x * xMultiplier, toCamera.y * yMultiplier, toCamera.z * zMultiplier));
		}
	}
}