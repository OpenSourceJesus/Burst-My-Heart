using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ClampedFloat
{
	public float value;
	public float min;
	public float max;
	
	public float GetValue ()
	{
		return Mathf.Clamp(value, min, max);
	}
	
	public void SetValue (float value)
	{
		this.value = value;
		value = GetValue();
	}
}