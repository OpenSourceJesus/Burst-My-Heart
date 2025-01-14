using UnityEngine;
using System.Collections.Generic;
using Extensions;

namespace BMH
{
    //[ExecuteAlways]
	public class SetZOrder : MonoBehaviour
	{
        public new Renderer renderer;
        public short sortingOrder;
        public bool useSortingLayerId;
        public int sortingLayerId;
        public string sortingLayerName;

        public virtual void OnEnable ()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (renderer == null)
                    renderer = GetComponent<Renderer>();
            }
#endif
            if (renderer != null)
            {
                if (useSortingLayerId)
                    renderer.sortingLayerID = sortingLayerId;
                else
                    renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = sortingOrder;
            }
        }
    }
}