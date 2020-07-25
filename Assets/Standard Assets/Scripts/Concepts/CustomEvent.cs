using UnityEngine.Events;
using System;

[Serializable]
public class CustomEvent
{
    public UnityEvent unityEvent;

    public virtual void Do ()
    {
        unityEvent.Invoke();
    }
}