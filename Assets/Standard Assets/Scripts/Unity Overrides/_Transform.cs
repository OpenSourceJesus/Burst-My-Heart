using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	//[ExecuteAlways]
	[DisallowMultipleComponent]
	public class _Transform : MonoBehaviour
	{
		public Transform trs;
		
		void Awake ()
		{
			if (!Application.isPlaying)
			{
				trs = GetComponent<Transform>();
				return;
			}
		}
		
		public void SetWorldScaleX (float scaleX)
		{
			trs.localScale = trs.localScale.SetX(trs.worldToLocalMatrix.MultiplyVector(trs.localScale * scaleX).x);
		}
		
		public void SetWorldScaleY (float scaleY)
		{
			trs.localScale = trs.localScale.SetY(trs.worldToLocalMatrix.MultiplyVector(trs.localScale * scaleY).y);
		}
		
		public void SetWorldScaleZ (float scaleZ)
		{
			trs.localScale = trs.localScale.SetZ(trs.worldToLocalMatrix.MultiplyVector(trs.localScale * scaleZ).z);
		}
		
		public void SetWorldScale (Vector3 scale)
		{
			trs.localScale = trs.worldToLocalMatrix.MultiplyVector(trs.localScale.Multiply(scale));
		}
		
		public void TeleportTo (Transform trs)
		{
			this.trs.position = trs.position;
		}
	}
}