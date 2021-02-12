using UnityEngine;
using Extensions;

namespace BMH
{
	//[ExecuteAlways]
	[RequireComponent(typeof(Rigidbody2D))]
	[DisallowMultipleComponent]
    public class Rigidbody2DController : SingletonMonoBehaviour<Rigidbody2DController>, IUpdatable
    {
		public Transform trs;
		public Rigidbody2D rigid;
		public new Collider2D collider;
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
		public string switchDimensionsButtonName;
		public float radius;
		[HideInInspector]
		public Vector2 extraVelocity;
		public bool inFirstDimension = true;
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
			if (Player.CanSwitchDimensions)
			{
				if (inFirstDimension)
					spriteRenderer.color = GameManager.instance.firstDimensionColor.SetAlpha(spriteRenderer.color.a);
				else
					spriteRenderer.color = GameManager.instance.secondDimensionColor.SetAlpha(spriteRenderer.color.a);
			}
		}

		public virtual void DoUpdate ()
		{
			if (inputterId == -1)
				return;
			if (Player.CanSwitchControl && controllerToSwitchTo != null)
				HandleControlSwitching ();
			if (Player.CanSwitchDimensions)
				HandleDimensionSwitching ();
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
				SwitchControl ();
		}

		public virtual void HandleDimensionSwitching ()
		{
			if (InputManager.inputters[inputterId].GetButtonDown(switchDimensionsButtonName) && canControl)
			{
				canControl = false;
				isSwitching = true;
			}
			else if (InputManager.inputters[inputterId].GetButtonUp(switchDimensionsButtonName) && isSwitching)
				SwitchControl ();
		}

		public virtual void SwitchDimensions ()
		{
			isSwitching = false;
			trs.position = trs.position.SetZ(1 - trs.position.z);
			inFirstDimension = !inFirstDimension;
			if (inFirstDimension)
				spriteRenderer.color = GameManager.instance.firstDimensionColor.SetAlpha(spriteRenderer.color.a);
			else
				spriteRenderer.color = GameManager.instance.secondDimensionColor.SetAlpha(spriteRenderer.color.a);
		}

		public virtual void SwitchControl ()
		{
			isSwitching = false;
			controllerToSwitchTo.canControl = true;
			controllingIndicator.color = controllingIndicator.color.DivideAlpha(4);
			controllerToSwitchTo.controllingIndicator.color = controllerToSwitchTo.controllingIndicator.color.MultiplyAlpha(4);
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