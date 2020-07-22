using System;
using System.Windows;

namespace BMHPlugin
{
	public static class VectorExtensions
	{
		public static Vector FromFacingAngle (float angle)
		{
			angle *= MathExtensions.Deg2Rad;
			Vector output = new Vector(Math.Cos(angle), Math.Sin(angle));
			output.Normalize();
			return output;
		}

		public static float GetFacingAngle (this Vector v)
		{
			v.Normalize();
			return (float) Math.Atan2(v.Y, v.X) * MathExtensions.Rad2Deg;
		}

		public static float Distance (Vector v1, Vector v2)
		{
			return (float) Vector.Subtract(v1, v2).Length;
		}
	}
}
