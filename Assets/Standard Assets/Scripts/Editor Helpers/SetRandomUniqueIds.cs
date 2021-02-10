#if UNITY_EDITOR
using UnityEngine;

namespace BMH
{
	public class SetRandomUniqueIds : EditorScript
	{
		public SaveAndLoadObject[] saveAndLoadObjects = new SaveAndLoadObject[0];

		public override void Do ()
		{
			foreach (SaveAndLoadObject saveAndLoadObject in saveAndLoadObjects)
				saveAndLoadObject.uniqueId = Random.Range(int.MinValue, int.MaxValue);
			saveAndLoadObjects = new SaveAndLoadObject[0];
		}
	}
}
#else
using UnityEngine;

namespace BMH
{
	public class SetRandomUniqueIds : MonoBehaviour
	{
	}
}
#endif