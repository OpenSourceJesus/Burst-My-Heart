#if UNITY_EDITOR
using UnityEngine;

namespace BMH
{
	public class Test : EditorScript
	{
		public override void Do ()
		{
			GameObject[] gos = FindObjectsOfType<GameObject>();
			foreach (GameObject go in gos)
			{
				if (go.name == "GameObject" && Mathf.Approximately(go.GetComponent<Transform>().localScale.x, 1))
				{
					Component[] components = go.GetComponents<Component>();
					foreach (Component component in components)
						Destroy(component);
					go.AddComponent<FaceCamera>();
				}
			}
		}
	}
}
#endif