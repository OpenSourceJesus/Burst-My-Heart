using UnityEngine;
using System.Collections;

public class FreezeObject : MonoBehaviour
{
	public bool freezePosition;
	public bool freezeRelativePosition;
	public bool freezeRotation;
	Vector3 initRota;
	Vector3 initPos;
	Vector3 initRelativePos;

	void Start ()
	{
		initRota = transform.eulerAngles;
		initPos = transform.position;
		initRelativePos = transform.position - transform.parent.position;
	}

	void Update ()
	{
		if (freezeRotation)
			transform.eulerAngles = initRota;
		if (freezePosition)
			transform.position = initPos;
		else if (freezeRelativePosition)
			transform.position = transform.parent.position + initRelativePos;
	}
}
