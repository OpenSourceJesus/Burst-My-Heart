using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace BMH
{
	public class AI : Player
	{
		Vector2 defenseVector;
		Vector2 bodyMove;
		Vector2 weaponMove;
		float imprecision;
		public FloatRange imprecisionRange;
		public Timer changeImprecisionTimer;
		public FloatRange changeImprecisionTimeIntervalRange;
		bool shouldSwitchPositions;
		public float minAcceptableLoseDistance;
		public float minLoseDistToConsiderFacingForward;
		float loseDistance;
		public float minAcceptableFaceForwardDifference;
		float potentialFaceForwardAmount;
		float faceForwardAmount;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
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
			imprecision = Random.Range(imprecisionRange.min, imprecisionRange.max);
			changeImprecisionTimer.duration = Random.Range(changeImprecisionTimeIntervalRange.min, changeImprecisionTimeIntervalRange.max);
			changeImprecisionTimer.Reset ();
			changeImprecisionTimer.Start ();
		}
		
		public virtual void Think ()
		{
			if (CanSwitchPositions)
			{
				shouldSwitchPositions = (GetFaceForwardAmount() - owner.opponent.representative.GetFaceForwardAmount() < minAcceptableFaceForwardDifference && GetFaceForwardAmountAfterSwitchPositions() > GetFaceForwardAmount()) || (GetWinDistanceAfterSwitchPositions() < GetWinDistance() && GetWinDistanceAfterSwitchPositions() < owner.opponent.representative.GetWinDistance() && GetWinDistanceAfterSwitchPositions() < owner.opponent.representative.GetWinDistanceAfterSwitchPositions());
				if (shouldSwitchPositions)
					SwitchPositions ();
			}
			weaponMove = owner.opponent.representative.body.trs.position - weapon.trs.position;
			bodyMove = body.trs.position - owner.opponent.representative.weapon.trs.position;
			loseDistance = GetLoseDistanceAfterMove(bodyMove);
			if (loseDistance > minAcceptableLoseDistance)
			{
				bodyMove = weaponMove;
				if (loseDistance > minLoseDistToConsiderFacingForward)
				{
					defenseVector = (weapon.trs.position - body.trs.position).Rotate(90);
					potentialFaceForwardAmount = GetFaceForwardAmountAfterMove(-defenseVector, defenseVector);
					faceForwardAmount = GetFaceForwardAmount();
					if (faceForwardAmount < potentialFaceForwardAmount)
					{
						if (potentialFaceForwardAmount < GetFaceForwardAmountAfterMove(defenseVector, -defenseVector))
							defenseVector *= -1;
						bodyMove = -defenseVector;
						weaponMove = defenseVector;
					}
					else if (faceForwardAmount < GetFaceForwardAmountAfterMove(defenseVector, -defenseVector))
					{
						bodyMove = defenseVector;
						weaponMove = -defenseVector;
					}
				}
			}
			bodyMove = bodyMove.normalized.Rotate(imprecision);
			weaponMove = weaponMove.normalized.Rotate(imprecision);
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
	}
}