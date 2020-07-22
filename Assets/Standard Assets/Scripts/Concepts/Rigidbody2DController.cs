using UnityEngine;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	[RequireComponent(typeof(Rigidbody2D))]
	[DisallowMultipleComponent]
    public class Rigidbody2DController : SingletonMonoBehaviour<Rigidbody2DController>, IUpdatable
    {
		public Transform trs;
		public Rigidbody2D rigid;
		public Collider2D collider;
		public SpriteRenderer spriteRenderer;
		public float moveSpeed;
		public DistanceJoint2D distanceJoint;
		public Player player;
		Vector2 moveInput;
		Vector2 previousMoveInput;
		public string moveXAxisName;
		public string moveYAxisName;
		public bool canControl;
		public Rigidbody2DController controllerToSwitchTo;
		bool isSwitching;
		public int inputterId;
		public SpriteRenderer controllingIndicator;
		public string switchControlButtonName;
		public float radius;
		[HideInInspector]
		public Vector2 extraVelocity;
		public bool PauseWhileUnfocused
		{
			get
			{
				return true;
			}
		}

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (rigid == null)
					rigid = GetComponent<Rigidbody2D>();
				if (distanceJoint == null)
					distanceJoint = GetComponent<DistanceJoint2D>();
				if (spriteRenderer == null)
					spriteRenderer = GetComponent<SpriteRenderer>();
				return;
			}
#endif
			base.Awake ();
			if (canControl)
				controllingIndicator.color = controllingIndicator.color.MultiplyAlpha(4);
		}

		public virtual void DoUpdate ()
		{
			if (Player.CanSwitchControl)
				HandleControlSwitching ();
			HandleMovement ();
		}

		public virtual void HandleControlSwitching ()
		{
			if (InputManager.inputters[inputterId].GetButtonDown(switchControlButtonName) && canControl)
			{
				canControl = false;
				isSwitching = true;
			}
			else if (InputManager.inputters[inputterId].GetButtonUp(switchControlButtonName) && isSwitching)
			{
				isSwitching = false;
				controllerToSwitchTo.canControl = true;
				controllingIndicator.color = controllingIndicator.color.DivideAlpha(4);
				controllerToSwitchTo.controllingIndicator.color = controllerToSwitchTo.controllingIndicator.color.MultiplyAlpha(4);
			}
		}

		public virtual void HandleMovement ()
		{
			if (canControl)
				moveInput = InputManager.inputters[inputterId].GetAxis2D(moveXAxisName, moveYAxisName);
			else
				moveInput = previousMoveInput;
			Move (moveInput);
			previousMoveInput = moveInput;
		}

		public virtual void Move (Vector2 move)
		{
			move = Vector2.ClampMagnitude(move, 1);
			rigid.velocity = move * moveSpeed + extraVelocity;
		}
    }
}