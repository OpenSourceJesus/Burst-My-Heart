using Extensions;
using UnityEngine;

namespace BMH
{
	public class GameCamera  : CameraScript
	{
		public bool inFirstDimension;

		public virtual void DoUpdate ()
		{
			HandlePosition ();
		}

		public override void HandlePosition ()
		{
			Vector2 bodyPosition = HumanPlayer.Instance.BodyPosition;
			// int dimensionOffsetMultiplier = 1;
			// if (HumanPlayer.instance.body.inFirstDimension != inFirstDimension)
			// 	dimensionOffsetMultiplier = -1;
			// if (HumanPlayer.instance.body.inFirstDimension)
			// 	bodyPosition -= GameManager.instance.dimensionOffset * dimensionOffsetMultiplier;
			// else
			// 	bodyPosition += GameManager.instance.dimensionOffset * dimensionOffsetMultiplier;
			Vector2 weaponPosition = HumanPlayer.instance.WeaponPosition;
			// dimensionOffsetMultiplier = 1;
			// if (HumanPlayer.instance.weapon.inFirstDimension != inFirstDimension)
			// 	dimensionOffsetMultiplier = -1;
			// if (HumanPlayer.instance.weapon.inFirstDimension)
			// 	weaponPosition -= GameManager.instance.dimensionOffset * dimensionOffsetMultiplier;
			// else
			// 	weaponPosition += GameManager.instance.dimensionOffset * dimensionOffsetMultiplier;
			trs.position = ((bodyPosition + weaponPosition) / 2).SetZ(trs.position.z);
			base.HandlePosition ();
		}
	}
}