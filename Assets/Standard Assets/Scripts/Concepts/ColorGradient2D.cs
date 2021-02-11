using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Extensions;
using BMH;
using Random = UnityEngine.Random;

[Serializable]
public class ColorGradient2D
{
	public const int MAX_GRADIENT_KEYS = 8;
	public Gradient[] xGradients = new Gradient[0];
	public Gradient[] yGradients = new Gradient[0];

	public virtual Color Evaluate (Vector2 position)
	{
		Color output;
		int xGradientIndex = Mathf.RoundToInt((xGradients.Length - 1) * position.x);
		Gradient xGradient = xGradients[xGradientIndex];
		float xTimeToEvaluate = (1 - xGradientIndex / xGradients.Length) * position.x + xGradientIndex / xGradients.Length;
		Color xColor = xGradient.Evaluate(xTimeToEvaluate);
		int yGradientIndex = Mathf.RoundToInt((yGradients.Length - 1) * position.y);
		Gradient yGradient = yGradients[yGradientIndex];
		float yTimeToEvaluate = (1 - yGradientIndex / yGradients.Length) * position.y + yGradientIndex / yGradients.Length;
		Color yColor = yGradient.Evaluate(yTimeToEvaluate);
		float lerpAmount = xColor.a / yColor.a;
		if (xColor.a > yColor.a)
			lerpAmount = 1f / lerpAmount;
		xColor = xColor.SetAlpha(1);
		yColor = yColor.SetAlpha(1);
		output = Color.Lerp(xColor, yColor, lerpAmount);
		return output;
	}

	public virtual Sprite MakeSprite (Vector2Int textureSize, float pixelsPerUnit)
	{
		Texture2D texture = (Texture2D) GameManager.Clone(Texture2D.whiteTexture);
		texture.Resize(textureSize.x, textureSize.y);
		for (int x = 0; x < textureSize.x; x ++)
		{
			for (int y = 0; y < textureSize.y; y ++)
				texture.SetPixel(x, y, Evaluate(new Vector2(x, y).Divide(new Vector2(textureSize.x, textureSize.y))));
		}
		texture.Apply();
		return Sprite.Create(texture, Rect.MinMaxRect(0, 0, textureSize.x, textureSize.y), Vector2.one / 2, pixelsPerUnit);
	}

