using UnityEngine;
using Extensions;

namespace BMH
{
	[ExecuteAlways]
	public class Area : SingletonMonoBehaviour<Area>
	{
		public Transform trs;
		public Renderer colorGradient2DRenderer;
		public ColorGradient2D colorGradient2D;
		public Vector2Int colorKeys;
		public Vector2Int alphaKeys;
		public Vector2Int textureSize;
		public float pixelsPerUnit;
		bool generateRandomGradient;
		
		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.Awake ();
			generateRandomGradient = colorGradient2D.xGradients.Length == 0;
			GameManager.singletons.Remove(GetType());
			GameManager.singletons.Add(GetType(), this);
		}

		public virtual void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (generateRandomGradient)
				colorGradient2D = ColorGradient2D.GenerateRandom(colorKeys.x, alphaKeys.x, colorKeys.y, alphaKeys.y);
			colorGradient2DRenderer.material.mainTexture = colorGradient2D.MakeSprite(textureSize, pixelsPerUnit).texture;
		}
	}
}
