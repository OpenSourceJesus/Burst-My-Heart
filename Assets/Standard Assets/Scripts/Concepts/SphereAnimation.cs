using UnityEngine;
using System.Collections;

public class SphereAnimation : MonoBehaviour
{
	public float rotaPerSecond;
	public float addToRotaPerSecond;
	public float maxRotaPerSecond;
	public float volativity;
	public Vector3 deltaRota;

    void Start()
    {
        if (deltaRota == Vector3.zero)
            deltaRota = Random.onUnitSphere;
    }

	void Update ()
	{
		rotaPerSecond = Mathf.Clamp(rotaPerSecond + addToRotaPerSecond * Time.deltaTime, 0, maxRotaPerSecond);
		deltaRota = Vector3.RotateTowards(deltaRota, Random.onUnitSphere, 180 * volativity * Mathf.Deg2Rad * Time.deltaTime, 0);
		transform.Rotate(deltaRota.normalized * rotaPerSecond * Time.deltaTime);
	}
}
