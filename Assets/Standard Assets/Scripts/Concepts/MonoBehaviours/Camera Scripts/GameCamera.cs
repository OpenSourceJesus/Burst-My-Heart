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
			trs.position = ((GameManager.GetSingleton<HumanPlayer>().BodyPosition + GameManager.GetSingleton<HumanPlayer>().WeaponPosition) / 2).SetZ(trs.position.z);
			base.HandlePosition ();
		}
	}
}