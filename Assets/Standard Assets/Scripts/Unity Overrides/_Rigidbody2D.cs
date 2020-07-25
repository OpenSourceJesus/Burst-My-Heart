using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	//[ExecuteAlways]
	[RequireComponent(typeof(Rigidbody2D))]
	[DisallowMultipleComponent]
	public class _Rigidbody2D : MonoBehaviour
	{
		public static List<Rigidbody2D> allInstances = new List<Rigidbody2D>();
		public Rigidbody2D rigid;
		
		void Awake ()
		{
#if UNITY_EDITOR
			if (rigid == null)
				rigid = GetComponent<Rigidbody2D>();
#endif
			allInstances.Add(rigid);
		}
		
		void OnDestroy ()
		{
			allInstances.Remove(rigid);
		}
	}
}