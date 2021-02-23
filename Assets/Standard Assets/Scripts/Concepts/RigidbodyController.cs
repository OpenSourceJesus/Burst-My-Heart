using UnityEngine;
using Extensions;

namespace BMH
{
	//[ExecuteAlways]
	// [RequireComponent(typeof(Rigidbody2D))]
	[DisallowMultipleComponent]
    public class RigidbodyController : SingletonMonoBehaviour<RigidbodyController>, IUpdatable
    {
		public Transform trs;
		public Rigidbody rigid;
		public Collider collider;
		public MeshRenderer meshRenderer;
		public float moveSpeed;
		// public DistanceJoint distanceJoint;
		public Player player;
		Vector2 moveInput;
		Vector2 previousMoveInput;
		public string moveXAxisName;
		public string moveYAxisName;
		public bool canControl;
		public RigidbodyController controllerToSwitchTo;
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
					rigid = GetComponent<Rigidbody>();
				// if (distanceJoint == null)
				// 	distanceJoint = GetComponent<DistanceJoint2D>();
				if (meshRenderer == null)
					meshRenderer = GetComponent<MeshRenderer>();
				return;
			}
#endif
			base.Awake ();
			if (canControl)
				controllingIndicator.color = controllingIndicator.color.MultiplyAlpha(4);
			if (!inFirstDimension)
				SwitchToSecondDimension ();
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
				SwitchDimensions ();
		}

		public virtual void SwitchDimensions ()
		{
			isSwitching = false;
			inFirstDimension = !inFirstDimension;
			if (inFirstDimension)
				SwitchToFirstDimension ();
			else
				SwitchToSecondDimension ();
		}

		void SwitchToFirstDimension ()
		{
			OnSwitchDimensions ();
			trs.position = trs.position.SetZ(GameManager.instance.firstDimensionZPosition);
			meshRenderer.material.color = GameManager.instance.firstDimensionColor.SetAlpha(meshRenderer.material.color.a);
			// trs.position -= (Vector3) GameManager.instance.dimensionOffset;
		}

		void SwitchToSecondDimension ()
		{
			OnSwitchDimensions ();
			trs.position = trs.position.SetZ(GameManager.instance.secondDimensionZPosition);
			meshRenderer.material.color = GameManager.instance.secondDimensionColor.SetAlpha(meshRenderer.material.color.a);
			// trs.position += (Vector3) GameManager.instance.dimensionOffset;
		}

		void OnSwitchDimensions ()
		{
			// if (player.body.inFirstDimension != player.weapon.inFirstDimension)
			// 	distanceJoint.distance += GameManager.instance.dimensionDistance;
			// else
			// 	distanceJoint.distance -= GameManager.instance.dimensionDistance;
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