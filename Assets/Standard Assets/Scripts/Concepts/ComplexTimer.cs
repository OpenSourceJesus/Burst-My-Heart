using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ComplexTimer : MonoBehaviour
{
	public string title;
	public bool realtime;
	public bool useFixedUpdate;
	public ClampedFloat value;
	public float changeAmountMultiplier;
	public RepeatType repeatType;
	float timeSinceLastGetValue;
	float previousChangeAmountMultiplier;
	[HideInInspector]
	public float initValue;
	
	public float GetValue ()
	{
		value.SetValue(value.GetValue() + timeSinceLastGetValue * changeAmountMultiplier);
		if ((value.GetValue() == value.max && changeAmountMultiplier > 0) || (value.GetValue() == value.min && changeAmountMultiplier < 0))
		{
			if (repeatType == RepeatType.Loop)
				JumpToStart ();
			else if (repeatType  == RepeatType.PingPong)
				changeAmountMultiplier *= -1;
		}
		timeSinceLastGetValue = 0;
		return value.GetValue();
	}
	
	void Awake ()
	{
		if (value == null)
			value = new ClampedFloat();
		initValue = value.GetValue();
	}
	
	void Update ()
	{
		if (!useFixedUpdate)
		{
			if (realtime)
				UpdateTimer(Time.unscaledDeltaTime);
			else
				UpdateTimer(Time.deltaTime);
		}
	}
	
	void FixedUpdate ()
	{
		if (useFixedUpdate)
		{
			if (realtime)
				UpdateTimer(Time.fixedUnscaledDeltaTime);
			else
				UpdateTimer(Time.fixedDeltaTime);
		}
	}
	
	void UpdateTimer (float deltaTime)
	{
		timeSinceLastGetValue += deltaTime;
	}
	
	public bool IsAtStart ()
	{
		float timerValue = GetValue();
		return (timerValue == value.max && changeAmountMultiplier < 0) || (timerValue == value.min && changeAmountMultiplier > 0) || ((timerValue == value.min || timerValue == value.max) && changeAmountMultiplier == 0);
	}
	
	public bool IsAtEnd ()
	{
		float timerValue = GetValue();
		return (timerValue == value.min && changeAmountMultiplier < 0) || (timerValue == value.max && changeAmountMultiplier > 0) || ((timerValue == value.min || timerValue == value.max) && changeAmountMultiplier == 0);
	}
	
	public void Pause ()
	{
		if (changeAmountMultiplier == 0)
			return;
		previousChangeAmountMultiplier = changeAmountMultiplier;
		changeAmountMultiplier = 0;
	}
	
	public void Resume ()
	{
		if (changeAmountMultiplier != 0)
			return;
		changeAmountMultiplier = previousChangeAmountMultiplier;
	}
	
	public void JumpToStart ()
	{
		if (changeAmountMultiplier > 0 || (changeAmountMultiplier == 0 && previousChangeAmountMultiplier > 0))
			value.SetValue(value.min);
		else
			value.SetValue(value.max);
	}
	
	public void JumpToEnd ()
	{
		if (changeAmountMultiplier > 0 || (changeAmountMultiplier == 0 && previousChangeAmountMultiplier > 0))
			value.SetValue(value.max);
		else
			value.SetValue(value.min);
	}
	
	public void JumpToInitValue ()
	{
		value.SetValue(initValue);
	}
	
	public enum RepeatType
	{
		End = 0,
		Loop = 1,
		PingPong = 2
	}
}