using UnityEngine;
using System.Collections;

namespace BMH
{
	[DisallowMultipleComponent]
	[ExecuteAlways]
	public class FaceCamera : MonoBehaviour
	{
		public Transform trs;
		public int xMultiplier;
		public int yMultiplier;
		public int zMultiplier;

		public virtual void Start ()
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

		public virtual void LateUpdate ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			Vector3 toCamera = GameManager.GetSingleton<CameraScript>().trs.position - trs.position;
			trs.rotation = Quaternion.LookRotation(transform.forward, new Vector3(toCamera.x * xMultiplier, toCamera.y * yMultiplier, toCamera.z * zMultiplier));
		}
	}
}