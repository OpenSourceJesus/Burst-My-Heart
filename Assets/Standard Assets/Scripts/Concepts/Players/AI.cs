using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class AI : Player
	{
		public new static AI instance;
		Vector2 offenseVector;
		Vector2 defenseVector;
		public float minAcceptableNormalizedLength;
		public float minAcceptableFaceForwardDifference;
		Vector2 bodyMove;
		Vector2 weaponMove;
		float imprecision;
		public float minImprecision;
		public float maxImprecision;
		public Timer changeImprecisionTimer;
		public float minChangeImprecisionTimeInterval;
		public float maxChangeImprecisionTimeInterval;
		Vector2 offset;
		bool shouldSwitch;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
			instance = this;
			body.enabled = false;
			body.Awake ();
			weapon.enabled = false;
			weapon.Awake ();
			changeImprecisionTimer.onFinished += ChangeImprecision;
			changeImprecisionTimer.Start ();
		}
		
		public override void DoUpdate ()
		{
			base.DoUpdate ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (GameManager.paused)
				return;
			Think ();
			Act ();
		}

		public virtual void ChangeImprecision (params object[] args)
		{
			imprecision = Random.Range(minImprecision, maxImprecision);
			offset = Random.insideUnitCircle * imprecision;
			changeImprecisionTimer.duration = Random.Range(minChangeImprecisionTimeInterval, maxChangeImprecisionTimeInterval);
			changeImprecisionTimer.Reset ();
			changeImprecisionTimer.Start ();
		}
		
		public virtual void Think ()
		{
			if (CanSwitchPositions)
			{
				shouldSwitch = (owner.opponent.representative.GetFaceForwardAmount() - GetFaceForwardAmount() > minAcceptableFaceForwardDifference && GetFaceForwardAmountAfterSwitch() > GetFaceForwardAmount()) || (GetWinDistanceAfterSwitch() < GetWinDistance() && GetWinDistanceAfterSwitch() < owner.opponent.representative.GetWinDistance() && GetWinDistanceAfterSwitch() < owner.opponent.representative.GetWinDistanceAfterSwitch());
				if (shouldSwitch)
					Switch ();
			}
			defenseVector = weapon.trs.position - body.trs.position;
			if (defenseVector.magnitude < maxBodyToWeaponDist * minAcceptableNormalizedLength && GetWinDistanceAfterMove(defenseVector) < GetWinDistance())
			{
				bodyMove = -defenseVector;
				weaponMove = defenseVector;
			}
			else
			{
				offenseVector = owner.opponent.representative.body.trs.position - weapon.trs.position;
				offenseVector += offset;
				defenseVector = defenseVector.Rotate(90);
				if (GetFaceForwardAmount() < minAcceptableFaceForwardDifference)
				{
					if (GetFaceForwardAmountAfterMove(-defenseVector, defenseVector) < GetFaceForwardAmountAfterMove(defenseVector, -defenseVector))
						defenseVector *= -1;
					bodyMove = -defenseVector;
					weaponMove = defenseVector;
				}
				else
				{
					bodyMove = offenseVector;
					weaponMove = offenseVector;
				}
			}
		}

		public virtual void Act ()
		{
			body.Move (bodyMove);
			weapon.Move (weaponMove);
		}

		public override void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDestroy ();
			changeImprecisionTimer.Stop ();
			changeImprecisionTimer.onFinished -= ChangeImprecision;
		}
		
		public new static AI GetInstance ()
		{
			if (instance == null)
				instance = FindObjectOfType<AI>();
			return instance;
		}
	}
}