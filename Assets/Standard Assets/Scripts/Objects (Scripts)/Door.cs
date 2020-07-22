using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Extensions;
using Ferr;

namespace BMH
{
	[ExecuteAlways]
	public class Door : MonoBehaviour
	{
		public int scoreToOpen;
		public Collider2D collider;
		public MeshRenderer terrainRenderer;
		[HideInInspector]
		public float closedColorAlpha;
		public float openColorAlpha;
		int playerRigidbodyControllersInMe;

		public virtual void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			closedColorAlpha = terrainRenderer.material.color.a;
		}

		public virtual void OnCollisionEnter2D (Collision2D coll)
		{
			if (coll.transform.root == GameManager.GetSingleton<HumanPlayer>().trs && RPG.instance.score >= scoreToOpen)
				Open ();
		}

		public virtual void OnTriggerEnter2D (Collider2D other)
		{
			if (other.GetComponent<Transform>().root == GameManager.GetSingleton<HumanPlayer>().trs && RPG.instance.score >= scoreToOpen)
			{
				playerRigidbodyControllersInMe ++;
				print(playerRigidbodyControllersInMe);
			}
		}

		public virtual void OnTriggerExit2D (Collider2D other)
		{
			if (other.GetComponent<Transform>().root == GameManager.GetSingleton<HumanPlayer>().trs && RPG.instance.score >= scoreToOpen)
			{
				playerRigidbodyControllersInMe --;
				print(playerRigidbodyControllersInMe);
				if (playerRigidbodyControllersInMe == 0)
					Close ();
			}
		}

		public virtual void Open ()
		{
			collider.enabled = false;
			terrainRenderer.material.color = terrainRenderer.material.color.SetAlpha(openColorAlpha);
		}

		public virtual void Close ()
		{
			collider.enabled = true;
			terrainRenderer.material.color = terrainRenderer.material.color.SetAlpha(closedColorAlpha);
		}
	}
}