	public static ColorGradient2D GenerateRandom (int xColorKeys, int xAlphaKeys, int yColorKeys, int yAlphaKeys)
	{
		ColorGradient2D output = new ColorGradient2D();
		Gradient gradient;
		GradientColorKey colorKey;
		GradientColorKey[] colorKeys;
		Gradient previousGradient;
		int remainderKeyCount = xColorKeys;
		while (remainderKeyCount >= MAX_GRADIENT_KEYS)
			remainderKeyCount -= MAX_GRADIENT_KEYS;
		for (int i = 0; i < xColorKeys / MAX_GRADIENT_KEYS; i ++)
		{
			gradient = new Gradient();
			colorKeys = new GradientColorKey[MAX_GRADIENT_KEYS];
			for (int i2 = 0; i2 < MAX_GRADIENT_KEYS; i2 ++)
			{
				colorKey = new GradientColorKey();
				if (i2 == 0 && output.xGradients.Length > 0)
				{
					colorKey.time = 0;
					previousGradient = output.xGradients[output.xGradients.Length - 1];
					colorKey.color = previousGradient.colorKeys[previousGradient.colorKeys.Length - 1].color;
				}
				else
				{
					if (i2 == MAX_GRADIENT_KEYS - 1)
						colorKey.time = 1;
					else
						colorKey.time = Random.value;
					colorKey.color = ColorExtensions.RandomColor();
				}
				colorKeys[i2] = colorKey;
			}
			gradient.SetKeys(colorKeys, gradient.alphaKeys);
			output.xGradients = output.xGradients.Add(gradient);
		}
		if (remainderKeyCount > 0)
		{
			gradient = new Gradient();
			colorKeys = new GradientColorKey[remainderKeyCount];
			for (int i = 0; i < remainderKeyCount; i ++)
			{
				colorKey = new GradientColorKey();
				if (i == 0 && output.xGradients.Length > 0)
				{
					colorKey.time = 0;
					previousGradient = output.xGradients[output.xGradients.Length - 1];
					colorKey.color = previousGradient.colorKeys[previousGradient.colorKeys.Length - 1].color;
				}
				else
				{
					if (i == MAX_GRADIENT_KEYS - 1)
						colorKey.time = 1;
					else
						colorKey.time = Random.value;
					colorKey.color = ColorExtensions.RandomColor();
				}
				colorKeys[i] = colorKey;
			}
			gradient.SetKeys(colorKeys, gradient.alphaKeys);
			output.xGradients = output.xGradients.Add(gradient);
		}

		remainderKeyCount = yColorKeys;
		while (remainderKeyCount >= MAX_GRADIENT_KEYS)
			remainderKeyCount -= MAX_GRADIENT_KEYS;
		for (int i = 0; i < yColorKeys / MAX_GRADIENT_KEYS; i ++)
		{
			gradient = new Gradient();
			colorKeys = new GradientColorKey[MAX_GRADIENT_KEYS];
			for (int i2 = 0; i2 < MAX_GRADIENT_KEYS; i2 ++)
			{
				colorKey = new GradientColorKey();
				if (i2 == 0 && output.yGradients.Length > 0)
				{
					colorKey.time = 0;
					previousGradient = output.yGradients[output.yGradients.Length - 1];
					colorKey.color = previousGradient.colorKeys[previousGradient.colorKeys.Length - 1].color;
				}
				else
				{
					if (i2 == MAX_GRADIENT_KEYS - 1)
						colorKey.time = 1;
					else
						colorKey.time = Random.value;
					colorKey.color = ColorExtensions.RandomColor();
				}
				colorKeys[i2] = colorKey;
			}
			gradient.SetKeys(colorKeys, gradient.alphaKeys);
			output.yGradients = output.yGradients.Add(gradient);
		}
		if (remainderKeyCount > 0)
		{
			gradient = new Gradient();
			colorKeys = new GradientColorKey[remainderKeyCount];
			for (int i = 0; i < remainderKeyCount; i ++)
			{
				colorKey = new GradientColorKey();
				if (i == 0 && output.yGradients.Length > 0)
				{
					colorKey.time = 0;
					previousGradient = output.yGradients[output.yGradients.Length - 1];
					colorKey.color = previousGradient.colorKeys[previousGradient.colorKeys.Length - 1].color;
				}
				else
				{
					if (i == MAX_GRADIENT_KEYS - 1)
						colorKey.time = 1;
					else
						colorKey.time = Random.value;
					colorKey.color = ColorExtensions.RandomColor();
				}
				colorKeys[i] = colorKey;
			}
			gradient.SetKeys(colorKeys, gradient.alphaKeys);
			output.yGradients = output.yGradients.Add(gradient);
		}

		GradientAlphaKey alphaKey;
		GradientAlphaKey[] alphaKeys;
		int gradientIndex = 0;
		while (remainderKeyCount >= MAX_GRADIENT_KEYS)
			remainderKeyCount -= MAX_GRADIENT_KEYS;
		for (int i = 0; i < xAlphaKeys / MAX_GRADIENT_KEYS; i ++)
		{
			gradient = output.xGradients[gradientIndex];
			alphaKeys = new GradientAlphaKey[MAX_GRADIENT_KEYS];
			for (int i2 = 0; i2 < MAX_GRADIENT_KEYS; i2 ++)
			{
				alphaKey = new GradientAlphaKey();
				if (i2 == 0 && gradientIndex > 0)
				{
					alphaKey.time = 0;
					previousGradient = output.xGradients[gradientIndex - 1];
					alphaKey.alpha = previousGradient.alphaKeys[previousGradient.alphaKeys.Length - 1].alpha;
				}
				else
				{
					if (i2 == MAX_GRADIENT_KEYS - 1)
						alphaKey.time = 1;
					else
						alphaKey.time = Random.value;
					alphaKey.alpha = Random.value;
				}
				alphaKeys[i2] = alphaKey;
			}
			gradient.SetKeys(gradient.colorKeys, alphaKeys);
			output.xGradients[gradientIndex] = gradient;
			gradientIndex ++;
		}
		if (remainderKeyCount > 0)
		{
			gradient = output.xGradients[gradientIndex];
			alphaKeys = new GradientAlphaKey[remainderKeyCount];
			for (int i = 0; i < remainderKeyCount; i ++)
			{
				alphaKey = new GradientAlphaKey();
				if (i == 0 && gradientIndex > 0)
				{
					alphaKey.time = 0;
					previousGradient = output.xGradients[gradientIndex - 1];
					alphaKey.alpha = previousGradient.alphaKeys[previousGradient.alphaKeys.Length - 1].alpha;
				}
				else
				{
					if (i == MAX_GRADIENT_KEYS - 1)
						alphaKey.time = 1;
					else
						alphaKey.time = Random.value;
					alphaKey.alpha = Random.value;
				}
				alphaKeys[i] = alphaKey;
			}
			gradient.SetKeys(gradient.colorKeys, alphaKeys);
			output.xGradients[gradientIndex] = gradient;
			gradientIndex ++;
		}

		gradientIndex = 0;
		remainderKeyCount = yAlphaKeys;
		while (remainderKeyCount >= MAX_GRADIENT_KEYS)
			remainderKeyCount -= MAX_GRADIENT_KEYS;
		for (int i = 0; i < yAlphaKeys / MAX_GRADIENT_KEYS; i ++)
		{
			gradient = output.yGradients[gradientIndex];
			alphaKeys = new GradientAlphaKey[MAX_GRADIENT_KEYS];
			for (int i2 = 0; i2 < MAX_GRADIENT_KEYS; i2 ++)
			{
				alphaKey = new GradientAlphaKey();
				if (i2 == 0 && gradientIndex > 0)
				{
					alphaKey.time = 0;
					previousGradient = output.yGradients[gradientIndex - 1];
					alphaKey.alpha = previousGradient.alphaKeys[previousGradient.alphaKeys.Length - 1].alpha;
				}
				else
				{
					if (i2 == MAX_GRADIENT_KEYS - 1)
						alphaKey.time = 1;
					else
						alphaKey.time = Random.value;
					alphaKey.alpha = Random.value;
				}
				alphaKeys[i2] = alphaKey;
			}
			gradient.SetKeys(gradient.colorKeys, alphaKeys);
			output.yGradients[gradientIndex] = gradient;
			gradientIndex ++;
		}
		if (remainderKeyCount > 0)
		{
			gradient = output.yGradients[gradientIndex];
			alphaKeys = new GradientAlphaKey[remainderKeyCount];
			for (int i = 0; i < remainderKeyCount; i ++)
			{
				alphaKey = new GradientAlphaKey();
				if (i == 0 && gradientIndex > 0)
				{
					alphaKey.time = 0;
					previousGradient = output.yGradients[gradientIndex - 1];
					alphaKey.alpha = previousGradient.alphaKeys[previousGradient.alphaKeys.Length - 1].alpha;
				}
				else
				{
					if (i == MAX_GRADIENT_KEYS - 1)
						alphaKey.time = 1;
					else
						alphaKey.time = Random.value;
					alphaKey.alpha = Random.value;
				}
				alphaKeys[i] = alphaKey;
			}
			gradient.SetKeys(gradient.colorKeys, alphaKeys);
			output.yGradients[gradientIndex] = gradient;
			gradientIndex ++;
		}
		return output;
	}
}
