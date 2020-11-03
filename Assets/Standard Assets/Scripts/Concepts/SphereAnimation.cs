using UnityEngine;
using Extensions;
using BMH;

public class SphereAnimation : SingletonMonoBehaviour<SphereAnimation>, IUpdatable
{
	public bool PauseWhileUnfocused
	{
		get
		{
			return true;
		}
	}
	public Transform trs;
	public float missileMapBounds;
	public float missileTurnRate;
	public float missileSpeed;
	Vector2 missilePosition;
	Vector2 missileDestination;
	Vector2 missileVelocity;
	float missileDistToDestSqr;
	bool missileHasReducedDistToDest;
	
	public override void Awake ()
	{
		base.Awake ();
		missileVelocity = Random.insideUnitCircle.normalized * missileSpeed;
		missileDestination = Random.insideUnitCircle * missileMapBounds;
		GameManager.updatables = GameManager.updatables.Add(this);
	}

	void OnDestroy ()
	{
		GameManager.updatables = GameManager.updatables.Remove(this);
	}

	public virtual void DoUpdate ()
	{
		float prevMissileDistToDestSqr = (missilePosition - missileDestination).sqrMagnitude;
		float missileIdealTurnAmount = Vector2.Angle(missileVelocity, missileDestination - missilePosition);
		float missileTurnAmount = Mathf.Clamp(missileIdealTurnAmount, -missileTurnRate * Time.deltaTime, missileTurnRate * Time.deltaTime);
		missileVelocity = VectorExtensions.Rotate(missileVelocity, missileTurnAmount);
		missileVelocity = missileVelocity.normalized * missileSpeed * Time.deltaTime;
		missilePosition += missileVelocity;
		float missileDistToDestSqr = (missilePosition - missileDestination).sqrMagnitude;
		if (missileHasReducedDistToDest && missileDistToDestSqr > prevMissileDistToDestSqr)
		{
			missileDestination = Random.insideUnitCircle * missileMapBounds;
			missileHasReducedDistToDest = false;
		}
		else if (missileDistToDestSqr < prevMissileDistToDestSqr)
			missileHasReducedDistToDest = true;
		trs.RotateAround(trs.position, trs.forward, missileVelocity.x);
		trs.RotateAround(trs.position, trs.right, missileVelocity.y);
	}
}
