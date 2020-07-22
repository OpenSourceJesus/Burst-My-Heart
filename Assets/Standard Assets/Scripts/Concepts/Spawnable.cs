using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMH
{
	public class Spawnable : MonoBehaviour, ISpawnable
	{
		public Transform trs;
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
	}
}