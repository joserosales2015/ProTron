using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Math
{
	public struct Vector4f
	{
		public float X;
		public float Y;
		public float Z;
		public float W;

		public Vector4f(float x, float y, float z, float w = 1f)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public static Vector4f operator +(Vector4f a, Vector4f b)
		{
			return new Vector4f(
				a.X + b.X,
				a.Y + b.Y,
				a.Z + b.Z,
				a.W + b.W);
		}

		public static Vector4f operator -(Vector4f a, Vector4f b)
		{
			return new Vector4f(
				a.X - b.X,
				a.Y - b.Y,
				a.Z - b.Z,
				a.W - b.W);
		}

		public static Vector4f operator *(Vector4f v, float s)
		{
			return new Vector4f(
				v.X * s,
				v.Y * s,
				v.Z * s,
				v.W * s);
		}

		public Vector3f ToVector3()
		{
			return new Vector3f(X, Y, Z);
		}

		public float Length()
		{
			return MathF.Sqrt(
				X * X +
				Y * Y +
				Z * Z +
				W * W);
		}

		public Vector4f Normalize()
		{
			float len = Length();

			if (len == 0)
				return this;

			return new Vector4f(
				X / len,
				Y / len,
				Z / len,
				W / len);
		}

		public Vector3f PerspectiveDivide()
		{
			if (MathF.Abs(W) < 0.00001f)
				return new Vector3f(X, Y, Z);

			return new Vector3f(
				X / W,
				Y / W,
				Z / W);
		}
	}
}
