using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class MakeMapBounds : MonoBehaviour
{
	public Transform trs;
    public bool makeMap = false;
	public int numberOfBounds;
	public GameObject mapBoundPrefab;
	public float pattern;

	public virtual void Update ()
	{
        if (!makeMap)
            return;
        makeMap = false;
		for (float ang = pattern / numberOfBounds; ang <= pattern; ang += pattern / numberOfBounds)
		{
			Vector3 upFacing = trs.rotation * new Vector3(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
			float distFromCenter = mapBoundPrefab.GetComponent<BoxCollider2D>().size.y * mapBoundPrefab.transform.lossyScale.y / 2;
			if (GetComponent<CircleCollider2D>() != null)
				distFromCenter += GetComponent<CircleCollider2D>().bounds.extents.x;
			GameObject mapBound = Instantiate(mapBoundPrefab, trs.position + (upFacing.normalized * distFromCenter), Quaternion.LookRotation(trs.forward, upFacing));
			mapBound.transform.SetParent(transform);
		}
		for (int i = 1; i < transform.childCount; i ++)
			transform.GetChild(i).SetAsFirstSibling();
	}
}
