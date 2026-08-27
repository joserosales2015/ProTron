using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Math
{
	public struct Vector3f
	{
		public float X;
		public float Y;
		public float Z;

		public static Vector3f Zero => new(0, 0, 0);

		public static Vector3f One => new(1, 1, 1);

		public static Vector3f Up => new(0, 1, 0);

		public static Vector3f Down => new(0, -1, 0);

		public static Vector3f Left => new(-1, 0, 0);

		public static Vector3f Right => new(1, 0, 0);

		public static Vector3f Forward => new(0, 0, 1);

		public static Vector3f Back => new(0, 0, -1);

		public Vector3f(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		// Suma de vectores
		public static Vector3f operator +(Vector3f a, Vector3f b)
		{
			return new Vector3f(
				a.X + b.X,
				a.Y + b.Y,
				a.Z + b.Z);
		}

		// Resta de vectores
		public static Vector3f operator -(Vector3f a, Vector3f b)
		{
			return new Vector3f(
				a.X - b.X,
				a.Y - b.Y,
				a.Z - b.Z);
		}

		// Devuelve un vector en la dirección opuesta
		public static Vector3f operator -(Vector3f v)
		{
			return new Vector3f(
				-v.X,
				-v.Y,
				-v.Z);
		}

		// Multiplicación por un escalar
		public static Vector3f operator *(Vector3f v, float s)
		{
			return new Vector3f(
				v.X * s,
				v.Y * s,
				v.Z * s);
		}

		// Multiplicación de escalar por un vector
		public static Vector3f operator *(float s, Vector3f v)
		{
			return v * s;
		}

		// División por un escalar
		public static Vector3f operator /(Vector3f v, float s)
		{
			return new Vector3f(
				v.X / s,
				v.Y / s,
				v.Z / s);
		}

		// Comparación de vectores
		public static bool operator ==(Vector3f a, Vector3f b)
		{
			return
				a.X == b.X &&
				a.Y == b.Y &&
				a.Z == b.Z;
		}

		// Comparación de vectores
		public static bool operator !=(Vector3f a, Vector3f b)
		{
			return !(a == b);
		}

		// Comparación de vectores con tolerancia
		public bool Equals(Vector3f other, float epsilon = 0.0001f)
		{
			return
				MathF.Abs(X - other.X) < epsilon &&
				MathF.Abs(Y - other.Y) < epsilon &&
				MathF.Abs(Z - other.Z) < epsilon;
		}

		// Sobrescribir Equals para permitir la comparación de vectores
		public override bool Equals(object? obj)
		{
			if (obj is not Vector3f other)
				return false;

			return this == other;
		}

		// Sobrescribir GetHashCode para permitir la comparación de vectores
		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z);
		}

		public Vector2 ToVector2()
		{
			return new Vector2(X, Y);
		}

		public Vector4f ToVector4()
		{
			return new Vector4f(X, Y, Z, 1f);
		}

		public Vector4f ToVector4(float w = 1f)
		{
			return new Vector4f(X, Y, Z, w);
		}

		public static Vector3f Lerp(Vector3f a, Vector3f b, float t)
		{
			t = System.Math.Clamp(t, 0f, 1f);

			return a + (b - a) * t;
		}
		public static Vector3f LerpUnclamped(Vector3f a, Vector3f b, float t)
		{
			return a + (b - a) * t;
		}

		public float Length
		{
			get => MathF.Sqrt(X * X + Y * Y + Z * Z);
		}

		public float LengthSquared
		{
			get => X * X + Y * Y + Z * Z;
		}

		public Vector3f Normalized()
		{
			float len = Length;

			if (len < 0.000001f)
				return new Vector3f();

			return this / len;
		}

		public void Normalize()
		{
			float len = Length;

			if (len < 0.000001f)
				return;

			X /= len;
			Y /= len;
			Z /= len;
		}

		public static float Dot(Vector3f a, Vector3f b)
		{
			return
				a.X * b.X +
				a.Y * b.Y +
				a.Z * b.Z;
		}

		public static Vector3f Cross(Vector3f a, Vector3f b)
		{
			return new Vector3f(
				a.Y * b.Z - a.Z * b.Y,
				a.Z * b.X - a.X * b.Z,
				a.X * b.Y - a.Y * b.X
			);
		}

		public static float Distance(Vector3f a, Vector3f b)
		{
			return (a - b).Length;
		}

		public static float DistanceSquared(Vector3f a, Vector3f b)
		{
			return (a - b).LengthSquared;
		}
	}
}
