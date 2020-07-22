using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	public class Event : SingletonMonoBehaviour<Event>, IUpdatable
	{
		public Transform trs;
	    public Timer beginTimer;
		[HideInInspector]
	    public Player[] participators = new Player[0];
		[HideInInspector]
		public bool hasStarted;
		public SpriteRenderer spriteRenderer;
		public Color startedColor;
		public Transform timerVisualizerTrs;
		public Collider2D collider;
		public bool Enabled
		{
			get
			{
				return SaveAndLoadManager.GetValue<bool>(name + " enabled", true);
			}
			set
			{
				SaveAndLoadManager.SetValue(name + " enabled", value);
			}
		}
		public bool PauseWhileUnfocused
		{
			get
			{
				return GameManager.GetSingleton<OnlineBattle>() == null;
			}
		}

	    public override void Awake ()
	    {
#if UNITY_EDITOR
	    	if (!Application.isPlaying)
	    	{
	    		if (trs == null)
	    			trs = GetComponent<Transform>();
	    		if (spriteRenderer == null)
	    			spriteRenderer = GetComponent<SpriteRenderer>();
				return;
	    	}
#endif
	    	base.Awake ();
			beginTimer.Reset ();
			beginTimer.Start ();
	    	beginTimer.onFinished += Begin;
			GameManager.updatables = GameManager.updatables.Add(this);
	    }

		public virtual void OnDestroy ()
		{
#if UNITY_EDITOR
	    	if (!Application.isPlaying)
				return;
#endif
			if (hasStarted)
			{
				foreach (Player participator in participators)
					UnapplyEffect (participator);
			}
			else
			{
	    		beginTimer.onFinished -= Begin;
				GameManager.updatables = GameManager.updatables.Remove(this);
			}
		}

		public virtual void DoUpdate ()
		{
			timerVisualizerTrs.localScale = Vector2.one * beginTimer.TimeElapsed / beginTimer.duration;
		}

	    public virtual void Begin (params object[] args)
	    {
			if (participators.Length < 2)
			{
				Destroy (gameObject);
				return;
			}
			hasStarted = true;
			timerVisualizerTrs.gameObject.SetActive(false);
			spriteRenderer.color = startedColor;
			foreach (Player participator in participators)
				ApplyEffect (participator);
			beginTimer.Stop ();
			beginTimer.onFinished -= Begin;
			GameManager.updatables = GameManager.updatables.Remove(this);
	    }

		public virtual void ApplyEffect (Player player)
		{
		}

		public virtual void UnapplyEffect (Player player)
		{
		}

	    public virtual void OnTriggerEnter2D (Collider2D other)
	    {
	    	Body body = other.GetComponent<Body>();
	    	if (body != null)
	    		AddPlayer (body.player);
	    }

	    public virtual void OnTriggerExit2D (Collider2D other)
	    {
	    	Body body = other.GetComponent<Body>();
			if (body != null)
				RemovePlayer (body.player);
	    }

	    public virtual void AddPlayer (Player player)
	    {
			participators = participators.Add_class(player);
			if (hasStarted)
				ApplyEffect (player);
	    }

	    public virtual void RemovePlayer (Player player)
	    {
	    	participators = participators.Remove_class(player);
			if (hasStarted)
			{
				if (participators.Length < 2)
					Destroy (gameObject);
				else
					UnapplyEffect (player);
			}
	    }
	}
}