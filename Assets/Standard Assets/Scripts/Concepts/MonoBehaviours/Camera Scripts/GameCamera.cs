using Extensions;

namespace BMH
{
	public class GameCamera  : CameraScript
	{
		public virtual void DoUpdate ()
		{
			HandlePosition ();
		}

		public override void HandlePosition ()
		{
			trs.position = ((HumanPlayer.Instance.BodyPosition + HumanPlayer.Instance.WeaponPosition) / 2).SetZ(trs.position.z);
			base.HandlePosition ();
		}
	}
}