using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Math
{
	public struct Matrix4x4
	{
		public float M11, M12, M13, M14;
		public float M21, M22, M23, M24;
		public float M31, M32, M33, M34;
		public float M41, M42, M43, M44;

		public Matrix4x4()
		{

		}

		public static Matrix4x4 Identity()
		{
			return new Matrix4x4
			{
				M11 = 1,
				M22 = 1,
				M33 = 1,
				M44 = 1
			};
		}

		public static Matrix4x4 CreateTranslation(float x, float y, float z)
		{
			Matrix4x4 m = Identity();

			m.M14 = x;
			m.M24 = y;
			m.M34 = z;

			return m;
		}

		public static Matrix4x4 CreateScale(float x, float y, float z)
		{
			Matrix4x4 m = Identity();

			m.M11 = x;
			m.M22 = y;
			m.M33 = z;

			return m;
		}

		public static Matrix4x4 CreateRotationX(float angle)
		{
			Matrix4x4 m = Identity();

			float radians = angle * MathF.PI / 180f;

			float c = MathF.Cos(radians);
			float s = MathF.Sin(radians);

			m.M22 = c;
			m.M23 = -s;

			m.M32 = s;
			m.M33 = c;

			return m;
		}

		public static Matrix4x4 CreateRotationY(float angle)
		{
			Matrix4x4 m = Identity();

			float radians = angle * MathF.PI / 180f;

			float c = MathF.Cos(radians);
			float s = MathF.Sin(radians);

			m.M11 = c;
			m.M13 = s;

			m.M31 = -s;
			m.M33 = c;

			return m;
		}

		public static Matrix4x4 CreateRotationZ(float angle)
		{
			Matrix4x4 m = Identity();

			float radians = angle * MathF.PI / 180f;

			float c = MathF.Cos(radians);
			float s = MathF.Sin(radians);

			m.M11 = c;
			m.M12 = -s;

			m.M21 = s;
			m.M22 = c;

			return m;
		}

		public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
		{
			Matrix4x4 r = new Matrix4x4();

			r.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
			r.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
			r.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
			r.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;

			r.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
			r.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
			r.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
			r.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;

			r.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
			r.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
			r.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
			r.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;

			r.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
			r.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
			r.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
			r.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;

			return r;
		}

		public static Vector4f operator *(Matrix4x4 m, Vector4f v)
		{
			return new Vector4f(
				m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W,
				m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W,
				m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W,
				m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W
			);
		}
	}
}
